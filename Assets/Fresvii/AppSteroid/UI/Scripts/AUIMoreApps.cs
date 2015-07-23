using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMoreApps : MonoBehaviour
    {
        public AUIFrame frame;

        List<AUIAppsPreviewCell> previewCells = new List<AUIAppsPreviewCell>();

        public GameObject prfbPreviewCell;

        Fresvii.AppSteroid.Models.ListMeta previewListMeta;

        public AUIScrollViewContents previewContents;

        public GameObject previewLoadingSpinner;

        List<AUIRecommendedAppCell> recommendedAppCells = new List<AUIRecommendedAppCell>();

        public GameObject prfbRecommendedAppCell;

        Fresvii.AppSteroid.Models.ListMeta recommendedAppListMeta;

        public AUIScrollViewContents recommendedAppContents;

        public GameObject recommendedLoadingSpinner;

        IList<Fresvii.AppSteroid.Models.Video> previewVideos;

        public AUICommunityTopCommentCell[] commentCells;

        // Use this for initialization
        void OnEnable()
        {
            StartCoroutine(Init());

            if (previewCells.Count > 0)
            {
                previewContents.ReLayout();
            }

            if (recommendedAppCells.Count > 0)
            {
                recommendedAppContents.ReLayout();
            }
        }

        void OnDisable()
        {
            AUIManager.Instance.HideLoadingSpinner();
        }

        IEnumerator Init()
        {
            yield return 1;

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASAdvertisement.GetAppList(1, OnGetAppList);

            FASAdvertisement.GetVideoList(1, OnGetVideoList);

            FASAdvertisement.GetThreadList(10, OnGetThreadList);
        }

        void OnGetAppList(IList<Fresvii.AppSteroid.Models.App> apps, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null || this.enabled == false)
            {
                return;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            recommendedLoadingSpinner.SetActive(false);

            this.recommendedAppListMeta = meta;

            foreach (var app in apps)
            {
                var cell = recommendedAppCells.Find(x => x.AddApp.Id == app.Id);

                if (cell != null)
                {
                    cell.SetApp(app);

                    return;
                }

                var item = ((GameObject)Instantiate(prfbRecommendedAppCell)).GetComponent<RectTransform>();

                recommendedAppContents.AddItem(item);

                cell = item.GetComponent<AUIRecommendedAppCell>();

                cell.SetApp(app);

                cell.OnClickAppCell += GoToAppDetail;

                recommendedAppCells.Add(cell);
            }

            recommendedAppContents.ReLayout();
        }

        void OnGetVideoList(IList<Fresvii.AppSteroid.Models.Video> videos, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null || this.enabled == false)
            {
                return;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            previewLoadingSpinner.SetActive(false);

            this.previewListMeta = meta;

            this.previewVideos = videos;

            foreach (var video in videos)
            {
                var cell = previewCells.Find(x => x.Video.Id == video.Id);

                if (cell != null)
                {
                    cell.SetPreview(video);

                    return;
                }

                var item = ((GameObject)Instantiate(prfbPreviewCell)).GetComponent<RectTransform>();

                previewContents.AddItem(item);

                cell = item.GetComponent<AUIAppsPreviewCell>();

                cell.OnTapAppButtonAtVideoUI += GoToAppDetail;

                cell.SetPreview(video);

                previewCells.Add(cell);
            }

            previewContents.ReLayout();
        }

        void OnGetThreadList(IList<Fresvii.AppSteroid.Models.Thread> threads, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null)
            {
                return;
            }

            if (this.enabled == false)
            {
                return;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.CacheNotExists)
                    {
                        Debug.LogWarning(error.ToString());
                    }
                    else
                    {
                        Debug.LogError(error.ToString());
                    }
                }

                return;
            }
            
            int index = 0;

            foreach (var thread in threads)
            {
                if (string.IsNullOrEmpty(thread.Comment.Text))
                    continue;

                thread.Comment.App = thread.App;

                commentCells[index].gameObject.SetActive(true);

                commentCells[index].SetComment(thread.Comment, OnClickComment);

                index++;

                if (index == commentCells.Length) break;
            }

            for (int i = index; i < commentCells.Length; i++)
            {
                commentCells[i].gameObject.SetActive(false);
            }
        }

        public void OnClickComment(Fresvii.AppSteroid.Models.Comment comment)
        {
            if(comment.App != null)
                GoToAppDetail(comment.App);
        }

        public GameObject prfbAppDetail;

        public void GoToAppDetail(Fresvii.AppSteroid.Models.App app)
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiAppDetail = ((GameObject)Instantiate(prfbAppDetail)).GetComponent<AUIAppDetail>();

            auiAppDetail.SetApp(app);

            auiAppDetail.transform.SetParent(transform.parent, false);

            auiAppDetail.transform.SetAsLastSibling();

            auiAppDetail.frame.backFrame = this.frame;

            auiAppDetail.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public GameObject prfbPreviewVideoList;

        public void GoToPreviewList()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiPreviewVideoList = ((GameObject)Instantiate(prfbPreviewVideoList)).GetComponent<AUIPreviewVideoList>();

            auiPreviewVideoList.PreviewVideos = this.previewVideos;

            auiPreviewVideoList.PreviewListMeta = this.previewListMeta;

            auiPreviewVideoList.transform.SetParent(transform.parent, false);

            auiPreviewVideoList.transform.SetAsLastSibling();

            auiPreviewVideoList.frame.backFrame = this.frame;

            auiPreviewVideoList.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public GameObject prfbRecommendedApps;

        public void GoToRecommendedApps()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiRecommendedApps = ((GameObject)Instantiate(prfbRecommendedApps)).GetComponent<AUIRecommendedApps>();

            auiRecommendedApps.mode = AUIRecommendedApps.Mode.AdvertisementApps;

            auiRecommendedApps.transform.SetParent(transform.parent, false);

            auiRecommendedApps.transform.SetAsLastSibling();

            auiRecommendedApps.frame.backFrame = this.frame;

            auiRecommendedApps.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

    }
}
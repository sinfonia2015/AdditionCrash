using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        List<AUIRecommendedAppCell> recommendedAppCells = new List<AUIRecommendedAppCell>();

        public GameObject prfbRecommendedAppCell;

        public AUIScrollViewContents recommendedAppContents;

        IList<Fresvii.AppSteroid.Models.Video> previewVideos;

        public AUICommunityTopCommentCell[] commentCells;

        public GameObject previews;

        public GameObject recommendApps;

        public GameObject communities;

        public Graphic fadeRecommendedApps;
        
        // Use this for initialization
        void Awake()
        {
            previews.SetActive(false);

            recommendApps.SetActive(false);
 
            communities.SetActive(false);
        }

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

            fadeRecommendedApps.gameObject.SetActive(true);

            fadeRecommendedApps.CrossFadeAlpha(1f, 0f, true);
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

            FASAdvertisement.GetVideoList(1, OnGetVideoList);

            yield return 1;

            FASAdvertisement.GetAppList(1, OnGetAppList);

            yield return 1;

            FASAdvertisement.GetEventList(null, OnGetEventList);

            yield return 1;

            FASAdvertisement.GetThreadList(10, OnGetThreadList);
        }

        IEnumerator FadeOutMask(Graphic g)
        {
            g.CrossFadeAlpha(0f, 0.3f, true);

            yield return new WaitForSeconds(0.3f);

            g.gameObject.SetActive(false);
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

            if (meta.TotalCount > 0)
            {
                recommendApps.SetActive(true);
            }
            else
            {
                return;
            }

            foreach (var app in apps)
            {
                var cell = recommendedAppCells.Find(x => x.AddApp.Id == app.Id);

                if (cell != null)
                {
                    cell.SetApp(app);
                }
                else
                {
                    var item = ((GameObject)Instantiate(prfbRecommendedAppCell)).GetComponent<RectTransform>();

                    recommendedAppContents.AddItem(item);

                    cell = item.GetComponent<AUIRecommendedAppCell>();

                    cell.SetApp(app);

                    cell.OnClickAppCell += GoToAppDetail;

                    recommendedAppCells.Add(cell);
                }
            }

            recommendedAppCells = recommendedAppCells.OrderBy(c => System.Guid.NewGuid()).ToList();

            recommendedAppContents.ReLayout();

            recommendedAppContents.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, recommendedAppContents.GetComponent<RectTransform>().anchoredPosition.y);

            if (fadeRecommendedApps.gameObject.activeSelf)
            {
                StartCoroutine(FadeOutMask(fadeRecommendedApps));
            }
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

            this.previewListMeta = meta;

            this.previewVideos = videos;

            if (meta.TotalCount > 0)
            {
                previews.SetActive(true);
            }
            else
            {
                return;
            }

            foreach (var video in videos)
            {
                var cell = previewCells.Find(x => x.Video.Id == video.Id);

                if (cell != null)
                {
                    cell.SetPreview(video);

                    continue;
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

        public GameObject gameEventsObj;

        private List<AUIHotGameEventCell> gameEventCells = new List<AUIHotGameEventCell>();

        public GameObject prfbGameEventCell;

        public AUIScrollViewContents gameEventContents;

        void OnGetEventList(IList<Fresvii.AppSteroid.Models.GameEvent> events, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
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

            if (meta.TotalCount > 0)
            {
                gameEventsObj.SetActive(true);
            }

            foreach (Fresvii.AppSteroid.Models.GameEvent gameEvent in events)
            {
                var cell = gameEventCells.Find(x => x.GameEvent.Id == gameEvent.Id);

                if (cell != null)
                {
                    cell.SetGameEvent(gameEvent, (ge) =>
                    {
                        GoToAppDetail(ge.App);
                    });

                    continue;
                }

                var item = ((GameObject)Instantiate(prfbGameEventCell)).GetComponent<RectTransform>();

                gameEventContents.AddItem(item);

                cell = item.GetComponent<AUIHotGameEventCell>();

                cell.SetGameEvent(gameEvent, (ge) => 
                {
                    GoToAppDetail(ge.App);
                });

                gameEventCells.Add(cell);
            }

            // Sort
            gameEventCells.Sort(SortEventsCondition);

            foreach (var obj in gameEventCells)
            {
                obj.transform.SetSiblingIndex(gameEventContents.transform.childCount - 1);
            }

            gameEventContents.ReLayout();
        }

        int SortEventsCondition(AUIHotGameEventCell a, AUIHotGameEventCell b)
        {
            int ret = System.DateTime.Compare(a.GameEvent.EndAt, b.GameEvent.EndAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = System.DateTime.Compare(a.GameEvent.StartAt, b.GameEvent.StartAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = string.Compare(a.GameEvent.Id, b.GameEvent.Id);

            return ret;
        }


        public GameObject prfbAUIGameEvent;

        public void GoToGameEvent(Fresvii.AppSteroid.Models.GameEvent gameEvent)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject goGameEvent = Instantiate(prfbAUIGameEvent) as GameObject;

            AUIGameEvent auiGameEvent = goGameEvent.GetComponent<AUIGameEvent>();

            auiGameEvent.GameEvent = gameEvent;

            goGameEvent.transform.SetParent(transform.parent, false);

            goGameEvent.gameObject.SetActive(true);

            goGameEvent.transform.SetAsLastSibling();

            AUIFrame nextFrame = auiGameEvent.frame;

            nextFrame.backFrame = this.frame;

            nextFrame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
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

            if (meta.TotalCount > 0)
            {
                communities.SetActive(true);
            }
            else
            {
                return;
            }

            int index = 0;

            foreach (var thread in threads)
            {
                if (string.IsNullOrEmpty(thread.Comment.Text))
                    continue;

                thread.Comment.App = thread.App;

                commentCells[index].gameObject.SetActive(true);

                commentCells[index].SetThread(thread, true, OnClickCell);

                index++;

                if (index == commentCells.Length) break;
            }

            for (int i = index; i < commentCells.Length; i++)
            {
                commentCells[i].gameObject.SetActive(false);
            }
        }

        public void OnClickCell(Fresvii.AppSteroid.Models.Thread thread)
        {
            if(thread.Comment != null &&  thread.Comment.App != null)
                GoToAppDetail(thread.Comment.App);
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

        Vector2 preSize;

        void Update()
        {
            if(preSize != GetComponent<RectTransform>().sizeDelta)
            {
                preSize = GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            }
        }
    }
}
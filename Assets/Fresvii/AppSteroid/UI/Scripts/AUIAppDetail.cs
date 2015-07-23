using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUIAppDetail : MonoBehaviour
    {
        public AUIFrame frame;
        
        public Fresvii.AppSteroid.Models.App App { get; set; }

        public AUIRawImageTextureSetter appIcon;

        public Text title, appName, devName, gameCategory, description;

        // Preview property
        List<AUIAppsPreviewCell> previewCells = new List<AUIAppsPreviewCell>();

        IList<Fresvii.AppSteroid.Models.Video> previewVideos;

        public GameObject prfbPreviewCell;

        Fresvii.AppSteroid.Models.ListMeta previewListMeta;

        public AUIScrollViewContents previewContents;

        public GameObject previewLoadingSpinner;

        public AUICommunityTopCommentCell[] commentCells;

        public Text communityText;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;
        }

        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while(this.App == null)
            {
                yield return 1;
            }

            FASApps.GetApp(this.App.Id, (app, error) =>
            {
                if (error == null)
                {
                    SetApp(app);
                }
                else
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("UnknownError"), FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"), (del)=>{ });
                }                
            });

            FASApps.GetVideoList(1, this.App.Id, OnGetVideoList);

            FASApps.GetThreadList(1, 10, this.App.Id, OnGetThreadList);

            FASUtility.SendPageView("pv.apps.show", this.App.Id, System.DateTime.UtcNow, (e) => 
            {
                if (e != null)
                    Debug.LogError(e.ToString());
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

            int index = 0;

            foreach (var thread in threads)
            {
                if (string.IsNullOrEmpty(thread.Comment.Text))
                    continue;

                thread.Comment.App = thread.App;

                thread.Comment.User = thread.User;

                commentCells[index].gameObject.SetActive(true);

                commentCells[index].SetComment(thread.Comment, null);

                index++;

                if (index == commentCells.Length) break;
            }

            for (int i = index; i < commentCells.Length; i++)
            {
                commentCells[i].gameObject.SetActive(false);
            }
        }

        public void SetApp(Fresvii.AppSteroid.Models.App app)
        {
            this.App = app;

            this.title.text = this.appName.text = this.App.Name;

            devName.text = this.App.GameDeveloper.Name;

            if(this.App.GameGenres.Count > 0)
                this.gameCategory.text = this.App.GameGenres[0].Name;

            this.description.text = this.App.Description;

            appIcon.Set(this.App.IconUrl);

            communityText.text = this.App.Name + " " + FASText.Get("Community");
        }

        public void ShareTwitter()
        {
            string text = FASText.Get("AppShare").Replace("%gametitle", FASConfig.Instance.appName);

            Fresvii.AppSteroid.Util.SocialNetworkingService.ShareTwitterWithUI(text, this.App.StoreUrl, (result) =>
            {
                if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                {
                    Debug.LogError("FASSNS.ShareTwitter : error");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("TwitterError"), (del) => { });
                }
            });

        }

        public void ShareFacebook()
         {
             string text = FASText.Get("AppShare").Replace("%gametitle", FASConfig.Instance.appName);

             Fresvii.AppSteroid.Util.SocialNetworkingService.ShareFacebook(text, this.App.StoreUrl, (result) =>
             {
                 if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                 {
                     Debug.LogError("FASSNS.ShareFacebook : error");

                     Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

                     Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("FacebookError"), (del) => { });
                 }
             });
         }

        public void GoToAppStore()
         {
             FASUtility.SendPageView("event.ad.click.store", this.App.Id, System.DateTime.UtcNow, (e) =>
             {
                 if (e != null)
                     Debug.LogError(e.ToString());
             });    

             Application.OpenURL(this.App.StoreUrl);
         }

        public void Back()
        {
            if (frame.Animating) return;

            if (frame.backFrame != null)
            {
                frame.backFrame.gameObject.SetActive(true);

                RectTransform rectTransform = GetComponent<RectTransform>();

                frame.backFrame.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
                {
                    Destroy(this.gameObject);
                });
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

            previewLoadingSpinner.SetActive(false);

            this.previewListMeta = meta;

            this.previewVideos = videos;

            foreach (var video in videos)
            {
                video.App = this.App;

                var cell = previewCells.Find(x => x.Video.Id == video.Id);

                if (cell != null)
                {
                    cell.SetPreview(video);

                    return;
                }

                var item = ((GameObject)Instantiate(prfbPreviewCell)).GetComponent<RectTransform>();

                previewContents.AddItem(item);

                cell = item.GetComponent<AUIAppsPreviewCell>();

                cell.SetPreview(video);

                previewCells.Add(cell);
            }
        }

        public GameObject prfbPreviewVideoList;

        public void GoToPreviewList()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiPreviewVideoList = ((GameObject)Instantiate(prfbPreviewVideoList)).GetComponent<AUIPreviewVideoList>();

            auiPreviewVideoList.App = this.App;

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
    }
}

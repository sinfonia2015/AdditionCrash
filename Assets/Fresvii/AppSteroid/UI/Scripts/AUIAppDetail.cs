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

        public AUICommunityTopCommentCell[] commentCells;

        public Text communityText;

        public GameObject buttonSeeMoreObj;

        public LayoutElement descriptionAreaLayoutElement;

        public RectTransform rectTransformDescriptionText;

        public AUITextSetter textSetterDescriptionText;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            AUIManager.Instance.HideLoadingSpinner();
        }

        IEnumerator Start()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while(this.App == null)
            {
                yield return 1;
            }

            if (string.IsNullOrEmpty(this.App.Description))
            {
                FASApps.GetApp(this.App.Id, (app, error) =>
                {
                    if (error == null)
                    {
                        SetApp(app);

                        SetLayout();
                    }
                    else
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("UnknownError"), FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"), (del) => { });
                    }
                });
            }
            else
            {
                yield return new WaitForEndOfFrame();

                SetLayout();
            }            

            while (frame.Animating)
                yield return 1;

            FASApps.GetEventList(1, 10, this.App.Id, OnGetEventList);

            FASApps.GetThreadList(1, 10, this.App.Id, OnGetThreadList);

            FASApps.GetVideoList(1, 10, this.App.Id, OnGetVideoList);

            yield return new WaitForSeconds(0.5f);

            FASUtility.SendPageView("pv.apps.show", this.App.Id, System.DateTime.UtcNow, (e) => 
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });
        }

        private Fresvii.AppSteroid.Models.ListMeta gameEventListMeta;

        public GameObject gameEventsObj;

        private List<AUICommunityTopGameEventCell> gameEventCells = new List<AUICommunityTopGameEventCell>();

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

            this.gameEventListMeta = meta;

            if (gameEventListMeta.TotalCount > 0)
            {
                gameEventsObj.SetActive(true);
            }
            else
            {
                return;
            }

            foreach (Fresvii.AppSteroid.Models.GameEvent gameEvent in events)
            {
                var cell = gameEventCells.Find(x => x.GameEvent.Id == gameEvent.Id);

                if (cell != null)
                {
                    cell.SetGameEvent(gameEvent, null);

                    return;
                }

                var item = ((GameObject)Instantiate(prfbGameEventCell)).GetComponent<RectTransform>();

                gameEventContents.AddItem(item);

                cell = item.GetComponent<AUICommunityTopGameEventCell>();

                cell.SetGameEvent(gameEvent, null);

                gameEventCells.Add(cell);
            }

            // Sort
            gameEventCells.Sort((a, b) => System.DateTime.Compare(a.GameEvent.EndAt, b.GameEvent.EndAt));

            foreach (var obj in gameEventCells)
            {
                obj.transform.SetSiblingIndex(gameEventContents.transform.childCount - 1);
            }

            gameEventContents.ReLayout();
        }


        public GameObject comments;

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
                comments.SetActive(true);
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

                thread.Comment.User = thread.User;

                commentCells[index].gameObject.SetActive(true);

                commentCells[index].SetThread(thread, false, null);

                index++;

                if (index == commentCells.Length) break;
            }

            for (int i = index; i < commentCells.Length; i++)
            {
                commentCells[i].gameObject.SetActive(false);
            }
        }

        private float descriptionPrefferedHeight = 280f;

        public void SetApp(Fresvii.AppSteroid.Models.App app)
        {
            this.App = app;
        }

        void SetLayout()
        {
            this.title.text = this.appName.text = this.App.Name;

            if (this.App.GameDeveloper != null)
            {
                devName.text = this.App.GameDeveloper.Name;
            }

            if (this.App.GameGenres != null && this.App.GameGenres.Count > 0)
            {
                if (this.App.GameGenres[0].ParentGenre != null)
                {
                    this.gameCategory.text = this.App.GameGenres[0].ParentGenre.Name + " / " + this.App.GameGenres[0].Name;
                }
                else
                {
                    this.gameCategory.text = this.App.GameGenres[0].Name;
                }
            }

            appIcon.Set(this.App.IconUrl);

            communityText.text = this.App.Name + " " + FASText.Get("Community");

            this.description.text = this.App.Description;

            this.description.CalculateLayoutInputVertical();

            this.description.CalculateLayoutInputHorizontal();

            descriptionPrefferedHeight = description.preferredHeight;
            
            if (descriptionPrefferedHeight <= textSetterDescriptionText.truncateHeight)
            {
                buttonSeeMoreObj.SetActive(false);

                rectTransformDescriptionText.sizeDelta = new Vector2(rectTransformDescriptionText.sizeDelta.x, descriptionPrefferedHeight);

                descriptionAreaLayoutElement.minHeight = descriptionAreaLayoutElement.preferredHeight = descriptionPrefferedHeight + descriptionBottomMargin;
            }
            else
            {
                buttonSeeMoreObj.SetActive(true);

                rectTransformDescriptionText.sizeDelta = new Vector2(rectTransformDescriptionText.sizeDelta.x, textSetterDescriptionText.truncateHeight);

                descriptionAreaLayoutElement.minHeight = descriptionAreaLayoutElement.preferredHeight = 286f;
            }
        }

        public float descriptionBottomMargin = 20f;

        public void OnClickSeeMore()
        {
            buttonSeeMoreObj.SetActive(false);

            rectTransformDescriptionText.sizeDelta = new Vector2(rectTransformDescriptionText.sizeDelta.x, descriptionPrefferedHeight);

            descriptionAreaLayoutElement.minHeight = descriptionAreaLayoutElement.preferredHeight = descriptionPrefferedHeight + descriptionBottomMargin;

            textSetterDescriptionText.truncate = AUITextSetter.TruncateType.None;

            this.description.text = this.App.Description;
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

                 Application.OpenURL(this.App.StoreUrl);
             });    

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

        public GameObject previews;

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

            if (this.previewListMeta.TotalCount > 0)
            {
                previews.SetActive(true);
            }
            else
            {
                return;
            }

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

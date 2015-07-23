﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUICommunityTop : MonoBehaviour
    {
        public AUIFrame frame;

        public AUICommunityTopCommentCell[] commentCells;

        public Transform imagesAndVideosContentsNode;

        public GameObject prfbGameEventCell;

        public GameObject prfbAUICommunityTopImagesAndVideosCell;

        private Fresvii.AppSteroid.Models.ListMeta threadsListMeta;

        private Fresvii.AppSteroid.Models.ListMeta gameEventListMeta;

        private IList<Fresvii.AppSteroid.Models.GameEvent> Events;

        private List<AUICommunityTopImagessAndVideosCell> imagesAndVideosCells = new List<AUICommunityTopImagessAndVideosCell>();

        private List<AUICommunityTopGameEventCell> gameEventCells = new List<AUICommunityTopGameEventCell>();

        public GameObject prfbAUIForum;

        public GameObject prfbAUIGameEvent;

        private GameObject forum;

        public AUIScrollViewContents gameEventContents;

        public AUIScrollViewContents imagesAndVideosContents;

        private bool cacheShown;

        public Text title;

        public Text devAppTitle;

        public Text gameEventLabel, forumLabel, threadLabel;

        // Use this for initialization
        void OnEnable()
        {
            SortImages();

            SortEvnets();

            imagesAndVideosContents.ReLayout();

            gameEventContents.ReLayout();

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.Instance.HideLoadingSpinner();
        }

        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            if (!cacheShown)
            {
                FASForum.GetForumThreadsFromCache(OnGetForumThreads);

                cacheShown = true;
            }

            while (!FASUser.IsLoggedIn())
            {
                yield return 1;
            }

            Fresvii.AppSteroid.FASForum.GetForumThreads(OnGetForumThreads);

            string query = "{\"where\":[{\"column\": \"video_id\", \"operator\": \"!=\", \"value\": null},{\"column\": \"image\",    \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

            Fresvii.AppSteroid.FASForum.GetCommentList(query, OnGetImageAndVideoComments);

            FASLeaderboard.GetEventList(Fresvii.AppSteroid.Models.GameEvent.Status.Ongoing, OnGetEventList);

            FASUtility.GetDeveloperAppList(1, OnGetAppList);
        }

        void Start()
        {
            title.text = frame.title = FASConfig.Instance.appName + " " + FASText.Get("Community");

            devAppTitle.text = "";

            forumLabel.text = FASConfig.Instance.appName + " " + FASText.Get("Forum");

			threadLabel.text = " / " + FASText.Get("Thread");

			gameEventLabel.text = FASConfig.Instance.appName + " " + FASText.Get("GameEvents");
        }

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

            this.Events = events;

            this.gameEventListMeta = meta;

            foreach (Fresvii.AppSteroid.Models.GameEvent gameEvent in events)
            {
                var cell = gameEventCells.Find(x => x.GameEvent.Id == gameEvent.Id);

                if (cell != null)
                {
                    cell.SetGameEvent(gameEvent, this);

                    return;
                }

                var item = ((GameObject)Instantiate(prfbGameEventCell)).GetComponent<RectTransform>();
                
                gameEventContents.AddItem(item);

                cell = item.GetComponent<AUICommunityTopGameEventCell>();

                cell.SetGameEvent(gameEvent, this);

                gameEventCells.Add(cell);
            }

            SortEvnets();
        }

        private void OnGetImageAndVideoComments(IList<Fresvii.AppSteroid.Models.Comment> comments, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
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

            foreach (var comment in comments)
            {
                AUICommunityTopImagessAndVideosCell cell = imagesAndVideosCells.Find(x => x.Comment.Id == comment.Id);

                if (cell != null)
                {
                    cell.SetComment(comment, this);

                    return;
                }

                var item = ((GameObject)Instantiate(prfbAUICommunityTopImagesAndVideosCell)).GetComponent<RectTransform>();

                imagesAndVideosContents.AddItem(item);

                cell = item.GetComponent<AUICommunityTopImagessAndVideosCell>();

                cell.SetComment(comment, this);

                imagesAndVideosCells.Add(cell);
            }

            SortImages();
        }

        private void OnGetForumThreads(IList<Fresvii.AppSteroid.Models.Thread> threads, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
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

            this.threadsListMeta = meta;

            int index = 0;

            foreach (Fresvii.AppSteroid.Models.Thread thread in threads)
            {
                if(string.IsNullOrEmpty(thread.Comment.Text))
                    continue;

                commentCells[index].gameObject.SetActive(true);

                UpdateThread(thread, index);

                index++;

                if (index == commentCells.Length) break;
            }

            for (int i = index; i < commentCells.Length; i++)
            {
                commentCells[i].gameObject.SetActive(false);
            }
        }

        public GameObject recommendedLoadingSpinner;

        private Fresvii.AppSteroid.Models.ListMeta recommendedAppListMeta;

        List<AUIRecommendedAppCell> recommendedAppCells = new List<AUIRecommendedAppCell>();

        public AUIScrollViewContents recommendedAppContents;

        public GameObject prfbRecommendedAppCell;

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

            if (apps.Count > 0)
            {
                devAppTitle.text = FASText.Get("MoreAppsBy").Replace("%developer", apps[0].GameDeveloper.Name);
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

        public void SortImages()
        {
            // Sort
            imagesAndVideosCells.Sort((a, b) => System.DateTime.Compare(b.Comment.CreatedAt, a.Comment.CreatedAt));

            for (int i = imagesAndVideosCells.Count - 1; i >= 0; i--)
            {
                var obj = imagesAndVideosCells[i];

                obj.transform.SetAsFirstSibling();
            }

            imagesAndVideosContents.ReLayout();
        }

        public void SortEvnets()
        {
            // Sort
            gameEventCells.Sort((a, b) => System.DateTime.Compare(a.GameEvent.EndAt, b.GameEvent.EndAt));

            foreach (var obj in gameEventCells)
            {
                obj.transform.SetSiblingIndex(gameEventContents.transform.childCount - 1);
            }

            gameEventContents.ReLayout();
        }

        private void UpdateThread(Fresvii.AppSteroid.Models.Thread thread, int index)
        {
            commentCells[index].SetThread(thread);
        }

        public void GoToForum(int mode)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            if (forum == null)
            {
                forum = Instantiate(prfbAUIForum) as GameObject;

                forum.transform.SetParent(transform.parent, false);

                forum.GetComponent<AUIForum>().auiCommunityTop = this;
            }

            forum.GetComponent<AUIForum>().SelectedMode = mode;

            forum.GetComponent<AUIForum>().segmentedControl.selectedIndex = mode;

            forum.gameObject.SetActive(true);

            forum.transform.SetAsLastSibling();

            AUIFrame nextFrame = forum.GetComponent<AUIFrame>();

            nextFrame.backFrame = this.frame;

            nextFrame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }
       
        //public void GoToThread(string threadId, string commentId, bool animation)
        public void GoToThread(string threadId, Fresvii.AppSteroid.Models.Comment comment, bool animation)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (forum == null)
            {
                forum = Instantiate(prfbAUIForum) as GameObject;

                forum.transform.SetParent(transform.parent, false);

                forum.GetComponent<AUIForum>().auiCommunityTop = this;

                forum.GetComponent<AUIForum>().segmentedControl.SetIndex(0);
            }

            AUIFrame nextFrame = forum.GetComponent<AUIFrame>();

            nextFrame.backFrame = this.frame;

            forum.GetComponent<AUIForum>().ShowThread(threadId, comment, animation, (error) =>
            {
                if (error != null)
                {
                    Debug.LogError(error.ToString());
                }
                else
                {
                    this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
                    {
                        this.gameObject.SetActive(false);
                    });
                }
            });

            forum.gameObject.SetActive(true);

            forum.transform.SetAsLastSibling();

            nextFrame.SetPosition(new Vector2(-rectTransform.rect.width, 0f));
        }

        public GameObject prfbAUIEvent;

        GameObject eventList;

        public void GoToEventList()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (eventList == null)
            {
                eventList = Instantiate(prfbAUIEvent) as GameObject;

                eventList.transform.SetParent(transform.parent, false);

                eventList.GetComponent<AUIEvents>().auiCommunityTop = this;
            }

            eventList.gameObject.SetActive(true);

            eventList.transform.SetAsLastSibling();

            AUIFrame nextFrame = eventList.GetComponent<AUIFrame>();

            nextFrame.backFrame = this.frame;

            nextFrame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

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

        public GameObject prfbRecommendedApps;

        public void GoToRecommendedApps()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiRecommendedApps = ((GameObject)Instantiate(prfbRecommendedApps)).GetComponent<AUIRecommendedApps>();

            auiRecommendedApps.mode = AUIRecommendedApps.Mode.DeveloperApps;

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
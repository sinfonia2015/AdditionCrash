using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIForumScrollView : MonoBehaviour
    {
        public AUIForum auiForum;

        public enum Mode { All, Images, Videos };

        public Mode mode;

        public AUIScrollViewContents contents;

        [HideInInspector]
        public List<AUIForumThreadCell> threadCells = new List<AUIForumThreadCell>();

        [HideInInspector]
        public List<AUICommentGridCell> gridCells = new List<AUICommentGridCell>();

        public AUIScrollViewPullReflesh pullReflesh;

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool isPullRefleshProc;

        public AUIScrollRect scrollView;

        public RectTransform prfbThreadCell;

        public RectTransform prfbCommentGridCell;

        private bool loaded;

        uint? minPage = 1, currentPage = 1, maxPage = 1;

        void Awake()
        {
            //contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;
        }

        void OnEnable()
        {
            if (mode == Mode.All)
            {
                pullReflesh.OnPullDownReflesh += OnPullDownReflesh;
            }

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            contents.ReLayout();

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            if (mode == Mode.All)
            {
                pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;
            }

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;
        }

        void OnPullDownReflesh()
        {
            isPullRefleshProc = true;

            FASForum.GetForumThreads(OnGetForumThreads);
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.TotalPages >= maxPage && !isPullRefleshProc)
            {
                isPullRefleshProc = true;

                maxPage++;

                if (mode == Mode.All)
                {
                    FASForum.GetForumThreads((uint)maxPage, OnGetForumThreads);
                }
                else if (mode == Mode.Images)
                {
                    string query = "{\"where\":[{\"column\": \"image\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                    Fresvii.AppSteroid.FASForum.GetCommentList((uint)maxPage, query, OnGetImageAndVideoComments);
                }
                else if (mode == Mode.Videos)
                {
                    string query = "{\"where\":[{\"column\": \"video_id\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                    Fresvii.AppSteroid.FASForum.GetCommentList((uint)maxPage, query, OnGetImageAndVideoComments);
                }
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        // Use this for initialization
        IEnumerator Init()
        {
            yield return 1;

            while (!FASUser.IsLoggedIn() || auiForum.frame.Animating)
            {
                yield return 1;
            }

            if (!this.gameObject.activeSelf)
            {
                yield break;
            }

            if (!loaded && mode == Mode.All)
            {
                FASForum.GetForumThreadsFromCache(OnGetForumThreads);
            }

            if (mode == Mode.All)
            {
                if (!string.IsNullOrEmpty(auiForum.showThreadId))
                {
                    var cell = threadCells.Find(x => x.Thread.Id == auiForum.showThreadId);

                    if (cell != null)
                    {
                        auiForum.GoToThread(cell, auiForum.showComment, auiForum.showThreadWithAnimation, true);

                        if (auiForum.showThreadRedayCallback != null)
                        {
                            auiForum.showThreadRedayCallback(null);
                        }

                        auiForum.showThreadId = null;

                        auiForum.showThreadWithAnimation = false;

                        auiForum.showComment = null;

                        auiForum.showThreadRedayCallback = null;
                    }
                    else
                    {
                        AddThreadCell(auiForum.showThreadId, (_cell, _error) => 
                        {
                            if (auiForum.showThreadRedayCallback != null)
                            {
                                auiForum.showThreadRedayCallback(_error);
                            }

                            if (_error == null)
                            {
                                auiForum.GoToThread(_cell, auiForum.showComment, auiForum.showThreadWithAnimation, true);
                            }

                            auiForum.showThreadId = null;

                            auiForum.showThreadWithAnimation = false;

                            auiForum.showComment = null;

                            auiForum.showThreadRedayCallback = null;
                        });
                    }
                }
                else
                {
                    FASForum.GetForumThreads(OnGetForumThreads);

                    if (auiForum.showThreadRedayCallback != null)
                    {
                        auiForum.showThreadRedayCallback(null);
                    }

                    auiForum.showThreadId = null;

                    auiForum.showThreadWithAnimation = false;

                    auiForum.showComment = null;

                    auiForum.showThreadRedayCallback = null;
                }
            }
            else if (mode == Mode.Images)
            {
                string query = "{\"where\":[{\"column\": \"image\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                Fresvii.AppSteroid.FASForum.GetCommentList(query, OnGetImageAndVideoComments);
            }
            else if (mode == Mode.Videos)
            {
                string query = "{\"where\":[{\"column\": \"video_id\", \"operator\" : \"!=\", \"value\": null}],\"order\": {\"created_at\": \"desc\"}}";

                Fresvii.AppSteroid.FASForum.GetCommentList(query, OnGetImageAndVideoComments);
            }

			contents.ReLayout();

        }

        IEnumerator SetContentPadding()
        {
            yield return new WaitForEndOfFrame();

            contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;
        }

        public void AddThreadCell(string threadId, Action<AUIForumThreadCell, Fresvii.AppSteroid.Models.Error> callback)
        {
            FASForum.GetThread(threadId, (thread, error) =>
            {
                if (error == null)
                {
                    var item = GameObject.Instantiate(prfbThreadCell) as RectTransform;

                    contents.AddItem(item);

                    AUIForumThreadCell cell = item.GetComponent<AUIForumThreadCell>();

                    cell.auiForum = auiForum;

                    cell.SetThraed(thread);

                    threadCells.Add(cell);

                    callback(cell, null);
                }
                else
                {
                    callback(null, error);
                }
            });
        }

        private void OnGetImageAndVideoComments(IList<Fresvii.AppSteroid.Models.Comment> comments, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (isPullRefleshProc)
            {
                isPullRefleshProc = false;

                pullReflesh.PullRefleshCompleted(false);
            }

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

                if (isPullRefleshProc)
                {
                    pullReflesh.PullRefleshCompleted();

                    isPullRefleshProc = false;
                }

                return;
            }

            this.listMeta = meta;
            
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(LoadImages(comments));
            }
        }

        public Transform gridContents;

        IEnumerator LoadImages(IList<Fresvii.AppSteroid.Models.Comment> comments)
        {
            int i = 0;

            foreach (var comment in comments)
            {
                if (comment.VideoState == Models.Comment.VideoStatus.Removed)
                {
                    continue;
                }

                var cell = gridCells.Find(x => x.Comment.Id == comment.Id);

                if (cell != null)
                {
                    cell.SetComment(comment, this.auiForum);

                    continue;
                }

                var item = GameObject.Instantiate(prfbCommentGridCell) as RectTransform;

                //contents.AddItem(item);

                cell = item.GetComponent<AUICommentGridCell>();

                cell.SetComment(comment, this.auiForum);

                gridCells.Add(cell);

                cell.transform.SetParent(gridContents, false);

                if(++i%2 == 0)  
                    yield return 1;
            }

            contents.ReLayout();
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
                    Debug.LogError(error.ToString());
                }

                if (isPullRefleshProc)
                {
                    pullReflesh.PullRefleshCompleted();

                    isPullRefleshProc = false;
                }

                return;
            }

            loaded = true;

            if (this.listMeta == null || this.listMeta.CurrentPage != 1)
            {
                this.listMeta = meta;
            }

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Thread thread in threads)
            {
                added |= UpdateThread(thread);
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted(added);

                isPullRefleshProc = false;
            }

            Sort();
        }

        public void Sort()
        {
            // Sort
            threadCells.Sort((a, b) => System.DateTime.Compare(b.Thread.LastUpdateAt, a.Thread.LastUpdateAt));

            foreach (var obj in threadCells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            contents.ReLayout();
        }

        public void LoadLatestThreads()
        {
            FASForum.GetForumThreads(OnGetForumThreads);
        }

        private bool UpdateThread(Fresvii.AppSteroid.Models.Thread thread)
        {
            AUIForumThreadCell cell = threadCells.Find(x => x.Thread.Id == thread.Id);

            if (cell != null)
            {
                cell.SetThraed(thread);

                return false;
            }

            var item = GameObject.Instantiate(prfbThreadCell) as RectTransform;

            contents.AddItem(item);

            cell = item.GetComponent<AUIForumThreadCell>();

            cell.auiForum = auiForum;

            cell.SetThraed(thread);

            threadCells.Add(cell);

            cell.gameObject.SetActive(true);

            return true;
        }

        public void RemoveCell(AUIForumThreadCell cell)
        {
            var removeCell = threadCells.Find(x => x.Thread.Id == cell.Thread.Id);

            if (removeCell != null)
            {
                threadCells.Remove(removeCell);

                contents.RemoveItem(removeCell.GetComponent<RectTransform>());

                Destroy(removeCell.gameObject);

                Sort();
            }
        }
    }
}

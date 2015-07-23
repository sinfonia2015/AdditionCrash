using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIThread : MonoBehaviour
    {
        public RectTransform prfbCommentCell;

        public AUIScrollViewContents contents;

        private List<AUIForumCommentCell> commentCells = new List<AUIForumCommentCell>();

        public AUIScrollViewPullReflesh pullReflesh;

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool pullRefleshing;

        public AUIForumThreadCell ThreadCell { get; set; }

        public AUIFrame frame;

        [HideInInspector]
        public AUIForum auiForumManager;

        public AUIScrollRect scrollView;

        public RectTransform buttonSubscribeCenter;

        public Button buttonSubscribe;

        public AUIInputComment auiInputComment;

        public GameObject prfbMyPage;

        public GameObject prfbUserPage;

        public Text title;

        //public Text backButtonText;

        public Fresvii.AppSteroid.Models.Comment showComment;

        public Image subscribeButtonImage;

        public Color subscribeColor, unsubscribeColor;

        uint? minPage = uint.MaxValue, currentPage = 1, maxPage = uint.MinValue;

        void OnEnable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            auiInputComment.OnInputDone += OnInputDone;

            auiInputComment.OnHeightChanged += OnInputHeightChanged;

            auiInputComment.OnChooseMovie += OnChooseMovie;

            AUIManager.OnEscapeTapped += Back;
        }

        void OnDisable()
        {
            pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            auiInputComment.OnInputDone -= OnInputDone;

            auiInputComment.OnHeightChanged -= OnInputHeightChanged;

            AUIManager.OnEscapeTapped -= Back;

            AUIManager.Instance.HideLoadingSpinner();

            auiInputComment.OnChooseMovie -= OnChooseMovie;
        }

        IEnumerator Start()
        {
            AUIManager.Instance.ShowLoadingSpinner(false);

            if (this.ThreadCell.Thread != null)
                subscribeButtonImage.color = ThreadCell.Thread.Subscribed ? subscribeColor : unsubscribeColor;

            while (frame.Animating)
            {
                yield return 1;
            }

            while (this.ThreadCell == null || this.ThreadCell.Thread == null)
            {
                yield return 1;
            }

            subscribeButtonImage.color = ThreadCell.Thread.Subscribed ? subscribeColor : unsubscribeColor;

            if (showComment != null)
            {
                FASForum.GetComment(showComment.Id, (comment, error) =>
                {
                    if (error == null)
                    {
                        uint page = (uint)Mathf.CeilToInt(comment.Order / 25f);

                        FASForum.GetThreadComments(this.ThreadCell.Thread.Id, page, OnGetThreadComments);
                    }
                    else
                    {
                        FASForum.GetThreadComments(this.ThreadCell.Thread.Id, OnGetThreadComments);
                    }
                });
            }
            else
            {
                FASForum.GetThreadComments(this.ThreadCell.Thread.Id, OnGetThreadComments);
            }

            FASUtility.SendPageView("pv.community.forum.threads.show", this.ThreadCell.Thread.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });
        }

        public GameObject prfbVideoList;

        void GoToVideoList()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbVideoList) as GameObject;

            go.GetComponent<RectTransform>().SetParent(transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIVideoList videoList = go.GetComponent<AUIVideoList>();

            videoList.IsModal = true;

            videoList.backButtonText.text = FASText.Get("Cancel");

            videoList.parentFrameTween = this.frame;

            videoList.mode = AUIVideoList.Mode.Select;

            videoList.frameTween.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () =>
            {
                this.gameObject.SetActive(false);
            });

            videoList.OnVideoSelected += (video, thumbnail) =>
            {
                if (video != null)
                {
                    auiInputComment.SetVideo(video, thumbnail);
                }
            };
        }

        bool isPullDown;

        void OnPullDownReflesh()
        {
            if (ThreadCell.Thread == null)
            {
                pullReflesh.PullRefleshCompleted();

                return;
            }

            isPullDown = true;

            pullRefleshing = true;

            if (minPage == uint.MaxValue)
            {
                FASForum.GetThreadComments(ThreadCell.Thread.Id, OnGetThreadComments);
            }
            else if (minPage > 1)
            {
                minPage--;

                FASForum.GetThreadComments(ThreadCell.Thread.Id, (uint)minPage, OnGetThreadComments);
            }
            else
            {
                FASForum.GetThreadComments(ThreadCell.Thread.Id, OnGetThreadComments);
            }
        }

        void OnChooseMovie()
        {
            GoToVideoList();
        }

        void OnPullUpReflesh()
        {
            if (ThreadCell.Thread == null)
            {
                pullReflesh.PullRefleshCompleted();

                return;
            }

            if(listMeta != null && maxPage > listMeta.TotalPages )
            {
                pullReflesh.PullRefleshCompleted();

                return;
            }

            if (maxPage == uint.MinValue)
            {
                FASForum.GetThreadComments(ThreadCell.Thread.Id, OnGetThreadComments);
            }
            else
            {
                maxPage++;

                FASForum.GetThreadComments(ThreadCell.Thread.Id, (uint)maxPage, OnGetThreadComments);
            }
        }

        public void SetThreadCell(AUIForumThreadCell threadCell)
        {
            this.ThreadCell = threadCell;
        }

        private void OnGetThreadComments(IList<Fresvii.AppSteroid.Models.Comment> comments, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            AUIManager.Instance.HideLoadingSpinner();

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

            this.listMeta = meta;

            currentPage = meta.CurrentPage;

            if (minPage > currentPage)
            {
                minPage = currentPage;
            }

            if (maxPage < currentPage)
            {
                maxPage = currentPage;
            }

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Comment comment in comments)
            {
                added |= UpdateComment(comment);
            }

            if (pullRefleshing)
            {
                pullReflesh.PullRefleshCompleted();

                pullRefleshing = false;
            }

            if (isPullDown)
            {
                isPullDown = false;

                scrollView.Pinned();
            }

            // Sort
            commentCells.Sort(SortCondition);

            foreach (var obj in commentCells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            contents.ReLayout();


            if (showComment != null)
            {
                StartCoroutine(ShowComment());
            }
        }

        int SortCondition(AUIForumCommentCell a, AUIForumCommentCell b)
        {
            int ret = System.DateTime.Compare(a.Comment.UpdateAt, b.Comment.UpdateAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = System.DateTime.Compare(a.Comment.CreatedAt, b.Comment.CreatedAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = string.Compare(a.Comment.User.Name, b.Comment.User.Name);
 
            return ret;
        }

        IEnumerator ShowComment()
        {
            yield return new WaitForEndOfFrame();

            AUIForumCommentCell cell = commentCells.Find(x => x.Comment.Id == showComment.Id);

            if (cell != null)
            {
                if (- contents.GetComponent<RectTransform>().sizeDelta.y + AUIManager.Instance.sizedCanvas.rect.height - cell.GetComponent<RectTransform>().anchoredPosition.y > 0)
                {
                    //scrollView.GoToBottom(0f);
                    contents.SetScorllPosition(new Vector2(contents.GetComponent<RectTransform>().anchoredPosition.x, contents.GetComponent<RectTransform>().sizeDelta.y - AUIManager.Instance.sizedCanvas.rect.height));
                }
                else
                {
                    contents.SetScorllPosition(-cell.GetComponent<RectTransform>().anchoredPosition - new Vector2(0f, contents.padding.top));
                }
            }

            showComment = null;
        }

        private bool UpdateComment(Fresvii.AppSteroid.Models.Comment comment)
        {
            AUIForumCommentCell cell = commentCells.Find(x => x.Comment.Id == comment.Id);

            if (cell != null)
            {
                cell.SetComment(comment);

                return false;
            }
            else
            {
                var item = GameObject.Instantiate(prfbCommentCell) as RectTransform;

                contents.AddItem(item);

                cell = item.GetComponent<AUIForumCommentCell>();

                cell.ThreadManager = this;

                cell.SetComment(comment);

                commentCells.Add(cell);

                cell.gameObject.SetActive(false);

                return true;
            }
        }

        public void Back()
        {
            scrollView.StopScroll();

            frame.backFrame.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            frame.backFrame.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frame.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (user.Id == FAS.CurrentUser.Id)
            {
                AUIMyPage myPage = ((GameObject)Instantiate(prfbMyPage)).GetComponent<AUIMyPage>();

                myPage.transform.SetParent(transform.parent, false);

                myPage.transform.SetAsLastSibling();

                myPage.parentFrameTween = this.frame;

                myPage.backButtonText.text = title.text;

                myPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }
            else
            {
                AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

                userPage.transform.SetParent(transform.parent, false);

                userPage.Set(user, title.text, this.frame);

                userPage.transform.SetAsLastSibling();

                userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public void OnClickSubscribeButton()
        {
            buttonSubscribe.interactable = false;

            List<string> buttons = new List<string>();

            if (ThreadCell.Thread.Subscribed)
            {
                buttons.Add(FASText.Get("Unsubscribe"));
            }
            else
            {
                buttons.Add(FASText.Get("Subscribe"));
            }

            if (ThreadCell.Thread.User.Id == FAS.CurrentUser.Id)
            {
                buttons.Add(FASText.Get("Delete"));
            }

            buttons.Add(FASText.Get("Cancel"));

            AUIPopUpBalloon.Show(buttons.ToArray(), buttonSubscribeCenter, (selected) =>
            {
                if (selected == FASText.Get("Unsubscribe"))
                {
                    Unsubscribe();
                }
                else if (selected == FASText.Get("Subscribe"))
                {
                    Subscribe();
                }
                else if (selected == FASText.Get("Delete"))
                {
                    DeleteThread();
                }
                else
                {
                    buttonSubscribe.interactable = true;
                }
            });
        }

        private void Subscribe()
        {
            subscribeButtonImage.color = subscribeColor;

            ThreadCell.SetSubscribeButtonColor(true);

            StartCoroutine(SubscribeCoroutine());
        }

        private IEnumerator SubscribeCoroutine()
        {
            while (string.IsNullOrEmpty(ThreadCell.Thread.Id)) yield return 1;

            FASForum.Subscribe(ThreadCell.Thread.Id, (thread, error) =>
            {
                buttonSubscribe.interactable = true;

                if (error == null)
                {
                    ThreadCell.SetThraed(thread);
                }
                else
                {
                    subscribeButtonImage.color = ThreadCell.Thread.Subscribed ? subscribeColor : unsubscribeColor;

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                }
            });
        }

        private void Unsubscribe()
        {
            subscribeButtonImage.color = unsubscribeColor;

            ThreadCell.SetSubscribeButtonColor(false);

            StartCoroutine(UnsubscribeCoroutine());
        }

        private IEnumerator UnsubscribeCoroutine()
        {
            while (string.IsNullOrEmpty(ThreadCell.Thread.Id)) yield return 1;

            FASForum.Unsubscribe(ThreadCell.Thread.Id, (error) =>
            {
                buttonSubscribe.interactable = true;

                if (error == null)
                {
                    ThreadCell.Thread.Subscribed = false;
                }
                else
                {
                    subscribeButtonImage.color = ThreadCell.Thread.Subscribed ? subscribeColor : unsubscribeColor;

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), delegate(bool del) { });
                }
            });
        }

        public void DeleteThread()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), delegate(bool del) { });

                return;
            }

            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Delete"), FASText.Get("Cancel"), FASText.Get("Close"));

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmDeleteThread"), (del) =>
            {
                if (del)
                {
                    StartCoroutine(DeleteThreadCoroutine());

                    Back();
                }
            });
        }

        private IEnumerator DeleteThreadCoroutine()
        {
            while (string.IsNullOrEmpty(ThreadCell.Thread.Id)) yield return 1;

            FASForum.DeleteThread(ThreadCell.Thread.Id, (error) =>
            {
                if (error != null)
                {
                    buttonSubscribe.interactable = true;

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                }
                else
                {
                    auiForumManager.RemoveCell(ThreadCell);
                }
            });
        }

        public float newCommentTweenDuration = 0.25f;

        void OnInputDone(string text, Texture2D clipImage, Fresvii.AppSteroid.Models.Video video)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del)=> {});

                return;
            }

            var item = GameObject.Instantiate(prfbCommentCell) as RectTransform;

            contents.AddItem(item);

            AUIForumCommentCell cell = item.GetComponent<AUIForumCommentCell>();

            cell.ThreadManager = this;

            Fresvii.AppSteroid.Models.Comment comment = new Fresvii.AppSteroid.Models.Comment();

            comment.User = FAS.CurrentUser;

            comment.Text = text;

            comment.UpdateAt = System.DateTime.Now;

            comment.Video = video;

            cell.SetClipImage(clipImage);
           
            cell.SetComment(comment);

            commentCells.Add(cell);

            if (video == null)
            {
                if (clipImage != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.ShowProgressSpinnerDialog("", FASText.Get("Uploading"), false);
                }

                FASForum.AddComment(ThreadCell.Thread.Id, text, clipImage, (_comment, _error)=>
                {
                    Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                    if (_error != null)
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                        if (_error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.WWWTimeOut)
                        {
                            if (this != null && this.gameObject != null)
                            {
                                if (this.gameObject.activeInHierarchy && this.enabled)
                                {
                                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("TimeOut"), delegate(bool del) { });
                                }
                            }
                        }
                        else if (_error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.NotFound || (Application.platform == RuntimePlatform.Android && _error.Detail.IndexOf("FileNotFound") >= 0))
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("ThreadNone"), delegate(bool del) { });

                            DeleteThread();
                        }
                        else
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("CommentCreateError"), delegate(bool del) { });
                        }

                        cell.DeleteCell();
                    }
                    else
                    {
                        cell.SetComment(_comment);
                    }
                });
            }
            else
            {

            }

            scrollView.GoToBottom(newCommentTweenDuration);
        }

        void OnInputHeightChanged(float height)
        {
            contents.padding.bottom = (int)height;
        }

        public void RemoveCell(AUIForumCommentCell cell)
        {
            commentCells.Remove(cell);

            contents.RemoveItem(cell.GetComponent<RectTransform>());

            Destroy(cell.gameObject);
        }
    }
}
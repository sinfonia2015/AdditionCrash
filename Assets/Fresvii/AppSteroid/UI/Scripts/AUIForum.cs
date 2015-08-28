using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIForum : MonoBehaviour
    {
        public AUIFrame frame;

        [HideInInspector]
        public AUICommunityTop auiCommunityTop;

        public GameObject prfbCreateThread;

        public GameObject prfbMyPage;

        public GameObject prfbUserPage;

        public Text title;
        
        public AUISegmentedControl segmentedControl;

        public AUIForumScrollView nodeAll, nodeImages, nodeVideos;

        public GameObject prfbThread;

        public RectTransform prfbThreadCell;

        public int SelectedMode = 0;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += GoToCommunity;

            segmentedControl.OnChanged += OnSegmentedControlChanged;

            StartCoroutine(SetSelectedModeCoroutine());
        }

        void OnDisable()
        {
            segmentedControl.OnChanged -= OnSegmentedControlChanged;

            AUIManager.OnEscapeTapped -= GoToCommunity;
        }

        void Start()
        {
            title.text = FASSettings.Instance.appName + " " + FASText.Get("Forum");
        }

        public void MoveToTopThread()
        {
            nodeAll.scrollView.GoToTop(0f);
        }

        IEnumerator SetSelectedModeCoroutine()
        {
            yield return 1;

            if (SelectedMode >= 0)
            {
                segmentedControl.selectedIndex = SelectedMode;

                segmentedControl.SetImage(SelectedMode);

                OnSegmentedControlChanged(SelectedMode);
            }

            SelectedMode = -1;
        }

        public void GoToThread(string threadId, Fresvii.AppSteroid.Models.Comment comment, bool animate, bool skipForum)
        {
            var cell = nodeAll.threadCells.Find(x => x.Thread.Id == threadId);

            if (cell != null)
            {
                GoToThread(cell, comment, animate, skipForum);
            }
            else
            {
                nodeAll.AddThreadCell(threadId, (_cell, _error) =>
                {
                    if (_error == null)
                    {
                        GoToThread(_cell, comment, animate, skipForum);
                    }
                    else
                    {
                        Debug.LogError(_error.ToString());

                        if (Application.internetReachability == NetworkReachability.NotReachable)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                            return;
                        }
                    }
                });
            }
        }

        public void GoToThread(AUIForumThreadCell cell, Fresvii.AppSteroid.Models.Comment comment, bool animate, bool skipForum)
        {
            if (frame.Animating) return;

            StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject thread = Instantiate(prfbThread) as GameObject;

            thread.GetComponent<RectTransform>().SetParent(transform.parent, false);

            thread.transform.SetAsLastSibling();

            AUIThread threadManager = thread.GetComponent<AUIThread>();

            threadManager.showComment = comment;

            threadManager.auiForumManager = this;

            threadManager.SetThreadCell(cell);

            AUIFrame nextFrame = threadManager.frame;

            if (animate)
            {
                if (skipForum)
                {
                    nextFrame.backFrame = frame.backFrame;
                    
                    nextFrame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => {});

                    this.gameObject.SetActive(false);
                }
                else
                {
                    nextFrame.backFrame = this.frame;

                    nextFrame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

                    frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
                    {
                        this.gameObject.SetActive(false);
                    });
                }
            }
            else
            {
                nextFrame.backFrame = this.frame;

                this.gameObject.SetActive(false);

                nextFrame.SetPosition(Vector2.zero);
            }
        }

        public void GoToCreateThread()
        {
            if (frame.Animating) return;

            StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUICreateThread createThread = ((GameObject)Instantiate(prfbCreateThread)).GetComponent<AUICreateThread>();

            createThread.transform.SetParent(transform.parent, false);

            createThread.auiForum = this;

            createThread.transform.SetAsLastSibling();

            createThread.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });            
        }

        public void GoToCommunity()
        {
            if (frame.Animating) return;

            StopScroll();

            auiCommunityTop.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            auiCommunityTop.GetComponent<AUIFrame>().Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frame.Animating) return;

            StopScroll();

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

        public AUIForumThreadCell CreateNewThreadCell(Fresvii.AppSteroid.Models.Thread thread, Texture2D clipImage)
        {
            segmentedControl.SetIndex(0);

            var item = GameObject.Instantiate(prfbThreadCell) as RectTransform;

            nodeAll.contents.AddItem(item);

            AUIForumThreadCell cell = item.GetComponent<AUIForumThreadCell>();

            cell.auiForum = this;

            cell.SetClipImage(clipImage);

            cell.SetThraed(thread);

            nodeAll.threadCells.Add(cell);

            cell.gameObject.SetActive(false);

            nodeAll.Sort();

            return cell;
        }

        private void StopScroll()
        {
            nodeAll.scrollView.StopScroll();

            nodeImages.scrollView.StopScroll();

            nodeVideos.scrollView.StopScroll();
        }        

        [HideInInspector]
        public string showThreadId;

        [HideInInspector]
        public Fresvii.AppSteroid.Models.Comment showComment;

        [HideInInspector]
        public bool showThreadWithAnimation;

        public Action<Fresvii.AppSteroid.Models.Error> showThreadRedayCallback;

        public void ShowThread(string threadId, Fresvii.AppSteroid.Models.Comment comment, bool animation, Action<Fresvii.AppSteroid.Models.Error> showThreadRedayCallback)
        {
            segmentedControl.SetIndex(0);

            OnSegmentedControlChanged(0);

            this.showThreadWithAnimation = true;

            this.showThreadId = threadId;

            this.showComment = comment;

            this.showThreadRedayCallback = showThreadRedayCallback;
        }

        void OnSegmentedControlChanged(int index)
        {
            nodeAll.gameObject.SetActive(index == 0);

            nodeImages.gameObject.SetActive(index == 1);

            nodeVideos.gameObject.SetActive(index == 2);
        }

        public void RemoveCell(AUIForumThreadCell cell)
        {
            nodeAll.RemoveCell(cell);

            nodeImages.RemoveCell(cell);

            nodeVideos.RemoveCell(cell);
        }

        public void ReLayout()
        {
            if (nodeAll.gameObject.activeSelf)
                nodeAll.contents.ReLayout();

            if (nodeImages.gameObject.activeSelf)
                nodeImages.contents.ReLayout();

            if (nodeVideos.gameObject.activeSelf)
                nodeVideos.contents.ReLayout();
        }
    }
}
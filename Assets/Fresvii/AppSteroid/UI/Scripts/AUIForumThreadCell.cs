using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIForumThreadCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Thread Thread { get; protected set; }

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public Text updatedAt;

        public Text likeCount;

        public Text commentCount;

        public Text comment;

        public AUIRawImageTextureSetter clipImage;

        private RectTransform clipImageRectTransform;

        private RectTransform userIconRectTransform;

        public Texture2D videoRemoved;

        public float margin;

        [HideInInspector]
        public AUIForum auiForum { get; set; }

        public RectTransform buttomButtonRectTransform;

        public RectTransform cellTopRectTransform;

        private RectTransform rectTransform;

        public Button buttonGoToThread;

        public Image fade;

        public GameObject buttonVideoPlayback;

        public Image subscribeButtonImage;

        public Color subscribeColor, unsubscribeColor;

		public AUITextSetter discriptionTextSetter;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            fade.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

            StartCoroutine(UpdateUpdatedAt());
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        void OnDestroy()
        {

        }

        public void OnClickUserIcon()
        {
            auiForum.GoToUserPage(this.Thread.User);
        }

        public void SetSubscribeButtonColor(bool subscribed)
        {
            Thread.Subscribed = subscribed;

            subscribeButtonImage.color = Thread.Subscribed ? subscribeColor : unsubscribeColor;
        }


        public void SetThraed(Fresvii.AppSteroid.Models.Thread thread)
        {
            this.Thread = thread;

            clipImage.delayCount = 2;

            if (Thread.User != null && Thread.Comment != null)
            {
                userIcon.Set(Thread.User.ProfileImageUrl);

                userName.text = Thread.User.Name;

                likeCount.text = Thread.Comment.LikeCount.ToString();

                commentCount.text = Thread.CommentCount.ToString();

                comment.text = Thread.Comment.Text;

                if (Thread.Comment.VideoState == Fresvii.AppSteroid.Models.Comment.VideoStatus.Ready)
                {
                    if (Thread.Comment.Video != null)
                    {
                        if (clipImage.GetTexture() == null && !string.IsNullOrEmpty(Thread.Comment.Video.ThumbnailUrl))
                        {
                            clipImage.Set(Thread.Comment.Video.ThumbnailUrl);
                        }

                        buttonVideoPlayback.SetActive(!string.IsNullOrEmpty(Thread.Comment.Video.VideoUrl));
                    }
                }
                else if (clipImage.GetTexture() != null && string.IsNullOrEmpty(clipImage.Url))
                {
                    clipImage.dontDestroy = false;
                }
                else if (Thread.Comment.VideoState == Fresvii.AppSteroid.Models.Comment.VideoStatus.Removed)
                {
                    clipImage.SetTexture(videoRemoved);
                }                
                else if (!string.IsNullOrEmpty(Thread.Comment.ImageThumbnailUrl))
                {
                    clipImage.Set(Thread.Comment.ImageThumbnailUrl);
                }
                else
                {
                    clipImage.gameObject.SetActive(clipImage.GetTexture() != null);
                }

                if (string.IsNullOrEmpty(thread.Comment.Text))
                {
                    comment.gameObject.SetActive(false);

                    comment.rectTransform.sizeDelta = Vector2.zero;
                }

                buttonGoToThread.interactable = !string.IsNullOrEmpty(thread.Id);

                buttonLike.colors = (this.Thread.Comment.Like) ? likeOn : likeOff;
            }
        
            SetLayout();
        }

        public void SetClipImage(Texture2D texture)
        {
            clipImage.SetTexture(texture);
        }

        void OnScreenSizeChanged()
        {
            StartCoroutine(delaySetLayout());
        }

        IEnumerator delaySetLayout()
        {
            yield return 1;

            SetLayout();
        }

        public float Height { get; protected set; }
        
        float commentHeight = 0f;

        void SetLayout()
        {
            if (clipImage.gameObject.activeSelf)
            {
                clipImageRectTransform = clipImage.gameObject.GetComponent<RectTransform>();

                if (comment.gameObject.activeSelf)
                {
					commentHeight = Mathf.Min(discriptionTextSetter.truncateHeight, comment.preferredHeight);

					clipImageRectTransform.anchoredPosition = new Vector2(0f, comment.rectTransform.anchoredPosition.y - commentHeight - margin);

                    buttomButtonRectTransform.anchoredPosition = new Vector2(0f, clipImageRectTransform.anchoredPosition.y - clipImageRectTransform.sizeDelta.y - margin);

                    Height = - buttomButtonRectTransform.anchoredPosition.y + buttomButtonRectTransform.sizeDelta.y;
                }
                else
                {
                    clipImageRectTransform.anchoredPosition = new Vector2(0f, cellTopRectTransform.anchoredPosition.y - cellTopRectTransform.sizeDelta.y - margin);

                    buttomButtonRectTransform.anchoredPosition = new Vector2(0f, clipImageRectTransform.anchoredPosition.y - clipImageRectTransform.sizeDelta.y - margin);

                    Height = -buttomButtonRectTransform.anchoredPosition.y + buttomButtonRectTransform.sizeDelta.y;
                }
            }
            else
            {
				commentHeight = Mathf.Min(discriptionTextSetter.truncateHeight, comment.preferredHeight);

				buttomButtonRectTransform.anchoredPosition = new Vector2(0f, comment.rectTransform.anchoredPosition.y - commentHeight - margin);

                Height = -buttomButtonRectTransform.anchoredPosition.y + buttomButtonRectTransform.sizeDelta.y;
            }

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Height);

            subscribeButtonImage.color = Thread.Subscribed ? subscribeColor : unsubscribeColor;
        }

        IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (Thread != null)
                {
                    updatedAt.text = AUIUtility.CurrentTimeSpan(Thread.UpdateAt);

                    yield return new WaitForSeconds(60f);
                }
                else
                {
                    yield return 1;
                }
            }
        }

        public void OnClickGoToThread()
        {
            auiForum.GoToThread(this, Thread.Comment, true, false);
        }

        public ColorBlock likeOn, likeOff;

        public Button buttonLike;

        public void OnClickLike()
        {
            this.Thread.Comment.Like = !this.Thread.Comment.Like;

            if (this.Thread.Comment.Like)
            {
                Thread.Comment.LikeCount++;

                StartCoroutine(ThreadLikeCoroutine());
            }
            else
            {
                if (Thread.Comment.LikeCount > 0) Thread.Comment.LikeCount--;

                StartCoroutine(ThreadUnlikeCoroutine());
            }

            buttonLike.colors = (this.Thread.Comment.Like) ? likeOn : likeOff;

            likeCount.text = this.Thread.Comment.LikeCount.ToString();

            //buttonLike.image.color = buttonLike.colors.normalColor;
        }

        IEnumerator ThreadLikeCoroutine()
        {
            while (string.IsNullOrEmpty(Thread.Id)) yield return 1;

            FASForum.LikeComment(Thread.Comment.Id, (comment, error) =>
            {
                if (error != null)
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                        Debug.LogError(error.ToString());
                }
                else
                {
                    this.Thread.Comment = comment;
                }

            });

            if (Thread.Comment.Video != null)
            {
                FASVideo.LikeVideo(Thread.Comment.Video.Id, (video, error) =>
                {
                    if (error != null)
                    {
                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                            Debug.LogError(error.ToString());
                    }
                });
            }
        }

        IEnumerator ThreadUnlikeCoroutine()
        {
            while (string.IsNullOrEmpty(Thread.Id)) yield return 1;

            FASForum.UnlikeComment(Thread.Comment.Id, (error) =>
            {
                if (error != null)
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                        Debug.LogError(error.ToString());
                }

            });

            if (Thread.Comment.Video != null)
            {
                FASVideo.UnlikeVideo(Thread.Comment.Video.Id, (video, error) =>
                {
                    if (error != null)
                    {
                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                            Debug.LogError(error.ToString());
                    }
                });
            }

        }

        public RectTransform buttonSubscribeCenter;

        public Button buttonSubscribe;

        public void OnClickSubscribeButton()
        {
            buttonSubscribe.interactable = false;

            List<string> buttons = new List<string>();

            if (this.Thread.Subscribed)
            {
                buttons.Add(FASText.Get("Unsubscribe"));
            }
            else
            {
                buttons.Add(FASText.Get("Subscribe"));
            }

            if (this.Thread.User.Id == FAS.CurrentUser.Id)
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

            StartCoroutine(SubscribeCoroutine());
        }

        private IEnumerator SubscribeCoroutine()
        {
            while (string.IsNullOrEmpty(Thread.Id)) yield return 1;

            FASForum.Subscribe(Thread.Id, (thread, error) =>
            {
                buttonSubscribe.interactable = true;

                if (error == null)
                {
                    this.Thread = thread;
                }
                else
                {
                    subscribeButtonImage.color = Thread.Subscribed ? subscribeColor : unsubscribeColor;

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del)=>{ });
                }
            });
        }

        private void Unsubscribe()
        {
            subscribeButtonImage.color = unsubscribeColor;

            StartCoroutine(UnsubscribeCoroutine());
        }

        private IEnumerator UnsubscribeCoroutine()
        {
            while (string.IsNullOrEmpty(Thread.Id)) yield return 1;

            FASForum.Unsubscribe(Thread.Id, (error)=>
            {
                buttonSubscribe.interactable = true;

                if (error == null)
                {
                    Thread.Subscribed = false;
                }
                else
                {
                    subscribeButtonImage.color = Thread.Subscribed ? subscribeColor : unsubscribeColor;

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), delegate(bool del) { });
                }
            });
        }

        public void DeleteThread()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), delegate(bool del) { });

                buttonSubscribe.interactable = true;

                return;
            }

            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Delete"), FASText.Get("Cancel"), FASText.Get("Close"));

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmDeleteThread"), (del) =>
            {
                if (del)
                {
                    StartCoroutine(DeleteThreadCoroutine());
                }
                else
                {
                    buttonSubscribe.interactable = true;
                }
            });
        }

        private IEnumerator DeleteThreadCoroutine()
        {
            while (string.IsNullOrEmpty(Thread.Id)) yield return 1;

            FASForum.DeleteThread(Thread.Id, (error)=>
            {
                if (error != null)
                {
                    buttonSubscribe.interactable = true;

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                }
                else
                {
                    StartCoroutine(DeleteCellCoroutine());
                }
            });
        }

        IEnumerator DeleteCellCoroutine()
        {
            fade.gameObject.SetActive(true);

            fade.CrossFadeAlpha(0f, 0f, true);

            yield return 1;

            fade.CrossFadeAlpha(1f, 0.5f, true);

            yield return new WaitForSeconds(0.5f);

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            iTween.ValueTo(this.gameObject, iTween.Hash("time", 0.5f, "from", Height, "to", 0f, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "OnUpdateDeleteCell", "oncomplete", "OnCompleteDeleteCell"));
        }

        void OnUpdateDeleteCell(float value)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, value);

            auiForum.ReLayout();
        }

        void OnCompleteDeleteCell()
        {
            auiForum.RemoveCell(this);
        }

        public void PlayVideo()
        {
            Thread.Comment.Video.User = Thread.Comment.User;

            FASVideo.Play(Thread.Comment.Video, (_video, button) => 
            { 
                Thread.Comment.Video = _video;

                if (button == Util.MoviePlayer.TappedButton.User)
                {
                    auiForum.GoToUserPage(Thread.Comment.User);
                }
                else if (button == Util.MoviePlayer.TappedButton.App)
                {
                    FASGui.BackToGameScene();
                }
            });

            FASVideo.IncrementVideoPlaybackCount(Thread.Comment.Video.Id, (video, error) =>
            {
                if (error == null)
                {
                    Thread.Comment.Video = video;
                }
            });
        }
    }
}
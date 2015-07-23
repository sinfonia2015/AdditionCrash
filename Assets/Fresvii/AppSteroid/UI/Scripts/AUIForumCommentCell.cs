using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIForumCommentCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Comment Comment { get; protected set; }

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public Text updatedAt;

        public Text likeCount;

        public Text commentCount;

        public Text commentText;

        public AUIRawImageTextureSetter clipImage;

        private RectTransform clipImageRectTransform;

        private RectTransform userIconRectTransform;

        public Texture2D videoRemoved;

        public float margin;

        public Image fade;

        private string includingUrl;

        private RectTransform rectTransform;

        [HideInInspector]
        public AUIThread ThreadManager;

        public GameObject viewCounts;

        public Color linkTextColor;

        public GameObject buttonVideoPlayback;

        public GameObject commentCountObjs;

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

        public void SetComment(Fresvii.AppSteroid.Models.Comment comment)
        {
            clipImage.delayCount = 2;

            buttonImage.interactable = false;

            this.Comment = comment;

            if (string.IsNullOrEmpty(Comment.Text))
            {
                commentText.gameObject.SetActive(false);

                commentText.rectTransform.sizeDelta = Vector2.zero;
            }
            else
            {
                commentText.gameObject.SetActive(true);

                string coloredText = Comment.Text;

                string hex = Fresvii.AppSteroid.Gui.FresviiGUIUtility.ColorToHex(linkTextColor);

                List<string> urls = Fresvii.AppSteroid.Gui.FresviiGUIUtility.GetUrls(Comment.Text);

                foreach (string url in urls)
                {
                    coloredText = coloredText.Insert(coloredText.IndexOf(url), "<color=" + hex + ">");

                    coloredText = coloredText.Insert(coloredText.IndexOf(url) + url.Length, "</color>");

                    includingUrl = url;
                }

                commentText.text = coloredText;
            }

            if (Comment.User != null)
            {
                userIcon.Set(Comment.User.ProfileImageUrl);

                userName.text = Comment.User.Name;

                likeCount.text = Comment.LikeCount.ToString();

                if (Comment.VideoState == Fresvii.AppSteroid.Models.Comment.VideoStatus.Ready)
                {
                    if (Comment.Video != null)
                    {
                        clipImage.Set(Comment.Video.ThumbnailUrl);

                        //viewCount.text = Comment.Video.PlaybackCount.ToString();

                        buttonVideoPlayback.SetActive(!string.IsNullOrEmpty(Comment.Video.VideoUrl));
                    }
                }
                else if (Comment.VideoState == Fresvii.AppSteroid.Models.Comment.VideoStatus.Removed)
                {
                    clipImage.SetTexture(videoRemoved);
                }
                else if (!string.IsNullOrEmpty(Comment.ImageThumbnailUrl))
                {
                    buttonImage.interactable = true;

                    clipImage.Set(Comment.ImageThumbnailUrl);
                }
                else
                {
                    clipImage.gameObject.SetActive(clipImage.GetTexture() != null);
                }
            }

            buttonLike.colors = (this.Comment.Like) ? likeOn : likeOff;

            commentCount.text = ThreadManager.ThreadCell.Thread.CommentCount.ToString();

            commentCountObjs.SetActive(ThreadManager.ThreadCell.Thread.Comment.Id == this.Comment.Id);

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
        float commentHeight = 0f;

        public float Height { get; protected set; }

        void SetLayout()
        {
            if (clipImage.gameObject.activeSelf)
            {
                clipImageRectTransform = clipImage.gameObject.GetComponent<RectTransform>();

                if (commentText.gameObject.activeSelf)
                {
                    clipImageRectTransform.anchoredPosition = new Vector2(0f, commentText.rectTransform.anchoredPosition.y - commentText.preferredHeight - margin);

                    commentHeight = commentText.preferredHeight;

                    Height = -clipImageRectTransform.anchoredPosition.y + clipImageRectTransform.sizeDelta.y + margin;

                }
                else
                {
                    userIconRectTransform = userIcon.gameObject.GetComponent<RectTransform>();

                    clipImageRectTransform.anchoredPosition = new Vector2(0f, userIconRectTransform.anchoredPosition.y - userIconRectTransform.sizeDelta.y - 2f * margin);

                    Height = -clipImageRectTransform.anchoredPosition.y + clipImageRectTransform.sizeDelta.y + margin;
                }
            }
            else
            {
                Height = -commentText.rectTransform.anchoredPosition.y + commentText.preferredHeight + margin;
            }

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Height);

            ThreadManager.contents.ReLayout();
        }

        IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (Comment != null)
                {
                    updatedAt.text = AUIUtility.CurrentTimeSpan(Comment.UpdateAt);

                    yield return new WaitForSeconds(60f);
                }
                else
                {
                    yield return 1;
                }
            }
        }

        public void DeleteCell()
        {

        }

        public ColorBlock likeOn, likeOff;

        public Button buttonLike;

        public void OnClickLike()
        {
            this.Comment.Like = !this.Comment.Like;

            if (this.Comment.Like)
            {
                Comment.LikeCount++;

                StartCoroutine(ThreadLikeCoroutine());
            }
            else
            {
                if (Comment.LikeCount > 0) Comment.LikeCount--;

                StartCoroutine(ThreadUnlikeCoroutine());
            }

            buttonLike.colors = (this.Comment.Like) ? likeOn : likeOff;

            likeCount.text = this.Comment.LikeCount.ToString();
        }

        IEnumerator ThreadLikeCoroutine()
        {
            while (string.IsNullOrEmpty(Comment.Id)) yield return 1;

            FASForum.LikeComment(Comment.Id, (comment, error) =>
            {
                if (error != null)
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                        Debug.LogError(error.ToString());
                }
                else
                {
                    this.Comment = comment;
                }

            });

            if (Comment.Video != null)
            {
                FASVideo.LikeVideo(Comment.Video.Id, (video, error) =>
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
            while (string.IsNullOrEmpty(Comment.Id)) yield return 1;

            FASForum.UnlikeComment(Comment.Id, (error) =>
            {
                if (error != null)
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                        Debug.LogError(error.ToString());
                }
            });

            if (Comment.Video != null)
            {
                FASVideo.UnlikeVideo(Comment.Video.Id, (video, error) =>
                {
                    if (error != null)
                    {
                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                            Debug.LogError(error.ToString());
                    }
                });
            }
        }

        public void OnLongPressed()
        {
            List<string> buttons = new List<string>();

            if (!string.IsNullOrEmpty(Comment.Text))
            {
                buttons.Add(FASText.Get("CopyText"));
            }

            if (clipImage.GetTexture() != null)
            {
                buttons.Add(FASText.Get("SaveImage"));
            }

            if(Comment.User != null && FAS.CurrentUser != null)
            {
                if (Comment.User.Id == FAS.CurrentUser.Id)
                {
                    buttons.Add(FASText.Get("EditText"));

                    if (clipImage.GetTexture() != null)
                    {
                        buttons.Add(FASText.Get("ChangeImageFromLibrary"));

                        buttons.Add(FASText.Get("ChangeImageFromCamera"));
                    }
                    else
                    {
                        buttons.Add(FASText.Get("AddImageFromLibrary"));

                        buttons.Add(FASText.Get("AddImageFromCamera"));
                    }

                    if (Comment.Id != ThreadManager.ThreadCell.Thread.Comment.Id)
                    {
                        buttons.Add(FASText.Get("Delete"));
                    }
                }
            }

            if (!string.IsNullOrEmpty(includingUrl))
            {
                buttons.Add(FASText.Get("OpenLink"));
            }

            buttons.Add(FASText.Get("Report"));

            AUIActionSheet.Show(buttons.ToArray(), (selectedButton) =>
            {
                if (selectedButton == FASText.Get("CopyText")) // copy
                {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS) 
					Fresvii.AppSteroid.Util.Clipboard.SetText(Comment.Text);
#endif
                }
                else if (selectedButton == FASText.Get("SaveImage"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.SaveImageData(this, clipImage.GetTexture().EncodeToPNG(), System.IO.Path.GetFileName(Comment.ImageThumbnailUrl));
                }
                else if (selectedButton == FASText.Get("EditText")) // edit
                {
                    AUIKeyboardInput.Show(Comment.Text, true, (text) =>
                    {
                        string origText = Comment.Text;

                        Comment.Text = text;

                        SetComment(Comment);

                        FASForum.EditComment(Comment.Id, text, "", (comment, error)=>
                        {
                            if (error == null)
                            {
                                this.Comment = comment;
                            }
                            else if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.WWWTimeOut)
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
                            else if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.NotFound || (Application.platform == RuntimePlatform.Android && error.Detail.IndexOf("FileNotFound") >= 0))
                            {
                                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("ThreadNone"), delegate(bool del) { });
                            }
                            else
                            {
                                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("CommentCreateError"), delegate(bool del) { });

                                Comment.Text = origText;
                            }

                            SetComment(Comment);
                        });
                    });
                }
                else if (selectedButton == FASText.Get("ChangeImageFromLibrary") || selectedButton == FASText.Get("ChangeImageFromCamera")
                    || selectedButton == FASText.Get("AddImageFromLibrary") || selectedButton == FASText.Get("AddImageFromCamera"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.Type type = (selectedButton == FASText.Get("ChangeImageFromLibrary") || selectedButton == FASText.Get("AddImageFromLibrary")) ? Fresvii.AppSteroid.Util.ImagePicker.Type.Gallery : Fresvii.AppSteroid.Util.ImagePicker.Type.Camera;

                    Fresvii.AppSteroid.Util.ImagePicker.Show(this, type, (image) =>
                    {
                        Texture2D origImage = clipImage.GetTexture();

                        if (image != null)
                        {
                            clipImage.SetTexture(image);

                            SetComment(Comment);

                            Fresvii.AppSteroid.Util.DialogManager.ShowProgressSpinnerDialog("", FASText.Get("Uploading"), false);

                            FASForum.EditComment(Comment.Id, "", image, (comment, error) =>
                            {
                                Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();
                                
                                if (error == null)
                                {
                                    if (!string.IsNullOrEmpty(Comment.ImageThumbnailUrl))
                                    {
                                        Fresvii.AppSteroid.Util.ResourceManager.Instance.ReleaseTexture(Comment.ImageThumbnailUrl);
                                    }

                                    this.Comment = comment;

                                    SetComment(Comment);
                                }
                                else if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.WWWTimeOut)
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
                                else if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.NotFound || (Application.platform == RuntimePlatform.Android && error.Detail.IndexOf("FileNotFound") >= 0))
                                {
                                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("ThreadNone"), delegate(bool del) { });

                                    // Implement 
                                    // Delete Thread NonConfirm
                                }
                                else
                                {
                                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("CommentEditError"), delegate(bool del) { });

                                    clipImage.SetTexture(origImage);
                                }
                            });
                        }
                    });
                }
                else if (selectedButton == FASText.Get("Delete"))
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Delete"), FASText.Get("Cancel"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmDeleteComment"), (del)=>
                    {
                        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
                        {
                            if (ThreadManager.ThreadCell.Thread.CommentCount >= 1)
                            {
                                ThreadManager.ThreadCell.Thread.CommentCount -= 1;
                            }

                            DeleteComment(false);
                        }
                        else
                        {
                            if (del)
                            {
                                if (ThreadManager.ThreadCell.Thread.CommentCount >= 1)
                                {
                                    ThreadManager.ThreadCell.Thread.CommentCount -= 1;
                                }

                                DeleteComment(false);
                            }
                        }
                    });
                }
                else if (selectedButton == FASText.Get("Report"))
                {
                    Report();
                }
                else if (selectedButton == FASText.Get("OpenLink"))
                {
                    Application.OpenURL(includingUrl);
                }
            });
        }

        public void OnClickUserIcon()
        {
            ThreadManager.GoToUserPage(this.Comment.User);
        }

        public void DeleteComment(bool showAlert)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), delegate(bool del) { });

                return;
            }

            if (showAlert)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Delete"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmDeleteComment"), (del) =>
                {
                    if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        StartCoroutine(DeleteCommentCoroutine());
                    }
                    else
                    {
                        if (del)
                        {
                            StartCoroutine(DeleteCommentCoroutine());
                        }
                    }
                });

            }
            else
            {
                StartCoroutine(DeleteCommentCoroutine());
            }
        }

        IEnumerator DeleteCommentCoroutine()
        {
            while (string.IsNullOrEmpty(Comment.Id)) yield return 1;

            FASForum.DeleteComment(Comment.Id, (error) =>
            {
                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });

                    Debug.LogError(error.ToString());
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

            ThreadManager.contents.ReLayout();
        }

        void OnCompleteDeleteCell()
        {
            ThreadManager.RemoveCell(this);
        }

        public void Report()
        {
            List<string> buttons = new List<string>();

            foreach (Fresvii.AppSteroid.Services.ForumService.ReportKind kind in System.Enum.GetValues(typeof(Fresvii.AppSteroid.Services.ForumService.ReportKind)))
            {
                buttons.Add(FASText.Get(kind.ToString()));
            }

            AUIActionSheet.Show(buttons.ToArray(), (selectedButton) =>
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (selectedButton == FASText.Get(((Fresvii.AppSteroid.Services.ForumService.ReportKind)i).ToString()))
                    {
                        FASForum.ReportComment(Comment.Id, (Fresvii.AppSteroid.Services.ForumService.ReportKind)i, "", (comment, error) => 
                        {
                            if (error != null && error.Code != 402) // 402
                            {
                                Debug.LogError("ReportComment :" + error.ToString());

                                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("OK"), FASText.Get("OK"), (del) => { });
                            }
                            else
                            {
                                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("ReportSent"), FASText.Get("OK"), FASText.Get("OK"), FASText.Get("OK"), (del) => { });
                            }                        
                        });

                        return;
                    }
                }
            });
        }

        public void PlayVideo()
        {
            Comment.Video.User = Comment.User;

            FASVideo.Play(Comment.Video, (_video, button) => 
            { 
                Comment.Video = _video;

				SetComment(Comment);

				if (button == Util.MoviePlayer.TappedButton.User)
                {
                    ThreadManager.GoToUserPage(Comment.User);
                }
            });

            FASVideo.IncrementVideoPlaybackCount(Comment.Video.Id, (video, error) =>
            {
                if (error == null)
                {
                    Comment.Video = video;

					SetComment(Comment);
                }
            });
        }

        public Button buttonImage;

        public void ShowImage()
        {
            AUIImageViewer.Show(this.Comment.ImageUrl, () => { });
        }
    }
}
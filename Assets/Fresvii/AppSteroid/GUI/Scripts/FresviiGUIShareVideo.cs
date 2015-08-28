using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fresvii.AppSteroid.Gui
{
    public class FresviiGUIShareVideo : MonoBehaviour
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern string _FasGetVideoThumbnailImagePath(string videoFilePath);
#endif

        public Vector2 portraitMargin, landscapeMargin;

        Fresvii.AppSteroid.Models.Video video;

        Texture2D videoThumbnail;

        string videoPath;

        public RectTransform rectTransform;

        public RawImage videoThumbnailImage;

        private bool goToUploaded = false;

        public void Set(FresviiGUIVideoSharing videoSharing, Fresvii.AppSteroid.Models.Video video, Texture2D videoThumbnail)
        {
            this.gameObject.SetActive(true);

            this.video = video;

            this.videoThumbnail = videoThumbnail;
        }

        public void Set(FresviiGUIVideoSharing videoSharing, string videoPath)
        {
            this.gameObject.SetActive(true);

            this.videoPath = videoPath;
        }

        IEnumerator Start()
        {
            while (!FASUser.IsLoggedIn())
            {
                yield return 1;
            }

            if (string.IsNullOrEmpty(videoPath))
            {
                videoThumbnailImage.material.mainTexture = videoThumbnail;

                shareButton.interactable = false;
            }
            else
            {
                StartCoroutine(GetVideoThumbnailCoroutine(videoPath));
            }
        }

        private IEnumerator GetVideoThumbnailCoroutine(string videoFilePath)
        {

#if UNITY_IOS

			string thumbnailPath = _FasGetVideoThumbnailImagePath(videoFilePath);

			yield return new WaitForSeconds(0.1f);

			Texture2D thumbnail = new Texture2D(4,4);
			
			byte[] imageBytes = System.IO.File.ReadAllBytes(thumbnailPath);
			
			thumbnail.LoadImage(imageBytes);

            this.videoThumbnail = thumbnail;

			this.videoThumbnailImage.material.mainTexture = thumbnail;

			if(thumbnail.width > thumbnail.height)
            {
				float offsetPixelX = 0.5f * (thumbnail.width - thumbnail.height);

                float offsetX = offsetPixelX / thumbnail.width;
				
                float scaleX = (thumbnail.width - 2f * offsetPixelX) / thumbnail.width;
				
                this.videoThumbnailImage.material.mainTextureOffset = new Vector2(offsetX, 0f);
				
                this.videoThumbnailImage.material.mainTextureScale = new Vector2(scaleX, 1f);
			}
			else
            {
				float offsetPixelY = 0.5f * (thumbnail.height - thumbnail.width);
			
                float offsetY = offsetPixelY / thumbnail.height;
				
                float scaleY = (thumbnail.height - 2f * offsetPixelY) / thumbnail.height;
				
                this.videoThumbnailImage.material.mainTextureOffset = new Vector2(0f, offsetY);
				
                this.videoThumbnailImage.material.mainTextureScale = new Vector2(1f, scaleY);
			}

            Debug.Log("GetVideoThumbnailCoroutine done");

#else
            yield return 1;
#endif
        }

        private int screenWidth;

        void Update()
        {
            if (screenWidth != Screen.width)
            {
                screenWidth = Screen.width;

                if (Screen.width > Screen.height)
                {
                    rectTransform.offsetMin = new Vector2(landscapeMargin.x, landscapeMargin.y);

                    rectTransform.offsetMax = new Vector2(-landscapeMargin.x, -landscapeMargin.y);
                }
                else
                {
                    rectTransform.offsetMin = new Vector2(portraitMargin.x, portraitMargin.y);

                    rectTransform.offsetMax = new Vector2(-portraitMargin.x, -portraitMargin.y);
                }
            }
        }

        public void Close()
        {
            if (isUploading) return;

            FresviiGUIVideoSharing.Hide();
        }

        void OnDestroy()
        {
            if (!string.IsNullOrEmpty(videoPath))
            {
                if (!goToUploaded)
                {
                    Destroy(videoThumbnail);
                }

                if (System.IO.File.Exists(videoPath))
                {
                    System.IO.File.Delete(videoPath);
                }
            }
        }

        bool wantToTweet = false;

        private static readonly int TweetMaxLength = 116;

        public void OnClickShareButton(int type)
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                return;
            }

            shareServiceButtons[type].IsOn = !shareServiceButtons[type].IsOn;

            if (string.IsNullOrEmpty(videoPath))
            {
                shareButton.interactable = (shareServiceButtons[0].IsOn) || (shareServiceButtons[1].IsOn) || (shareServiceButtons[2].IsOn) || (shareServiceButtons[3].IsOn);
            }

            if (type == 0) // Facebook
            {
                if (!Fresvii.AppSteroid.Util.SocialNetworkingService.IsFacebookEnable())
                {
                    Debug.LogError("FASSNS.ShareFacebook : error");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("FacebookError"), (del) => { });

                    shareServiceButtons[type].IsOn = !shareServiceButtons[type].IsOn;

                    return;
                }
            }
            else if (type == 1) // Twitter
            {
                if (!Fresvii.AppSteroid.Util.SocialNetworkingService.IsTwitterEnable())
                {
                    Debug.LogError("FASSNS.ShareTwitter : error");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("TwitterError"), (del) => { });

                    shareServiceButtons[type].IsOn = !shareServiceButtons[type].IsOn;

                    return;
                }
            }

            wantToTweet = shareServiceButtons[1].IsOn;

            if (shareServiceButtons[1].IsOn && (commentInputField.text.Length > TweetMaxLength))
            {
                shareServiceButtons[1].IsOn = false;

                Debug.LogError("FASSNS.ShareTwitter : text too long");

                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("TweetIsTooLong") + commentInputField.text.Length + ".", (del) => { });
            }
        }

        public void Progress(float progress)
        {
            progressBar.fillAmount = progress;
        }

        public Button shareButton;

        public Text shareButtonText;

        void OnEmailDone(string msg)
        {
            shareProcessing = false;

            if (msg == "Sent")
            {
                isSomethingSent = true;
            }
        }

        private bool tweetOverAlerted = false;

        private bool isUploading;

        public void OnEditInputField()
        {
            if (wantToTweet && (commentInputField.text.Length > TweetMaxLength) && !tweetOverAlerted)
            {
                tweetOverAlerted = true;

                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("OK"), FresviiGUIText.Get("Cancel"), FresviiGUIText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("TweetOvered"), delegate(bool del) { });
            }

            if (commentInputField.text.Length <= TweetMaxLength)
            {
                tweetOverAlerted = false;
            }

            shareServiceButtons[1].IsOn = wantToTweet && (commentInputField.text.Length <= TweetMaxLength);
        }

        public void OnClickShare()
        {
            if (isUploading) return;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("OK"), FresviiGUIText.Get("Cancel"), FresviiGUIText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("Offline"), (del)=>{ });

                return;
            }

            isUploading = true;

            if (video == null)
            {

#if UNITY_EDITOR
                OnCompleteUploadVideo(null, null);
#else
                FASVideo.UploadVideo(videoPath, null, commentInputField.text, OnCompleteUploadVideo, Progress);
#endif
                shareButtonText.text = "Uploading...";
            }
            else
            {
                OnCompleteUploadVideo(video, null);
            }

            shareButton.interactable = false;

            commentInputField.interactable = false;

            foreach (FresviiGUIToggleButton toggleButton in shareServiceButtons)
            {
                toggleButton.button.interactable = false;
            }
        }

        void OnCompleteUploadVideo(Fresvii.AppSteroid.Models.Video video, Fresvii.AppSteroid.Models.Error error)
        {
            isUploading = false;

            if (error != null)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("Yes"), FresviiGUIText.Get("No"), FresviiGUIText.Get("Cancel"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FresviiGUIText.Get("VideoUploadErrorAndRetry"),

                    (del) =>
                    {
                        if (del)
                        {
                            OnClickShare();
                        }
                        else
                        {
                            if (System.IO.File.Exists(videoPath))
                            {
                                System.IO.File.Delete(videoPath);
                            }

                            FresviiGUIVideoSharing.Hide();

                            FASGesture.Resume();
                        }
                    });

                if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }
            }
            else
            {
                StartCoroutine(ShareCoroutine(video));
            }
        }

        private bool shareProcessing;

        private bool coroutineRunning = false;

        bool isSomethingSent = false;

        public FresviiGUIUploadedVideo guiVideoUploaded;

        public GameObject toneDownUpper, toneDownBottom;

        public FresviiGUIToggleButton[] shareServiceButtons;

        public InputField commentInputField;

        public Image progressBar;

        public Button buttonClose;

        IEnumerator ShareCoroutine(Fresvii.AppSteroid.Models.Video video)
        {
            buttonClose.interactable = false;

            if (coroutineRunning)
            {
                yield break;
            }

            coroutineRunning = true;

            byte[] videoData = null;

            string tmpFilePath = "";

            isSomethingSent = false;

            //  Facebook
            if (shareServiceButtons[0].IsOn)
            {
                shareProcessing = true;

                Fresvii.AppSteroid.Util.SocialNetworkingService.ShareFacebook(commentInputField.text, video.VideoUrl, (result) =>
                {
                    shareProcessing = false;

                    if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Done)
                    {
                        isSomethingSent = true;
                    }

                    if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                    {
                        Debug.LogError("FASSNS.ShareFacebook : error");

                        Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("FacebookError"), (del) => { });
                    }

                });
            }

            while (shareProcessing) yield return 1;

            if (System.IO.File.Exists(tmpFilePath))
            {
                System.IO.File.Delete(tmpFilePath);
            }

            // Twitter
            if (shareServiceButtons[1].IsOn)
            {
                shareProcessing = true;

                Fresvii.AppSteroid.Util.SocialNetworkingService.ShareTwitter(commentInputField.text + " " + video.VideoUrl, (result) =>
                {
                    shareProcessing = false;

                    if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Done)
                    {
                        isSomethingSent = true;
                    }
                    else if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("Yes"), FresviiGUIText.Get("No"), FresviiGUIText.Get("Close"));

                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("TweetError"), (del) => { });
                    }
                });
            }

            while (shareProcessing) yield return 1;

            // YouTube
            if (shareServiceButtons[2].IsOn)
            {
                shareProcessing = true;

                shareButtonText.text = FASText.Get("UploadingToYoutube");

                progressBar.fillAmount = 0f;

                Fresvii.AppSteroid.Util.DialogManager.ShowProgressSpinnerDialog("", "Uploading ...", false, true);

                toneDownUpper.SetActive(true);

                toneDownBottom.SetActive(false);

                toneDownUpper.GetComponent<RectTransform>().localScale = Vector3.one;

                if (string.IsNullOrEmpty(videoPath) && videoData == null)
                {
                    FAS.Instance.Client.VideoService.DownloadVideo(video.VideoUrl, (data, error) =>
                    {
                        shareProcessing = false;

                        if (error != null)
                        {
                            Debug.LogError("FASSNS.ShareYoutube 1 : " + error.ToString());

                            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog("Youtube Upload Error.", (del) => { });
                        }
                        else
                        {
                            shareProcessing = true;

                            Fresvii.AppSteroid.Util.SocialNetworkingService.ShareYoutube(data, FASConfig.Instance.appName, commentInputField.text,

                                (result) =>
                                {
                                    shareProcessing = false;

                                    if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                                    {
                                        Debug.LogError("FASSNS.ShareYoutube 2 : Error");

                                        Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog("Youtube Upload Error.", (del) => { });
                                    }
                                    else if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Done)
                                    {
                                        isSomethingSent = true;
                                    }
                                },

                                null

                            );
                        }
                    });
                }
                else if (string.IsNullOrEmpty(videoPath) && videoData != null)
                {
                    Fresvii.AppSteroid.Util.SocialNetworkingService.ShareYoutube(videoData, FASConfig.Instance.appName, commentInputField.text,

                    (result) =>
                    {
                        shareProcessing = false;

                        if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                        {
                            Debug.LogError("FASSNS.ShareYoutube : Error");

                            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog("Youtube Upload Error.", (del) => { });
                        }
                        else if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Done)
                        {
                            isSomethingSent = true;
                        }
                    },

                    null

                    );
                }
                else
                {
                    Fresvii.AppSteroid.Util.SocialNetworkingService.ShareYoutube(videoPath, FASConfig.Instance.appName, commentInputField.text,

                        (result) =>
                        {
                            shareProcessing = false;

                            if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                            {
                                Debug.LogError("FASSNS.ShareYoutube : Error");

                                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog("Youtube Upload Error.", (del) => { });
                            }
                            else if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Done)
                            {
                                isSomethingSent = true;
                            }
                        },

                        null

                    );
                }
            }

            while (shareProcessing)
            {
                yield return 1;
            }

            toneDownUpper.SetActive(false);

            toneDownUpper.GetComponent<RectTransform>().localScale = Vector3.zero;

            toneDownBottom.SetActive(true);

            Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

            // Email
            if (shareServiceButtons[3].IsOn)
            {
#if UNITY_EDITOR

#elif UNITY_IOS 
			    string body = commentInputField.text + "\n\n" + video.VideoUrl;

                Fresvii.AppSteroid.Util.Email.Send(FresviiGUIText.Get("VideoSharingEmailSubjectPrefix") + FASSettings.Instance.appName, body, "", this.gameObject.name);
#elif UNITY_ANDROID 

#endif

                shareProcessing = true;
            }


            while (shareProcessing)
            {
                yield return 1;
            }


            this.videoThumbnailImage.material.mainTexture = null;

            if (!string.IsNullOrEmpty(videoPath))
            {
                goToUploaded = true;

                guiVideoUploaded.Set(videoThumbnail, video);

                this.gameObject.SetActive(false);
            }
            else
            {
                if (isSomethingSent)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog("Video sent", (del) => { });
                }
            
                FresviiGUIVideoSharing.Hide();
            }

            coroutineRunning = false;
        }
    }
}

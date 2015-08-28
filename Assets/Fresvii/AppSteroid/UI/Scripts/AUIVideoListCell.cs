using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIVideoListCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Video Video { get; protected set; }

        AUIVideoList.Mode mode;

        public Action<Fresvii.AppSteroid.Models.Video> videoSelectCallback;

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public Text updatedAt;

        public Text likeCount;

        public Text viewedCount;

        public Text uploadDate, duration;

        public AUIRawImageTextureSetter thumbnail;

        public Texture2D videoRemoved;

        public RectTransform buttonMenuCenter;

        public Button buttonMenu;

        public Button buttonShare, buttonSelect;

        public AUICellDeleteAnimator cellDeleteAnimator;

        [HideInInspector]
        public AUIVideoList auiVideoList { get; set; }

        public Image fade;

        private bool selected;

        void Awake()
        {
            fade.gameObject.SetActive(false);

            RectTransform rectTransform = GetComponent<RectTransform>();

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f));
        }

        void OnEnable()
        {
            StartCoroutine(UpdateUpdatedAt());
        }

        void OnDisable()
        {

        }

        void OnDestroy()
        {

        }

        public void SetVideo(Fresvii.AppSteroid.Models.Video video, AUIVideoList.Mode mode)
        {
            this.Video = video;

            this.mode = mode;

            buttonShare.gameObject.SetActive(mode == AUIVideoList.Mode.Share);

            buttonSelect.gameObject.SetActive(mode == AUIVideoList.Mode.Select);

            if (Video.User != null)
            {
                buttonMenu.gameObject.SetActive(Video.User.Id == FAS.CurrentUser.Id);

                userIcon.Set(Video.User.ProfileImageUrl);

                userName.text = Video.User.Name;

                likeCount.text = Video.LikeCount.ToString();

                viewedCount.text = Video.PlaybackCount.ToString();

                thumbnail.Set(Video.ThumbnailUrl);

                uint min = video.Duration / 60;

                uint sec = video.Duration % 60;

                uploadDate.text = video.CreatedAt.ToShortDateString() + " " + FASText.Get("Uploaded");

                duration.text = min + ":" + sec.ToString("00");
            }
        }

        public float Height { get; protected set; }

        IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (Video != null)
                {
                    updatedAt.text = AUIUtility.CurrentTimeSpan(Video.CreatedAt);

                    yield return new WaitForSeconds(60f);
                }
                else
                {
                    yield return 1;
                }
            }
        }

        public void OnClickMenuButton()
        {
            buttonMenu.interactable = false;

            List<string> buttons = new List<string>();

            buttons.Add(FASText.Get("CopyToCameraRoll"));

            buttons.Add(FASText.Get("Delete"));
 
            buttons.Add(FASText.Get("Cancel"));

            AUIPopUpBalloon.Show(buttons.ToArray(), buttonMenuCenter, (selected) =>
            {
                buttonMenu.interactable = true;

                if (selected == FASText.Get("CopyToCameraRoll"))
                {
                    FASVideo.DownloadAndCopyToAlbum(Video.VideoUrl, () =>
                    {

                    }
                    ,

                    (progress) =>
                    {
                        
                    }

                    );
                }
                else if (selected == FASText.Get("Delete"))
                {
                    DeleteVideo();
                }
            });
        }

        public void DeleteVideo()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Delete"), FASText.Get("Cancel"), FASText.Get("Close"));

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmDeleteVideo"), (del) =>
            {
                if (del)
                {
                    FASVideo.DeleteVideo(Video.Id, (error)=>
                    {
                        if (error != null)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (d) => { });
                        }
                        else
                        {
                            cellDeleteAnimator.Animate(this.auiVideoList.contents, (size) =>
                            {
                                this.auiVideoList.RemoveCell(this);
                            });
                        }
                    });
                }
            });
        }

        public void Share()
        {
            FASPlayVideo.videoShareGuiMode = FASPlayVideo.VideoShareGuiMode.WithUGUI;

            Fresvii.AppSteroid.Gui.FresviiGUIVideoSharing.Show(int.MinValue + 1000, Video, thumbnail.GetTexture());
        }

        public void Select()
        {
            thumbnail.dontDestroy = true;

            auiVideoList.VideoSelected(this.Video, thumbnail.GetTexture());
        }

        public void PlayVideo()
        {
            FASVideo.Play(Video, (_video, button) => 
            { 
                Video = _video;

                SetVideo(this.Video, this.mode);

                if (button == Fresvii.AppSteroid.Util.MoviePlayer.TappedButton.User)
                {
                    auiVideoList.GoToUserPage(this.Video.User);
                }
            });

            FASVideo.IncrementVideoPlaybackCount(Video.Id, (video, error) =>
            {
                if (error == null)
                {
                    Video = video;

                    SetVideo(this.Video, this.mode);
                }
            });
        }
    }
}
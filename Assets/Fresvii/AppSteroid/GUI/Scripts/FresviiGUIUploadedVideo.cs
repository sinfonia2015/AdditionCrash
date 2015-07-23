using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fresvii.AppSteroid.Gui
{
    public class FresviiGUIUploadedVideo : MonoBehaviour
    {
        Fresvii.AppSteroid.Models.Video video;

        public Text textUrl;

        public Text textCommunity;

        public RawImage videoThumbnailImage;

        Texture2D videoThumbnail;

        public void Set(Texture2D videoThumbnail, Fresvii.AppSteroid.Models.Video video)
        {
            this.gameObject.SetActive(true);

            this.video = video;

            this.videoThumbnail = videoThumbnail;

            textCommunity.text = FASConfig.Instance.appName + " Community";

            this.videoThumbnailImage.material.mainTexture = videoThumbnail;

            if (videoThumbnail.width * 0.6f > videoThumbnail.height)
            {
                float offsetPixelX = 0.5f * (videoThumbnail.width - videoThumbnail.height * 5f / 3f);

                float offsetX = offsetPixelX / videoThumbnail.width;

                float scaleX = (videoThumbnail.width - 2f * offsetPixelX) / videoThumbnail.width;

                this.videoThumbnailImage.material.mainTextureOffset = new Vector2(offsetX, 0f);

                this.videoThumbnailImage.material.mainTextureScale = new Vector2(scaleX, 1f);
            }
            else
            {
                float offsetPixelY = 0.5f * (videoThumbnail.height - videoThumbnail.width * 0.6f);

                float offsetY = offsetPixelY / videoThumbnail.height;

                float scaleY = (videoThumbnail.height - 2f * offsetPixelY) / videoThumbnail.height;

                this.videoThumbnailImage.material.mainTextureOffset = new Vector2(0f, offsetY);

                this.videoThumbnailImage.material.mainTextureScale = new Vector2(1f, scaleY);
            }

            textUrl.text = video.VideoUrl;
        }

        private int screenWidth;

        public Vector2 portraitMargin, landscapeMargin;

        public RectTransform rectTransform;

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

        void OnDestroy()
        {
            FASGesture.Resume();

            FASPlayVideo.GuiEndCallback();

            if (videoThumbnail != null) 
            {
                Destroy(videoThumbnail);
            }
        }

        public void OnClickURLCopy()
        {
            Fresvii.AppSteroid.Util.Clipboard.SetText(video.VideoUrl);
        }

        public void Close()
        {
            FresviiGUIVideoSharing.Hide();
        }

        public void OnClickCommunityButton()
        {
            FASGui.ShowGUI(FASGui.Mode.All, FASGui.ReturnSceneName, FASGui.Mode.Forum);
        }

        public void OnClickVideoListButton()
        {            
            FresviiGUIMyProfile.StartPage = FresviiGUIMyProfile.Page.VideoList;

            Fresvii.AppSteroid.UI.AUIMyPage.StartPage = UI.AUIMyPage.Page.VideoList;

            FASGui.ShowGUI(FASGui.Mode.All, FASGui.ReturnSceneName, FASGui.Mode.MyProfile);
        }
    }
}
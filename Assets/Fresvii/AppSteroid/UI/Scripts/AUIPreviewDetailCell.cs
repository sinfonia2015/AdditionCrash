using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIPreviewDetailCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Video Video;

        public AUIRawImageTextureSetter videoThumbnail;

        public AUIRawImageTextureSetter appIcon;

        public RectTransform infoPanel;

        public RectTransform cellRectTransform;

        public Text textAppName, textGameCategory;

        public float margin = 30f;

        private AUIPreviewVideoList auiPreviewVideoList;

        public bool DetaiButtonEnabled = true;

        public Text uploadedText, durationText;

        public void Awake()
        {
            RectTransform rect = videoThumbnail.gameObject.GetComponent<RectTransform>();

            rect.sizeDelta *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

            infoPanel.sizeDelta = new Vector2(infoPanel.sizeDelta.x * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f), infoPanel.sizeDelta.y);

            infoPanel.anchoredPosition = new Vector2(infoPanel.anchoredPosition.x, rect.anchoredPosition.y - rect.sizeDelta.y - margin);

            cellRectTransform.sizeDelta = new Vector2(cellRectTransform.sizeDelta.x, -infoPanel.anchoredPosition.y + infoPanel.sizeDelta.y + margin);
        }

        public GameObject buttonDetail;

        void Start()
        {
            buttonDetail.SetActive(DetaiButtonEnabled);
        }

        public void SetPreview(Fresvii.AppSteroid.Models.Video video, AUIPreviewVideoList auiPreviewVideoList)
        {
            this.auiPreviewVideoList = auiPreviewVideoList;

            this.Video = video;

            videoThumbnail.Set(this.Video.ThumbnailUrl);

            if (this.Video.App != null)
            {
                appIcon.Set(this.Video.App.IconUrl);

                textAppName.text = this.Video.App.Name;

                if (this.Video.App.GameGenres.Count > 0)
                    textGameCategory.text = this.Video.App.GameGenres[0].Name;

            }

            uint min = video.Duration / 60;

            uint sec = video.Duration % 60;

            uploadedText.text = video.CreatedAt.ToShortDateString() + " " + FASText.Get("Uploaded");

            durationText.text = min + ":" + sec.ToString("00");

        }

        public void PlayVideo()
        {
            if (this.Video != null)
            {
                Debug.Log("Play Video : " + this.Video.VideoUrl);

                FASVideo.Play(this.Video, (_video, button) => 
                {
                    if (button == Util.MoviePlayer.TappedButton.App)
                    {
                        auiPreviewVideoList.GoToAppDetail(this.Video.App);
                    }
                });
            }
        }

        public void GoToDetail()
        {
            auiPreviewVideoList.GoToAppDetail(this.Video.App);
        }


    }
}
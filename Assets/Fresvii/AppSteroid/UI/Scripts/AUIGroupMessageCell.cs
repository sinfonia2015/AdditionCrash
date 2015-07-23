using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public abstract class AUIGroupMessageCell : MonoBehaviour
    {
        [HideInInspector]
        public AppSteroid.Models.GroupMessage GroupMessage;

        [HideInInspector]
        public AUIMessages auiMessages;

        public abstract void SetGroupMessage(AppSteroid.Models.GroupMessage groupMessage);

        public RectTransform prfbDateTimeLine;

        protected bool hasTimeLine;

        protected GameObject dateTimeLine;

        public Image balloonBg;

        public Text comment;

        protected RectTransform rectTransform;

        public float margin;

        public AUIRawImageTextureSetter clipImage;

        //public float maxBalloonWidth = 500f;

        public Button buttonVideoPlay;

        public Vector2 stickerSize = new Vector2(300f, 300f);

        public Image balloonTip;

        public void SetTimeLine(bool hasTimeLine)
        {
            if (this.hasTimeLine != hasTimeLine)
            {
                if (this.hasTimeLine)
                {
                    Destroy(dateTimeLine);
                }
                else
                {
                    dateTimeLine = (Instantiate(prfbDateTimeLine) as RectTransform).gameObject;

                    dateTimeLine.transform.SetParent(this.transform, false);

                    dateTimeLine.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    dateTimeLine.GetComponent<AUIMessageDateTimeLine>().SetDateTime(GroupMessage.CreatedAt);
                }

                this.hasTimeLine = hasTimeLine;

                SetLayout();
            }
        }

        public Vector2 balloonReferenceSize = new Vector2(500f, 300f);

        public Vector2 clipImageReferenceSize = new Vector2(460f, 260f);

        protected void SetLayout()
        {
            RectTransform rtBalloonBg = balloonBg.gameObject.GetComponent<RectTransform>();

            clipImage.GetComponent<RectTransform>().sizeDelta = clipImageReferenceSize * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

            if (comment.gameObject.activeSelf)
            {
                float w = balloonReferenceSize.x * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

                rtBalloonBg.sizeDelta = new Vector2(w, rtBalloonBg.sizeDelta.y);

                comment.rectTransform.sizeDelta = new Vector2(-40f, comment.rectTransform.sizeDelta.y);

                rtBalloonBg.anchoredPosition = new Vector2(rtBalloonBg.anchoredPosition.x, ((hasTimeLine) ? - 20f - prfbDateTimeLine.sizeDelta.y : 0f));

                rtBalloonBg.sizeDelta = new Vector2(Mathf.Min(w, comment.preferredWidth + 40f), comment.preferredHeight + 40f);
            }
            else if (GroupMessage.Type == Models.GroupMessage.ContentType.Sticker)
            {
                clipImage.GetComponent<RectTransform>().sizeDelta = stickerSize;

                rtBalloonBg.anchoredPosition = new Vector2(rtBalloonBg.anchoredPosition.x, ((hasTimeLine) ? -20f - prfbDateTimeLine.sizeDelta.y : 0f));

                rtBalloonBg.sizeDelta = new Vector2(stickerSize.x + 40f, stickerSize.y + 40f);

                balloonBg.enabled = balloonTip.enabled = false;
            }
            else
            {
                rtBalloonBg.anchoredPosition = new Vector2(rtBalloonBg.anchoredPosition.x, ((hasTimeLine) ? - 20f - prfbDateTimeLine.sizeDelta.y : 0f));

                rtBalloonBg.sizeDelta = balloonReferenceSize * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);
            }

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, -rtBalloonBg.anchoredPosition.y + rtBalloonBg.sizeDelta.y + margin);

            buttonVideoPlay.gameObject.SetActive(GroupMessage.Type == Models.GroupMessage.ContentType.Video);
        }

        public void PlayVideo()
        {
            if (GroupMessage.Type == Models.GroupMessage.ContentType.Video && GroupMessage.VideoStatus == Models.GroupMessage.VideoStatuses.Ready)
            {
                if (GroupMessage.Video != null && !string.IsNullOrEmpty(GroupMessage.Video.VideoUrl))
                {
                    GroupMessage.Video.User = GroupMessage.User;

                    FASVideo.Play(GroupMessage.Video, (_video, button) => 
                    { 
                        GroupMessage.Video = _video;

						SetGroupMessage(GroupMessage);

                        if (button == Util.MoviePlayer.TappedButton.User)
                        {
                            auiMessages.GoToUserPage(GroupMessage.User);
                        }
                    });

					FASVideo.IncrementVideoPlaybackCount(GroupMessage.Video.Id, (_video, error)=>{

						if(error == null)
						{
							GroupMessage.Video = _video;
							
							SetGroupMessage(GroupMessage);
						}

					});
                }
            }
        }

        public void ShowImage()
        {
            if (GroupMessage.Type == Models.GroupMessage.ContentType.Image)
            {
                AUIImageViewer.Show(GroupMessage.ImageUrl, () => { });
            }

        }
    }
}

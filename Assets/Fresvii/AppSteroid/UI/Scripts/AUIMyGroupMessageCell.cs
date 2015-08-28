using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMyGroupMessageCell : AUIGroupMessageCell
    {
        public Text createdAt;

        private RectTransform clipImageRectTransform;

        private RectTransform userIconRectTransform;

        public Texture2D videoRemoved;

        private bool settle;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

            if(settle)
                StartCoroutine(ResetLayout());
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        public override void SetGroupMessage(AppSteroid.Models.GroupMessage groupMessage)
        {
            this.GroupMessage = groupMessage;

            if (GroupMessage.User != null)
            {
                if (GroupMessage.Type == Models.GroupMessage.ContentType.Text)
                {
                    if (!string.IsNullOrEmpty(GroupMessage.Text))
                    {
                        comment.text = GroupMessage.Text;
                    }
                    else
                    {
                        comment.text = "";
                    }

                    comment.gameObject.SetActive(true);

                    clipImage.gameObject.SetActive(false);
                }
                else if (GroupMessage.Type == Models.GroupMessage.ContentType.Image)
                {
                    clipImage.Set(GroupMessage.ImageThumbnailUrl);

                    comment.gameObject.SetActive(false);

                    clipImage.gameObject.SetActive(true);

                }
                else if (GroupMessage.Type == Models.GroupMessage.ContentType.Video)
                {
                    if (GroupMessage.VideoStatus == Models.GroupMessage.VideoStatuses.Removed)
                    {
                        clipImage.SetTexture(videoRemoved);
                    }
                    else
                    {
                        clipImage.Set(GroupMessage.Video.ThumbnailUrl);
                    }

                    comment.gameObject.SetActive(false);

                    clipImage.gameObject.SetActive(true);
                }
                else if (GroupMessage.Type == Models.GroupMessage.ContentType.Sticker)
                {
                    if (groupMessage.Sticker != null)
                    {
                        clipImage.resize = false;

                        clipImage.Set(GroupMessage.Sticker.Url);

                        comment.gameObject.SetActive(false);

                        clipImage.gameObject.SetActive(true);
                    }
                }
            }

            createdAt.text = GroupMessage.CreatedAt.ToString("HH:mm");

            SetLayout();

            settle = true;
        }

        void OnScreenSizeChanged()
        {
            StartCoroutine(ResetLayout());
        }

        IEnumerator ResetLayout()
        {
            yield return new WaitForEndOfFrame();

            SetGroupMessage(this.GroupMessage);
        }

        /*IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (GroupMessage != null)
                {
                    createdAt.text = AUIUtility.CurrentTimeSpan(GroupMessage.CreatedAt);

                    yield return new WaitForSeconds(60f);
                }
                else
                {
                    yield return 1;
                }
            }
        }*/
    }
}

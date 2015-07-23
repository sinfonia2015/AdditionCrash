using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIOtherGroupMessageCell : AUIGroupMessageCell
    {
        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        private RectTransform clipImageRectTransform;

        private RectTransform userIconRectTransform;

        public Texture2D videoRemoved;

        [HideInInspector]
        public AUIMessages auiMessages { get; set; }

        public AUITextSetter nameTextSetter;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

            //StartCoroutine(UpdateUpdatedAt());
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
                userIcon.Set(GroupMessage.User.ProfileImageUrl);

                userName.text = GroupMessage.User.Name + "・" + AUIUtility.CurrentTimeSpan(GroupMessage.CreatedAt);

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

            userName.text = GroupMessage.User.Name + "・" + GroupMessage.CreatedAt.ToString("HH:mm");

            if (userName.rectTransform.sizeDelta.x >= userName.preferredWidth)
            {
                userName.text = GroupMessage.User.Name;

                nameTextSetter.truncatedReplacement = "...・" + GroupMessage.CreatedAt.ToString("HH:mm");
            }

           
            SetLayout();
        }
        
        void OnScreenSizeChanged()
        {
            //SetLayout();
        }

        IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (GroupMessage != null)
                {
                    userName.text = GroupMessage.User.Name + "・" + AUIUtility.CurrentTimeSpan(GroupMessage.CreatedAt);

                    yield return new WaitForSeconds(60f);
                }
                else
                {
                    yield return 1;
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUICommunityTopImagessAndVideosCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Comment Comment { get; protected set; }

        public AUIRawImageTextureSetter image;

        AUICommunityTop communityTop;

        public GameObject buttonImage, buttonVideo;

        public void SetComment(Fresvii.AppSteroid.Models.Comment comment, AUICommunityTop communityTop)
        {
            this.communityTop = communityTop;

            if (this.Comment == null || this.Comment.Id != comment.Id)
            {
                this.Comment = comment;
            }
        }

        void OnEnable()
        {
            StartCoroutine(SetComment());
        }

        IEnumerator SetComment()
        {
            yield return 1;

            if (Comment.VideoState == Models.Comment.VideoStatus.Ready)
            {
                image.delayCount = 2;

                image.Set(Comment.Video.ThumbnailUrl);

                buttonVideo.gameObject.SetActive(true);
            }
            else if (!string.IsNullOrEmpty(Comment.ImageThumbnailUrl))
            {
                image.delayCount = 2;

                image.Set(Comment.ImageThumbnailUrl);

                buttonImage.gameObject.SetActive(true);
            }
        }

        public void OnClick()
        {
            communityTop.GoToThread(Comment.ThreadId, Comment, true);
        }
    }
}
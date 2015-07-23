using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUICommunityTopCommentCell : MonoBehaviour
    {
        Fresvii.AppSteroid.Models.Thread thread;

        public Fresvii.AppSteroid.Models.Comment Comment;

        public AUIRawImageTextureSetter userIcon;

        public Text text;

        public AUICommunityTop communityTop;

        public bool showAppIcon;

        private System.Action<Fresvii.AppSteroid.Models.Comment> OnClickComment;

        public void SetThread(Fresvii.AppSteroid.Models.Thread thread)
        {
            if (this.thread == null || this.thread.Id != thread.Id)
            {
                UpdateThread(thread);
            }
        }

        public void SetComment(Fresvii.AppSteroid.Models.Comment comment, System.Action<Fresvii.AppSteroid.Models.Comment> OnClickComment)
        {
            this.OnClickComment = OnClickComment;

            if (this.Comment == null || this.Comment.Id != comment.Id)
            {
                UpdateComment(comment);
            }
        }

        // Update is called once per frame
        void UpdateThread(Fresvii.AppSteroid.Models.Thread thread)
        {
            this.thread = thread;

            userIcon.Set(thread.User.ProfileImageUrl);

            text.text = thread.Comment.Text;
        }

        // Update is called once per frame

        public Material appIconMaterial;

        void UpdateComment(Fresvii.AppSteroid.Models.Comment comment)
        {
            this.Comment = comment;

            if (showAppIcon)
            {
                userIcon.Set(comment.App.IconUrl);
            }
            else
            {
                userIcon.Set(comment.User.ProfileImageUrl);
            }

            text.text = comment.Text;
        }

        public void OnClick()
        {
            if (thread != null)
            {
                communityTop.GoToThread(thread.Id, thread.Comment, true);
            }
            else if (Comment != null && OnClickComment != null)
            {
                OnClickComment(Comment);
            }
        }
    }
}
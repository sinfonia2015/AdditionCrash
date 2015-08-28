using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUICommunityTopCommentCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Thread Thread;

        public AUIRawImageTextureSetter userIcon;

        public Text text;

        public AUICommunityTop communityTop;

        public bool showAppIcon;

        private bool isApp;

        private System.Action<Fresvii.AppSteroid.Models.Thread> OnClickCell;

        public void SetThread(Fresvii.AppSteroid.Models.Thread thread, bool isApp, System.Action<Fresvii.AppSteroid.Models.Thread> OnClickCell)
        {
            this.OnClickCell = OnClickCell;

            this.isApp = isApp;

            UpdateThread(thread);
        }

        // Update is called once per frame
        void UpdateThread(Fresvii.AppSteroid.Models.Thread thread)
        {
            this.Thread = thread;

            if (isApp)
            {
                userIcon.Set(thread.App.IconUrl);
            }
            else
            {
                userIcon.Set(thread.User.ProfileImageUrl);
            }

            if (string.IsNullOrEmpty(thread.Title))
            {
                text.text = thread.Comment.Text;
            }
            else
            {
                text.text = thread.Title;
            }
        }

        public void OnClick()
        {
            if (OnClickCell != null)
            {
                OnClickCell(Thread);
            }
        }
    }
}
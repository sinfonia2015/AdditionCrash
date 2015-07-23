using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFriendListCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Friend Friend { get; set; }

        private Fresvii.AppSteroid.Models.User user;

        public AUIFriendList FriendList { get; set; }

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public Image tagLabel;

        public Text tagText;

        private AUIFriendList parentPage;

        bool tagSized = false;

        public void SetFriend(Fresvii.AppSteroid.Models.Friend friend, AUIFriendList parentPage)
        {
            this.Friend = friend;

            this.parentPage = parentPage;

            this.user = this.Friend.ToUser();

            if (this.user.Id == FAS.CurrentUser.Id)
            {
                tagLabel.gameObject.SetActive(true);

                tagText.text = FASText.Get("You");

                if (!tagSized)
                {
                    userName.rectTransform.sizeDelta = new Vector2(userName.rectTransform.sizeDelta.x - tagLabel.rectTransform.sizeDelta.x - 30f, userName.rectTransform.sizeDelta.y);

                    tagSized = true;
                }
            }

            userIcon.Set(user.ProfileImageUrl);

            userName.text = user.Name;
        }

        public void GoToUserPage()
        {
            if (user == null) return;

            parentPage.GoToUserPage(this.user);
        }       
    }
}
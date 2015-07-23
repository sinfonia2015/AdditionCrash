using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIUserSearchCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.User User;

        private AUIUserSearch parentPage;

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public Image tagLabel;

        public Text tagText;

        public Color tagYouColor, tagFriendColor;

        bool tagSized = false;

        public void SetUser(Fresvii.AppSteroid.Models.User user, AUIUserSearch parentPage)
        {
            this.User = user;

            this.parentPage = parentPage;

            userName.text = user.Name;

            userIcon.Set(user.ProfileImageUrl);

            if (this.User.Id == FAS.CurrentUser.Id)
            {
                tagLabel.gameObject.SetActive(true);

                tagLabel.GetComponent<Image>().color = tagYouColor;

                tagText.text = FASText.Get("You");

                tagText.color = tagYouColor;
            }
            else if (this.User.FriendStatus == Models.User.FriendStatuses.Friend)
            {
                tagLabel.gameObject.SetActive(true);

                tagText.text = FASText.Get("Friend");

                tagLabel.GetComponent<Image>().color = tagFriendColor;

                tagText.color = tagFriendColor;
            }

            if (tagLabel.gameObject.activeSelf)
            {
                if (!tagSized)
                {
                    userName.rectTransform.sizeDelta = new Vector2(userName.rectTransform.sizeDelta.x - tagLabel.rectTransform.sizeDelta.x - 30f, userName.rectTransform.sizeDelta.y);

                    tagSized = true;
                }
            }
        }

        public void GoToUserPage()
        {
            parentPage.GoToUserPage(User);
        }
    }
}
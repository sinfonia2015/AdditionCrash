using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMatchmakingSelectFriendsCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Friend Friend { get; set; }

        private Fresvii.AppSteroid.Models.User user;

        private AUIMatchmakingSelectFriends parentPage;

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public AUIToggleButton buttonCheck;

        private bool isSelected;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                isSelected = value;

                buttonCheck.Set(isSelected);
            }
        }

        public event Action<bool, Fresvii.AppSteroid.Models.User> OnCheckStateChanged;

        public void SetFriend(Fresvii.AppSteroid.Models.Friend friend, AUIMatchmakingSelectFriends parentPage, bool isSelected)
        {
            this.Friend = friend;

            this.parentPage = parentPage;

            this.IsSelected = isSelected;

            userName.text = friend.Name;

            GetUser();

            buttonCheck.Set(this.IsSelected);
        }

        void GetUser()
        {
            FASUser.GetUser(this.Friend.Id, (user, error) =>
            {
                if (error == null)
                {
                    this.user = user;

                    userIcon.Set(user.ProfileImageUrl);

                    userName.text = user.Name;
                }
                else
                {
                    Invoke("GetUser", 3f);
                }
            });
        }

        public void GoToUserPage()
        {
            if (user == null) return;

            parentPage.GoToUserPage(user);
        }

        public void OnCheckButtonClicked()
        {
            if (this.user == null) return;

            IsSelected = !IsSelected;

            if (OnCheckStateChanged != null)
            {
                OnCheckStateChanged(IsSelected, this.user);
            }
        }
    }
}
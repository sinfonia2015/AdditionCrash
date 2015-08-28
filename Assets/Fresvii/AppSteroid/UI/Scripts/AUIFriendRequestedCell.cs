using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFriendRequestedCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Friend Friend { get; protected set; }

        public Fresvii.AppSteroid.Models.User user { get; protected set; }

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        private AUIFriendRequestScrollView parentNode;

        public AUICellDeleteAnimator cellDeleteAnimator;

        public GameObject line, buttonNotNow;

        public void SetFriend(Fresvii.AppSteroid.Models.Friend friend, AUIFriendRequestScrollView parentNode, AUIFriendRequestScrollView.Mode mode)
        {
            this.Friend = friend;

            this.parentNode = parentNode;

            line.SetActive(mode == AUIFriendRequestScrollView.Mode.Requested);

            buttonNotNow.SetActive(mode == AUIFriendRequestScrollView.Mode.Requested);

            GetUser();
        }

        void GetUser()
        {
            FASUser.GetUser(Friend.Id, (_user, _error) =>
            {
                if (_error == null)
                {
                    this.user = _user;

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

            parentNode.auiFriendRequest.GoToUserPage(this.user);
        }

        public void OnClickConfirm()
        {
            parentNode.AcceptFriendshipRequest(this);
        }

        public void OnClickNotNow()
        {
            parentNode.HideFriendshipRequest(this);
        }
    }
}
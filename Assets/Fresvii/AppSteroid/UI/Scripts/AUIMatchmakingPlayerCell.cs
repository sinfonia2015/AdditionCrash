using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMatchmakingPlayerCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Player Player { get; set; }

        private AUIMatchMaking parentPage;

        public AUIRawImageTextureSetter userIcon;

        public Text userNameSetting;

        public Text userNameMatching, matchingStatus;

        public Image tagLabel;

        public Text tagText;

        public Button buttonInfo;

        public Texture2D defaultUserIcon;

        public Color tagMeColor, tagFriendColor;

        public Color joinedColor, invitingColor, cancelColor;

        public GameObject goSetting, goMatching;

        public Vector2 normalSize, taggedSize;

        public Button buttonCell;

        public GameObject[] tagLabelSpaces;

        public GameObject[] infoButtonSpaces;

        public void SetPlayer(Fresvii.AppSteroid.Models.Player player, AUIMatchMaking parentPage)
        {
            this.Player = player;

            this.parentPage = parentPage;

            userIcon.Set(Player.User.ProfileImageUrl);

            UpdateCell();
        }

        public void ClearPlayer()
        {
            this.Player = new AppSteroid.Models.Player();

            userIcon.ReleaseTexture();

            userIcon.SetTexture(defaultUserIcon);

            UpdateCell();
        }

        private Fresvii.AppSteroid.Models.User postUser;

        public void UpdateCell()
        {
            tagLabel.gameObject.SetActive(false);

            userNameMatching.rectTransform.sizeDelta = userNameSetting.rectTransform.sizeDelta = normalSize;

            if (parentPage.State != AUIMatchMaking.Status.Matching)
            {
                goMatching.SetActive(false);

                goSetting.SetActive(true);
            }
            else
            {
                goMatching.SetActive(true);

                goSetting.SetActive(false);
            }

            buttonInfo.gameObject.SetActive(Player != null && !string.IsNullOrEmpty(Player.User.Id));

            if (Player != null && !string.IsNullOrEmpty(Player.User.Id))
            {
                if (postUser == null || postUser.Id != Player.User.Id)
                {
                    userNameMatching.text = userNameSetting.text = Player.User.Name;
                }

                postUser = Player.User;

                if (Player.User.Id == FAS.CurrentUser.Id)
                {
                    tagLabel.gameObject.SetActive(true);

                    foreach (GameObject go in tagLabelSpaces)
                        go.SetActive(true);

                    tagText.text = FASText.Get("You");

                    tagLabel.color = tagText.color = tagMeColor;

                    //userNameMatching.rectTransform.sizeDelta = userNameSetting.rectTransform.sizeDelta = taggedSize;
                }
                else if (Player.User.FriendStatus == Models.User.FriendStatuses.Friend)
                {
                    tagLabel.gameObject.SetActive(true);

                    foreach (GameObject go in tagLabelSpaces)
                        go.SetActive(true);

                    tagText.text = FASText.Get("Friend");

                    tagLabel.color = tagText.color = tagFriendColor;

                    //userNameMatching.rectTransform.sizeDelta = userNameSetting.rectTransform.sizeDelta = taggedSize;
                }
                else
                {
                    tagLabel.gameObject.SetActive(false);

                    foreach (GameObject go in tagLabelSpaces)
                        go.SetActive(false);
                }

                //buttonInfo.gameObject.SetActive(true);

                if (parentPage.State == AUIMatchMaking.Status.Matching)
                {
                    //buttonInfo.gameObject.SetActive(false);

                    if (Player.State == Fresvii.AppSteroid.Models.Player.Status.Invited)
                    {
                        matchingStatus.text = FASText.Get("PlayerStatusInvited");

                        matchingStatus.color = invitingColor;
                    }
                    else if (Player.State == Fresvii.AppSteroid.Models.Player.Status.Accepted)
                    {
                        matchingStatus.text = FASText.Get("PlayerStatusAccepted");

                        matchingStatus.color = joinedColor;
                    }
                    else if (Player.State == Fresvii.AppSteroid.Models.Player.Status.Cancelled)
                    {
                        matchingStatus.text = FASText.Get("PlayerStatusCancelled");

                        matchingStatus.color = cancelColor;
                    }
                    else if (Player.State == Fresvii.AppSteroid.Models.Player.Status.Matching)
                    {
                        matchingStatus.text = FASText.Get("PlayerStatusMatching");
                    
                        matchingStatus.color = joinedColor;
                    }
                    else if (Player.State == Fresvii.AppSteroid.Models.Player.Status.Declined)
                    {
                        matchingStatus.text = FASText.Get("PlayerStatusDeclined");

                        matchingStatus.color = cancelColor;
                    }
                }

                foreach (GameObject go in infoButtonSpaces)
                    go.SetActive(true);

            }
            else if (parentPage.State != AUIMatchMaking.Status.Matching)
            {
                foreach (GameObject go in infoButtonSpaces)
                        go.SetActive(false);

                tagLabel.gameObject.SetActive(false);

                foreach (GameObject go in tagLabelSpaces)
                    go.SetActive(false);

                userNameSetting.text = FASText.Get("InviteFriend");
            }
            else if (parentPage.State == AUIMatchMaking.Status.Matching && string.IsNullOrEmpty(Player.User.Id))
            {
                foreach (GameObject go in infoButtonSpaces)
                    go.SetActive(false);

                tagLabel.gameObject.SetActive(false);

                foreach (GameObject go in tagLabelSpaces)
                    go.SetActive(false);

                goMatching.SetActive(false);

                goSetting.SetActive(true);

                userNameSetting.text = FASText.Get("AutoMatch");
            }

            buttonCell.interactable = (parentPage.State == AUIMatchMaking.Status.Setting);
        }

        public void Clear()
        {         
            userIcon.ReleaseTexture();

            userIcon.SetTexture(defaultUserIcon);

            buttonInfo.gameObject.SetActive(false);

            this.Player = new Fresvii.AppSteroid.Models.Player();

            UpdateCell();
        }

        public void GoToUserPage()
        {
            if (Player == null || Player.User == null) return;

            parentPage.GoToUserPage(Player.User);
        }

        public void GoToSelectFriends()
        {
            parentPage.GoToSelectFriends();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIPlayerScoreCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Score Score { get; protected set; }

        private int rank;

        public int Rank
        {
            get
            {
                return rank;
            }
         
            set
            {
                rank = value;

                if (rank <= 3)
                {
                    topRankedObj.SetActive(true);

                    nomalRankedObj.SetActive(false);

                    topRankedText.text = rank.ToString();

                    if(rank > 0)
                        topRankedObj.GetComponent<Image>().color = rankBadgeColors[rank - 1];
                }
                else
                {
                    topRankedObj.SetActive(false);

                    nomalRankedObj.SetActive(true);

                    nomalRankedText.text = rank.ToString();
                }
            }
        }

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public Text scoreText;

        private bool isMe;

        public Image tagLabel;

        public Text tagText;

        private AUILeaderboard parentPage;

        public Color[] rankBadgeColors;

        public GameObject topRankedObj, nomalRankedObj;

        public Text topRankedText, nomalRankedText;

        public Color tagYouColor, tagFriendColor;

        public bool myScoreCell;

        bool tagSized = false;

        private Action<Fresvii.AppSteroid.Models.User> goToUserPage;

        public void SetScore(Fresvii.AppSteroid.Models.Score score, Action<Fresvii.AppSteroid.Models.User> goToUserPageCallback)
        {
            this.Score = score;

            this.goToUserPage = goToUserPageCallback;

            isMe = (this.Score.User.Id == FAS.CurrentUser.Id);

            if (!myScoreCell)
            {
                if (isMe)
                {
                    tagLabel.gameObject.SetActive(true);

                    tagLabel.GetComponent<Image>().color = tagYouColor;

                    tagText.text = FASText.Get("You");

                    tagText.color = tagYouColor;
                }
                else if (this.Score.User.FriendStatus == Models.User.FriendStatuses.Friend)
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

            userIcon.Set(this.Score.User.ProfileImageUrl);

            userName.text = this.Score.User.Name;

            this.scoreText.text = this.Score.FormattedValue;
        }
     
        public void GoToUserPage()
        {
            if (this.Score == null || this.Score.User == null) return;

            if (goToUserPage != null)
            {
                goToUserPage(this.Score.User);
            }
        }
    }
}
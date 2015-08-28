using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUILeaderboardListCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Leaderboard Leaderboard { get; set; }

        public AUIRawImageTextureSetter icon;

        public Text leaderboardName;

        public Text despcription;

        private AUILeaderboardList parentPage;

        public void SetLeaderboard(Fresvii.AppSteroid.Models.Leaderboard leaderboard, AUILeaderboardList parentPage)
        {
            this.Leaderboard = leaderboard;

            this.parentPage = parentPage;

            leaderboardName.text = leaderboard.Name;

            despcription.text = leaderboard.Description;

            icon.Set(leaderboard.IconUrl);
        }

        public void GoToLeaderboard()
        {
            if (Leaderboard == null) return;

            parentPage.GoToLeaderboard(this.Leaderboard);
        }
    }
}
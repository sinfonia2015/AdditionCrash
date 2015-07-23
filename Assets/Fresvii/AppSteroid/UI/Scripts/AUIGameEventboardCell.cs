using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGameEventboardCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Eventboard Eventboard { get; protected set; }

        public AUIRawImageTextureSetter image;

        public Text textTitle, textRankingScore;

        AUIGameEvent auiGameEvent;

        public void SetEventboard(Fresvii.AppSteroid.Models.Eventboard eventboard, AUIGameEvent auiGameEvent)
        {
            this.auiGameEvent = auiGameEvent;

            this.Eventboard = eventboard;

            this.textTitle.text = Eventboard.Leaderboard.Name;

            this.textRankingScore.text = Eventboard.Leaderboard.Description;

            if (string.IsNullOrEmpty(Eventboard.Leaderboard.IconUrl))
            {
                FASLeaderboard.GetLeaderboard(Eventboard.Leaderboard.IconUrl, (leaderboard, error) =>
                {
                    if (error == null)
                    {
                        image.Set(leaderboard.IconUrl);
                    }
                });
            }
            else
            {
                image.Set(Eventboard.Leaderboard.IconUrl);
            }
        }

        public void OnClick()
        {
            auiGameEvent.GoToEventboard(this.Eventboard);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUICommunityTopGameEventCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.GameEvent GameEvent { get; protected set; }

        public AUIRawImageTextureSetter image;

        public Text eventNameText;

        AUICommunityTop auiCommunityTop;

        public void SetGameEvent(Fresvii.AppSteroid.Models.GameEvent gameEvent, AUICommunityTop auiCommunityTop)
        {
            this.auiCommunityTop = auiCommunityTop;

            this.GameEvent = gameEvent;

            image.Set(GameEvent.ImageUrl);

            eventNameText.text = this.GameEvent.Name;
        }

        public void OnClick()
        {
            if (!string.IsNullOrEmpty(GameEvent.WebSiteUrl))
            {
                Application.OpenURL(GameEvent.WebSiteUrl);
            }
            else
            {
                auiCommunityTop.GoToGameEvent(this.GameEvent);
            }
        }
    }
}
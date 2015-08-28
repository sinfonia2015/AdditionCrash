using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIHotGameEventCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.GameEvent GameEvent { get; protected set; }

        public AUIRawImageTextureSetter image;

        public Text eventNameText;

        public AUIRawImageTextureSetter appIcon;

        public Text appName;

        System.Action<Fresvii.AppSteroid.Models.GameEvent> OnClickCallback;

        public void SetGameEvent(Fresvii.AppSteroid.Models.GameEvent gameEvent, System.Action<Fresvii.AppSteroid.Models.GameEvent> OnClick)
        {
            this.OnClickCallback = OnClick;

            this.GameEvent = gameEvent;

            image.Set(GameEvent.ImageUrl);

            eventNameText.text = this.GameEvent.Name;

            if (GameEvent.App != null)
            {
                appIcon.Set(GameEvent.App.IconUrl);

                appName.text = GameEvent.App.Name;
            }
        }

        public void OnClick()
        {
            if (!string.IsNullOrEmpty(GameEvent.WebSiteUrl))
            {
                FASUtility.SendPageView("pv.community.events.show", GameEvent.Id, System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                        Debug.LogError(e.ToString());

                    Application.OpenURL(GameEvent.WebSiteUrl);
                });
            }
            else
            {
                if (this.OnClickCallback != null)
                    OnClickCallback(this.GameEvent);
            }
        }
    }
}
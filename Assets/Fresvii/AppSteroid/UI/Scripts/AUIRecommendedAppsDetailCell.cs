using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIRecommendedAppsDetailCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.App AddApp;

        public AUIRawImageTextureSetter banner;

        public AUIRawImageTextureSetter appIcon;

        public Text textAppName, textDeveloperName;

        public event System.Action<Fresvii.AppSteroid.Models.App> OnClickAppCell;

        AUIRecommendedApps AUIRecommendedApps;

        public Button button;

        public float twoLineHeight = 10000f;

        public bool IsTwoLine()
        {
            return (textAppName.preferredHeight > twoLineHeight);
        }

        public void SetApp(Fresvii.AppSteroid.Models.App addApp, AUIRecommendedApps AUIRecommendedApps)
        {
            this.AUIRecommendedApps = AUIRecommendedApps;

            if (addApp == null)
            {
                if (banner != null)
                    banner.gameObject.SetActive(false);

                appIcon.gameObject.SetActive(false);

                textAppName.gameObject.SetActive(false);

                textDeveloperName.gameObject.SetActive(false);

                button.interactable = false;
            }
            else
            {
                this.AddApp = addApp;

                if (banner != null)
                    banner.Set(this.AddApp.IconUrl);

                appIcon.Set(this.AddApp.IconUrl);

                textAppName.text = this.AddApp.Name;

                textDeveloperName.text = this.AddApp.GameGenres[0].Name;
            }
        }

        public void OnClick()
        {
            AUIRecommendedApps.GoToAppDetail(this.AddApp);
        }
    }
}

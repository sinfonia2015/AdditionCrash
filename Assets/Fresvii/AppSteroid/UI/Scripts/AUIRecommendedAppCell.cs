using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIRecommendedAppCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.App AddApp;

        public AUIRawImageTextureSetter appIcon;

        public Text textAppName, textGameCategory;

        public event System.Action<Fresvii.AppSteroid.Models.App> OnClickAppCell;

        public void SetApp(Fresvii.AppSteroid.Models.App addApp)
        {
            this.AddApp = addApp;

            appIcon.Set(this.AddApp.IconUrl);

            textAppName.text = this.AddApp.Name;

            if (this.AddApp.GameGenres.Count > 0)
                textGameCategory.text = this.AddApp.GameGenres[0].Name;

        }

        public void OnClick()
        {
            if(OnClickAppCell != null)
                OnClickAppCell(this.AddApp);
        }

    }
}
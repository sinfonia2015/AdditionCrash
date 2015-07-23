using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUICommunityTopBanner : MonoBehaviour
    {
        public AUIRawImageTextureSetter bannerImage;

        public string imageUrl;

        public string iOSUrl, AndroidUrl;

        void Start()
        {
            bannerImage.Set(imageUrl);
        }

        public void OnClickBanner()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Application.OpenURL(iOSUrl);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                Application.OpenURL(AndroidUrl);
            }
        }
    }
}

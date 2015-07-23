using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIBannerCell : MonoBehaviour {

        public Fresvii.AppSteroid.Models.App App;

        public AUIRawImageTextureSetter bannerImage;

        public Graphic image;

        public bool Showing = false;

        public bool TextureIsReady { get; protected set; }

        public float fadeDuraion = 0.5f;

		public void Awake()
		{
			image.CrossFadeAlpha (0f, 0f, true);
		}

        public void SetApp(Fresvii.AppSteroid.Models.App addApp)
        {
            this.App = addApp;

            bannerImage.delayCount = transform.GetSiblingIndex() * 10;

            bannerImage.Set(this.App.BannerImageUrl, (obj) =>
            {
                if(!Showing)
                    gameObject.SetActive(false);

                TextureIsReady = true;
            });
        }

        public void Show()
        {
            Showing = true;

            this.gameObject.SetActive(true);

            StartCoroutine(ShowCoroutine());
        }

        IEnumerator ShowCoroutine()
        {
            image.CrossFadeAlpha(0f, 0f, true);

            yield return 1;

            image.CrossFadeAlpha(1f, fadeDuraion, true);
        }

        public void Hide(System.Action callback)
        {
            StartCoroutine(HideCoroutine(callback));
        }

        IEnumerator HideCoroutine(System.Action callback)
        {
            image.CrossFadeAlpha(0f, fadeDuraion, true);

            yield return new WaitForSeconds(fadeDuraion);

            this.gameObject.SetActive(false);

            Showing = false;

            callback();
        }

        public void OnClick()
        {
            if (!string.IsNullOrEmpty(App.StoreUrl))
            {
                FASUtility.SendPageView("event.ad.click.store", this.App.Id, System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                        Debug.LogError(e.ToString());
                });
 
                Application.OpenURL(App.StoreUrl); 
            }
        }
    }
}




  
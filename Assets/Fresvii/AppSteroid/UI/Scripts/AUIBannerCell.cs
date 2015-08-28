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

        private AUIFrame parentFrame;

		public void Awake()
		{
			image.CrossFadeAlpha (0f, 0f, true);
		}

        void OnEnable()
        {
            if (!TextureIsReady && this.App != null && parentFrame != null)
            {
                SetApp(this.App, parentFrame);
            }
        }

        void OnDisable()
        {
            if(TextureIsReady)
                this.gameObject.SetActive(false);
        }

        public void SetApp(Fresvii.AppSteroid.Models.App addApp, AUIFrame parentFrame)
        {
            this.parentFrame = parentFrame;

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
            GoToAppDetail(this.App);
        }

        public GameObject prfbAppDetail;

        public void GoToAppDetail(Fresvii.AppSteroid.Models.App app)
        {
            FASUtility.SendPageView("event.ad.click.store", this.App.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());

                Application.OpenURL(this.App.StoreUrl);
            });

            /*if (parentFrame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiAppDetail = ((GameObject)Instantiate(prfbAppDetail)).GetComponent<AUIAppDetail>();

            auiAppDetail.SetApp(app);

            auiAppDetail.transform.SetParent(parentFrame.transform.parent, false);

            auiAppDetail.transform.SetAsLastSibling();

            auiAppDetail.frame.backFrame = this.parentFrame;

            auiAppDetail.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.parentFrame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                parentFrame.gameObject.SetActive(false);
            });*/
        }

    }
}




  
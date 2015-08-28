using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(AUIPinchDetector))]

    public class AUIImageViewer : MonoBehaviour
    {
        public AUIFrame frame;

        private static AUIImageViewer instance;

        public AUIRawImageTextureSetter image;
        
        public float zoomSpeed;

        private Vector2 textureSize;

        public RectTransform area;

        public RectTransform imageRect;

        private Vector2 initSize;

        public Image bg;
        
        public RawImage photo;

        public Button closeButton;

        AUIPinchDetector pinchDetector;

        public static void Show(string imageUrl, Action callback)
        {
            instance = ((GameObject)Instantiate((Resources.Load("AUIImageViewer") as GameObject))).GetComponent<AUIImageViewer>();

            instance.transform.SetParent(AUIManager.Instance.FramesNode, false);

            instance.transform.SetAsLastSibling();

            instance.image.Set(imageUrl);
        }

        void OnEnable()
        {
            pinchDetector = GetComponent<AUIPinchDetector>();

            pinchDetector.OnPinch += OnPinch;

            pinchDetector.OnPinchStart += OnPinchStart;

            pinchDetector.OnPinchEnd += OnPinchEnd;
        }

        void OnDisable()
        {
            AUIManager.Instance.HideLoadingSpinner();

            pinchDetector.OnPinch -= OnPinch;

            pinchDetector.OnPinchEnd -= OnPinchEnd;
        }

        bool initialized = false;

		public GameObject loadingSpinner;

        IEnumerator Start()
        {
            bg.CrossFadeAlpha(0f, 0f, true);

            photo.CrossFadeAlpha(0f, 0f, true);

            this.frame.Animate(new Vector2(0f, -AUIManager.Instance.sizedCanvas.rect.height), Vector2.zero, () => { });

            yield return 1;

            bg.CrossFadeAlpha(1f, 1f, true);

			loadingSpinner.SetActive(true);

            while (image.GetTexture() == null)
            {
                yield return 1;
            }

            photo.CrossFadeAlpha(1f, 1f, true);

            initialized = true;

			loadingSpinner.SetActive(false);

            textureSize = new Vector2(image.GetTexture().width, image.GetTexture().height);

            initSize = imageRect.sizeDelta = area.rect.width / textureSize.x * textureSize;

            if (imageRect.sizeDelta.y > area.rect.height)
            {
                initSize = imageRect.sizeDelta = area.rect.height / textureSize.y * textureSize;
            }
        }

        public void Close()
        {
            bg.CrossFadeAlpha(0f, 1f, true);

            photo.CrossFadeAlpha(0f, 1f, true);

            this.frame.Animate(Vector2.zero, new Vector2(0f, -AUIManager.Instance.sizedCanvas.rect.height), () => 
            {
                Destroy(this.gameObject);
            });
        }

        Vector2 startSize;

        void OnPinchStart()
        {
            closeButton.interactable = false;

            startSize = imageRect.sizeDelta;
        }



        void OnPinch(float scale)
        {
            if (!initialized)
            {
                return;
            }

            Vector2 size = imageRect.sizeDelta;

            imageRect.sizeDelta = startSize * scale;

            if (imageRect.sizeDelta.x < initSize.x || imageRect.sizeDelta.y < initSize.y)
            {
                imageRect.sizeDelta = size;
            }
            else if (imageRect.sizeDelta.x / initSize.x > 5f || imageRect.sizeDelta.y / initSize.y > 5f)
            {
                imageRect.sizeDelta = size;
            }
            else if (imageRect.sizeDelta.x > 1.2f * size.x || imageRect.sizeDelta.x < 0.8f * size.x || imageRect.sizeDelta.y > 1.2f * size.y || imageRect.sizeDelta.y < 0.8f * size.y)
            {
                imageRect.sizeDelta = size;
            }


        }

        void OnPinchEnd()
        {
            if (closeButton.interactable == false)
            {
                StartCoroutine(DelayedButtonActivate());
            }
        }

        IEnumerator DelayedButtonActivate()
        {
            yield return new WaitForSeconds(0.5f);

            if(!pinchDetector.Piching)
                closeButton.interactable = true;
        }

    }
}
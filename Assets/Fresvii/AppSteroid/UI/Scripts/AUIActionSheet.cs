using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIActionSheet : MonoBehaviour
    {
        private static AUIActionSheet instance;

        public GameObject prfbTopButton, prfbCenterButton, prfbBottomButton, prfbSingleButton;

        private Action<string> callback;

        public RectTransform buttonsRectTransform;

        public Image bg;

        public static void Show(string[] buttons, Action<string> callback)
        {
            if (instance != null)
            {
                callback("");

                return;
            } 

            AUIManager.Instance.canBackButton = false;

            instance = ((GameObject)Instantiate((Resources.Load("AUIActionSheet") as GameObject))).GetComponent<AUIActionSheet>();

            instance.transform.SetParent(AUIManager.Instance.sizedCanvas, false);

            instance.transform.SetAsLastSibling();

            instance.GetComponent<RectTransform>().sizeDelta = AUIManager.Instance.sizedCanvas.sizeDelta;

            instance.callback = callback;

            // Buttons
            if (buttons.Length == 1)
            {
                RectTransform buttonRectTransform = (Instantiate(instance.prfbSingleButton) as GameObject).GetComponent<RectTransform>();

                buttonRectTransform.SetParent(instance.buttonsRectTransform, false);

                buttonRectTransform.SetAsFirstSibling();

                Button button = buttonRectTransform.GetComponent<Button>();

                button.transform.GetChild(0).GetComponent<Text>().text = buttons[0];

                button.onClick.AddListener(() =>
                {
                    instance.StartCoroutine(instance.Hide(buttons[0]));
                });
            }
            else
            {
                for (int i = buttons.Length - 1; i >= 0; i--)
                {
                    RectTransform buttonRectTransform;

                    if (i == buttons.Length - 1)
                    {
                        buttonRectTransform = (Instantiate(instance.prfbBottomButton) as GameObject).GetComponent<RectTransform>();
                    }
                    else if (i == 0)
                    {
                        buttonRectTransform = (Instantiate(instance.prfbTopButton) as GameObject).GetComponent<RectTransform>();
                    }
                    else
                    {
                        buttonRectTransform = (Instantiate(instance.prfbCenterButton) as GameObject).GetComponent<RectTransform>();
                    }

                    buttonRectTransform.SetParent(instance.buttonsRectTransform, false);

                    buttonRectTransform.SetAsFirstSibling();

                    Button button = buttonRectTransform.GetComponent<Button>();

                    button.transform.GetChild(0).GetComponent<Text>().text = buttons[i];

                    string strButton = buttons[i];

                    button.onClick.AddListener(() => {

                        instance.StartCoroutine(instance.Hide(strButton));
                    });
                }
            }
        }

        void OnUpdatePosition(float y)
        {
            buttonsRectTransform.anchoredPosition = new Vector2(buttonsRectTransform.anchoredPosition.x, y);
        }

        public float fadeIn = 0.5f, fadeOut = 0.0f, duration = 0.5f;

        public iTween.EaseType easetype;

        IEnumerator Start()
        {
            buttonsRectTransform.anchoredPosition = new Vector2(buttonsRectTransform.anchoredPosition.x, AUIManager.Instance.sizedCanvas.rect.y);

            iTween.ValueTo(this.gameObject, iTween.Hash("time", duration, "easetype", easetype, "from", AUIManager.Instance.sizedCanvas.rect.y, "to", 0f, "onupdate", "OnUpdatePosition"));

            bg.CrossFadeAlpha(0.0f, 0.0f, true);

            yield return 1;

            bg.CrossFadeAlpha(fadeIn, duration, true);
        }

        public void FadeOut()
        {
            bg.CrossFadeAlpha(fadeOut, duration, true);
        }

        public void OnCancel()
        {
            StartCoroutine(Hide(null));
        }

        string callbackButton;

        IEnumerator Hide(string button)
        {
            instance = null;

            this.callbackButton = button;

            yield return new WaitForSeconds(duration);

            FadeOut();

            iTween.ValueTo(this.gameObject, iTween.Hash("time", duration, "easetype", easetype, "to", AUIManager.Instance.sizedCanvas.rect.y, "from", 0f, "onupdate", "OnUpdatePosition", "oncomplete", "OnHideComplete"));
        }

        void OnHideComplete()
        {
            this.callback(callbackButton);

            AUIManager.Instance.canBackButton = true;

            Destroy(this.gameObject);
        }
    }
}
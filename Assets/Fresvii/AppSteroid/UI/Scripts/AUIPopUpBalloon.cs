using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIPopUpBalloon : MonoBehaviour
    {
        private static AUIPopUpBalloon instance;

        public GameObject prfbTopButton, prfbCenterButton, prfbBottomButton;

        private Action<string> callback;

        public RectTransform buttonsRectTransform;

        public RectTransform bg;

        public float margin = 120f;

        public float minWidth = 310f;

        public Vector2 offset;

        private List<Button> buttons;

        public static void Show(string[] buttons, RectTransform parent, Action<string> callback)
        {
            instance = ((GameObject)Instantiate((Resources.Load("AUIPopUpBalloon") as GameObject))).GetComponent<AUIPopUpBalloon>();

            instance.transform.SetParent(parent, false);

            instance.buttonsRectTransform.anchoredPosition = instance.offset;

            instance.transform.SetParent(AUIManager.Instance.sizedCanvas);

            instance.transform.SetAsLastSibling();

            instance.callback = callback;

            instance.buttons = new List<Button>();

            float height = 0f;

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

                height += buttonRectTransform.GetComponent<LayoutElement>().preferredHeight;

                buttonRectTransform.SetParent(instance.buttonsRectTransform, false);

                buttonRectTransform.SetAsFirstSibling();

                Button button = buttonRectTransform.GetComponent<Button>();

                button.transform.FindChild("Text").GetComponent<Text>().text = buttons[i];

                string strButton = buttons[i];

                button.onClick.AddListener(() =>
                {
                    instance.StartCoroutine(instance.Hide(strButton));
                });

                instance.buttons.Add(button);
            }

            float maxWidth = instance.minWidth;

            foreach(Button button in instance.buttons)
            {
                maxWidth = Mathf.Max(maxWidth, button.transform.FindChild("Text").GetComponent<Text>().preferredWidth + instance.margin);
            }

            instance.buttonsRectTransform.sizeDelta = new Vector2(maxWidth, height);
        }

        public float fadeIn = 0.5f, fadeOut = 0.0f, duration = 0.5f;

        public iTween.EaseType easetype;

        string callbackButton;

        IEnumerator Hide(string button)
        {
            this.callbackButton = button;

            yield return new WaitForSeconds(duration);

            this.callback(callbackButton);

            Destroy(this.gameObject);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                bool isChildren = false;

                foreach (Transform child in transform)
                {
                    if (EventSystem.current.currentSelectedGameObject == child.gameObject)
                    {
                        isChildren = true;

                        break;
                    }
                }

                if (!isChildren)
                {
                    this.callback(null);

                    Destroy(this.gameObject);
                }
            }
        }
    }
}
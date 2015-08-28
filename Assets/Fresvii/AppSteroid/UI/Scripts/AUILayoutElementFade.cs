using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUILayoutElementFade : MonoBehaviour
    {
        private readonly static float duration = 1f;

        private readonly iTween.EaseType easetype = iTween.EaseType.easeOutExpo;

        public float targetHeight;

        private bool initialized;

        public Graphic fade;

        public LayoutElement layoutElement;

        public GameObject[] elements;

        void OnEnable()
        {
            if (!initialized)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }

                StartCoroutine(FadeIn());
            }
        }

        IEnumerator FadeIn()
        {
            fade.gameObject.SetActive(true);

            fade.CrossFadeAlpha(1f, 0f, true);

            if (layoutElement != null)
            {
                layoutElement.preferredHeight = layoutElement.minHeight = 0f;

                iTween.ValueTo(this.gameObject, iTween.Hash("from", 0f, "to", targetHeight, "easetype", easetype, "time", duration * 0.5f, "onupdate", "OnUpdateHeight"));

                yield return new WaitForSeconds(duration * 0.5f);

                layoutElement.preferredHeight = layoutElement.minHeight = targetHeight;
            }

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }

            if (fade.gameObject.activeSelf && this.gameObject.activeInHierarchy)
            {
                fade.CrossFadeAlpha(0f, duration * 0.5f, true);

                yield return new WaitForSeconds(duration * 0.5f);

                fade.gameObject.SetActive(false);

                initialized = true;
            }
        }

        void OnUpdateHeight(float value)
        {
            layoutElement.preferredHeight = layoutElement.minHeight = value;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFadeSetActive : MonoBehaviour {

        public float duration = 0.5f;

        public Graphic[] graphics;

	    // Use this for initialization
        public void FadeIn()
        {
            this.gameObject.SetActive(true);

            foreach (Graphic image in graphics)  
                image.CrossFadeAlpha(0f, 0f, true);

            if (this.gameObject.activeInHierarchy)
                StartCoroutine(FadeInCoroutine());
        }

        IEnumerator FadeInCoroutine()
        {
            yield return 1;

            foreach (Graphic image in graphics)
                image.CrossFadeAlpha(1f, duration, true);
        }

        public void FadeOut()
        {
            foreach (Graphic image in graphics)
                image.CrossFadeAlpha(0f, duration, true);

            if(this.gameObject.activeInHierarchy)
                StartCoroutine(FadeOutCoroutine());
        }

        IEnumerator FadeOutCoroutine()
        {
            yield return new WaitForSeconds(duration);

            this.gameObject.SetActive(false);
        }
    }
}

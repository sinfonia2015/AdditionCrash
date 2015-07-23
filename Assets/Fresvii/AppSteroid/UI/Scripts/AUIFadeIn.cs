using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFade : MonoBehaviour
    {
        public float duration  =0.5f;

        public float fadeIn = 1.0f;

        public float fadeOut = 0.0f;

        public bool fadeInEnable;

        // Use this for initialization
        IEnumerator Start()
        {
            if (fadeInEnable)
            {
                GetComponent<Image>().CrossFadeAlpha(0.0f, 0.0f, true);

                yield return 1;

                GetComponent<Image>().CrossFadeAlpha(fadeIn, duration, true);
            }
        }

        public void FadeOut()
        {
            GetComponent<Image>().CrossFadeAlpha(fadeOut, duration, true);
        }

    }
}
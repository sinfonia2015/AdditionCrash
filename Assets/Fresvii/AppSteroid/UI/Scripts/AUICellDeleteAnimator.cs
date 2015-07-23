using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUICellDeleteAnimator : MonoBehaviour
    {
        public Image fade;

        public float fadeDuration = 0.5f;

        public float shrinkDuration = 0.5f;

        private RectTransform rectTransform;

        public iTween.EaseType easetype = iTween.EaseType.easeOutExpo;

        private AUIScrollViewContents contents;

        private Action<Vector2> callback;

        private Vector2 size;

        public void Animate(AUIScrollViewContents contents, Action<Vector2> callback)
        {
            this.contents = contents;

            this.callback = callback;

            rectTransform = GetComponent<RectTransform>();

            this.size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

            StartCoroutine(AnimCoroutine());
        }

        IEnumerator AnimCoroutine()
        {
            fade.gameObject.SetActive(true);

            fade.CrossFadeAlpha(0f, 0f, true);

            yield return 1;

            fade.CrossFadeAlpha(1f, fadeDuration, true);

            yield return new WaitForSeconds(fadeDuration);

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            iTween.ValueTo(this.gameObject, iTween.Hash("time", shrinkDuration, "from", size.y, "to", 0f, "easetype", easetype, "onupdate", "OnUpdateDeleteCell", "oncomplete", "OnCompleteDeleteCell"));
        }

        void OnUpdateDeleteCell(float value)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, value);

            contents.ReLayout();
        }

        void OnCompleteDeleteCell()
        {
            callback(size);
        }
    }
}

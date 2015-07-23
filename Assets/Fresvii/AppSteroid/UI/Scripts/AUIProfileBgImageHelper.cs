using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIProfileBgImageHelper : MonoBehaviour
    {
        public RectTransform rectTransform;
     
        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

            CalcSize();
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        void OnScreenSizeChanged()
        {
            CalcSize();
        }

        public void CalcSize()
        {
            float length = Mathf.Max(AUIManager.Instance.sizedCanvas.rect.width, 750f);

            rectTransform.sizeDelta = new Vector2(length, length);

            rectTransform.anchoredPosition = new Vector2(0f, length * 0.5f - 310f);
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class AUILayoutRectTransformHelper : MonoBehaviour
    {
        public enum Mode { Width, Height, Both };

        public Mode mode;

        RectTransform rectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (mode == Mode.Width)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f), rectTransform.sizeDelta.y);
            }
            else if (mode == Mode.Height)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f));
            }
            else if (mode == Mode.Both)
            {
                rectTransform.sizeDelta *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;


namespace Fresvii.AppSteroid.UI
{
    public class AUIClipImageSizeControl : MonoBehaviour
    {
        public Vector2 referenceSize;

        void Awake()
        {
            CalcSize();
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

            StartCoroutine(calc());
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        void OnScreenSizeChanged()
        {
            StartCoroutine(calc());
        }

        // Use this for initialization
        void Start()
        {
            StartCoroutine(calc());
		}

        void CalcSize()
        {
            RectTransform rect = GetComponent<RectTransform>();

            rect.sizeDelta = referenceSize * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);
        }

        IEnumerator calc()
        {
            yield return new WaitForEndOfFrame();

            CalcSize();
        }

    }
}
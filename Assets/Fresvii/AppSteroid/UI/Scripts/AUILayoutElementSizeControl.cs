using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;


namespace Fresvii.AppSteroid.UI
{
	public class AUILayoutElementSizeControl : MonoBehaviour
	{
        public RectTransform rectTransform;

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

        float initPreferredHeight;

        float initMinHeight;

        public bool initCalc;

        void Awake()
        {
            LayoutElement elem = GetComponent<LayoutElement>();

            initPreferredHeight = elem.preferredHeight;

            initMinHeight = elem.minHeight;

            if (initCalc)
                CalcSize();
        }

        // Use this for initialization
        void Start()
        {
            StartCoroutine(calc());
        }

		// Use this for initialization
		void CalcSize()
		{
            LayoutElement elem = GetComponent<LayoutElement>();

            elem.preferredHeight = initPreferredHeight * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

            elem.minHeight = initMinHeight * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, elem.minHeight);
            }
		}

        IEnumerator calc()
        {
            yield return new WaitForEndOfFrame();

            CalcSize();
        }
		
	}
}
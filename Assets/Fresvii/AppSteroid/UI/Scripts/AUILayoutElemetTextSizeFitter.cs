using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;


namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(Text))]
    [RequireComponent(typeof(LayoutElement))]
    public class AUILayoutElemetTextSizeFitter : MonoBehaviour
    {
        public enum Mode { Width, Height};

        public Mode mode = Mode.Width;

        Text text;

        LayoutElement elem;

        float initMinHeight, initMinWidth, initPreferredHeight, initPreferredWidth;

        void Awake()
        {
            text = GetComponent<Text>();

            elem = GetComponent<LayoutElement>();
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        void OnScreenSizeChanged()
        {
            StartCoroutine(DelaySet());
        }

        IEnumerator DelaySet()
        {
            yield return new WaitForEndOfFrame();

            Set();
        }

        void Set()
        {
            if(mode == Mode.Width)
            {
                elem.preferredWidth = elem.minWidth = text.preferredWidth;
            }
            else if (mode == Mode.Height)
            {
                elem.preferredHeight = elem.minHeight = text.preferredHeight;
            }
        }

        string preText;

        void Update()
        {
            if (preText != text.text)
            {
                Set();

                preText = text.text;
            }
        }

    }
}

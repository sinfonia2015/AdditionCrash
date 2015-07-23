using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUITouchScreenKeyboardLine : MonoBehaviour
    {
        public RectTransform line;

        public Image lineImage;

#if UNITY_IOS
        public void Update()
        {
            if (TouchScreenKeyboard.visible)
            {
                lineImage.enabled = true;

                line.anchoredPosition = new Vector2(line.anchoredPosition.x, TouchScreenKeyboard.area.height);
            }
            else
            {
                lineImage.enabled = false;
            }
        }
#endif
    }
}
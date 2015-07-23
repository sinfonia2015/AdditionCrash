using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIProgressBar : MonoBehaviour
    {
        public RectTransform area;

        public RectTransform bar;

        public void Set(float value)
        {
            bar.sizeDelta = new Vector2(area.rect.width * value, bar.sizeDelta.y);
        }
    }
}
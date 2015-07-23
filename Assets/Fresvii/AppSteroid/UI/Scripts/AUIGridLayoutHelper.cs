using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGridLayoutHelper : MonoBehaviour
    {
        public GridLayoutGroup gridLayoutGroup;

        public LayoutElement layoutElement;

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
            float gridLength = AUIManager.Instance.sizedCanvas.rect.width / (float)gridLayoutGroup.constraintCount;

            gridLayoutGroup.cellSize = new Vector2(gridLength, gridLayoutGroup.cellSize.y);

            layoutElement.preferredHeight = layoutElement.minHeight = gridLayoutGroup.cellSize.y * Mathf.CeilToInt((float)gridLayoutGroup.transform.childCount / (float)gridLayoutGroup.constraintCount);
        }

    }
}

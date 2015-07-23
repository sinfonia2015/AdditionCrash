using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGridLayoutManager : MonoBehaviour
    {
        public GridLayoutGroup gridLayoutGroup;

        public int portraitGridNum = 3;

        public int landScapeGridNum = 5;

        public LayoutElement layoutElement;

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
            CalcSize();
        }

        void Start()
        {
            CalcSize();
        }

        public void CalcSize()
        {
            int gridColNum = (Screen.width > Screen.height ? landScapeGridNum : portraitGridNum);

            float gridLength = Mathf.Min(AUIManager.Instance.sizedCanvas.rect.width, AUIManager.Instance.sizedCanvas.rect.height) / portraitGridNum;

            gridLayoutGroup.cellSize = new Vector2(gridLength, gridLayoutGroup.cellSize.y);

            float activeChileCount = 0;

            foreach(Transform child in transform)
            {
                if(child.gameObject.activeSelf)
                {
                    activeChileCount++;
                }
            }

            layoutElement.preferredHeight = layoutElement.minHeight = gridLayoutGroup.cellSize.y * Mathf.CeilToInt((float)activeChileCount / (float)gridColNum);
        }

    }
}

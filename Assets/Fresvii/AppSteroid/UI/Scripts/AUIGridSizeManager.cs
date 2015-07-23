using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGridSizeManager : MonoBehaviour
    {
        public GridLayoutGroup gridLayoutGroup;

        public bool paddingBottom;

		public int fixedColCount;

        // Use this for initialization
        void Awake()
        {
			if (fixedColCount > 0) 
			{
				float size = ((Mathf.Min(AUIManager.Instance.sizedCanvas.rect.width, AUIManager.Instance.sizedCanvas.rect.height)) 
				              - gridLayoutGroup.spacing.x * (fixedColCount - 1) 
				              - gridLayoutGroup.padding.left - gridLayoutGroup.padding.right )/ (float)fixedColCount;

				gridLayoutGroup.cellSize = new Vector2(size, size);
			} 
			else 
			{
				float scale = Mathf.Min (1.0f, AUIManager.Instance.auiCanvasScaleManager.scale);

				gridLayoutGroup.cellSize *= scale;

				gridLayoutGroup.spacing *= scale;

				gridLayoutGroup.padding.top = Mathf.FloorToInt (gridLayoutGroup.padding.top * scale);

				if (!paddingBottom)
					gridLayoutGroup.padding.bottom = Mathf.FloorToInt (gridLayoutGroup.padding.bottom * scale);

				gridLayoutGroup.padding.left = Mathf.FloorToInt (gridLayoutGroup.padding.left * scale);

				gridLayoutGroup.padding.right = Mathf.FloorToInt (gridLayoutGroup.padding.right * scale);
			}
        }

    }
}
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;


namespace Fresvii.AppSteroid.UI
{
	public class AUIRectTransformHeightCalc : MonoBehaviour {

		public LayoutElement[] sizedHeightLayoutElement;
		
		public LayoutElement[] nonSizedHeightLayoutElement;
		
		// Use this for initialization
		void Start()
		{			
			RectTransform rt = GetComponent<RectTransform>();
			
			float height = 0;
			
			foreach (LayoutElement h in sizedHeightLayoutElement)
				height += h.preferredHeight;
			
			foreach (LayoutElement h in nonSizedHeightLayoutElement)
				height += h.preferredHeight;
			
			rt.sizeDelta = new Vector2 (rt.sizeDelta.x, height);
		}
	}
}
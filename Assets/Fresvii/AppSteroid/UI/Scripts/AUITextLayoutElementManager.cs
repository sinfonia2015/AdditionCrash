using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
	public class AUITextLayoutElementManager : MonoBehaviour
	{
        public enum Mode {Height, Width, Both};

        public Mode mode = Mode.Height;

		public LayoutElement layoutElement;
		
		public Text textUi;
		
		string text;

		public float margin = 30f;

        public AUITextSetter textSetter;

		// Use this for initialization
		void Update()
		{
			if (textUi.text != text) 
			{
                if (mode == Mode.Height || mode == Mode.Both)
                    layoutElement.minHeight = layoutElement.preferredHeight = textUi.preferredHeight + margin;
            
                if (mode == Mode.Width || mode == Mode.Both)
                    layoutElement.minWidth = layoutElement.preferredWidth = textUi.preferredWidth + margin;

                if (textSetter != null)
                    textSetter.Truncate();

                text = textUi.text;
            }
		}
		
	}
}
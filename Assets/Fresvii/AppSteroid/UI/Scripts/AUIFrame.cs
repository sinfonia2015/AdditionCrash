using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class AUIFrame : MonoBehaviour
    {
        private float tweenTime = 0.4f;

        iTween.EaseType easetype = iTween.EaseType.easeOutCirc;

        public string title;

        public AUIFrame backFrame;
        
        [HideInInspector]
        public Vector2 RefResolution;

        private Action callback;

        public bool Animating;

		public static bool SomethingAnimationg;

        public void Animate(Vector2 from, Vector2 to, Action callback)
        {
            this.callback = callback;

            Animating = true;

			SomethingAnimationg = true;

            SetPosition(from);

            iTween.ValueTo(this.gameObject, iTween.Hash("from", from, "to", to, "time", tweenTime, "easetype", easetype, "onupdate", "UpdatePosition", "oncomplete", "OnComplete"));
        }

        public void SetPosition(Vector2 pos)
        {
            GetComponent<RectTransform>().anchoredPosition = pos;
        }

        void UpdatePosition(Vector2 pos)
        {
            GetComponent<RectTransform>().anchoredPosition = pos;
        }

        void OnComplete()
        {
            if (this.callback != null)
            {
                callback();
            }

            Animating = false;

			SomethingAnimationg = false;
        }

		void OnDisable()
		{
			SomethingAnimationg = false;
		}
    }
}
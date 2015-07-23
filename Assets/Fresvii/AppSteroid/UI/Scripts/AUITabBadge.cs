using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUITabBadge : MonoBehaviour
    {
        public Text countText;

        private uint count;

        public RectTransform rectTransform;

        public Vector2 originalSize = new Vector2(32f, 32f);

        public float duration = 0.5f;

        public iTween.EaseType easetype;

        public uint Count
        {
            get { return count; }

            set
            {
                if (this == null) return;

                count = value;

                countText.text = (count > 25) ? "25+" : count.ToString();

                if (this.gameObject.activeSelf && count <= 0)
                {
                    iTween.ValueTo(this.gameObject, iTween.Hash("from", rectTransform.sizeDelta, "to", Vector2.zero, "time", duration, "easytype", easetype, "onupdate", "UpdateSize", "oncomplete", "OnCompleteHide"));
                }
                else if (!this.gameObject.activeSelf && count > 0)
                {
                    rectTransform.sizeDelta = Vector2.zero;

                    this.gameObject.SetActive(true);

                    iTween.ValueTo(this.gameObject, iTween.Hash("from", Vector2.zero, "to", originalSize, "time", duration, "easytype", easetype, "onupdate", "UpdateSize"));                    
                }
            }
        }
       
        void UpdateSize(Vector2 value)
        {
            rectTransform.sizeDelta = value;
        }

        void OnCompleteHide()
        {
            this.gameObject.SetActive(false);
        }
    }
}

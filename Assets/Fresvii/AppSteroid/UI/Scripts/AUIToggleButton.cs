using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(Button))]
    public class AUIToggleButton : MonoBehaviour
    {
        private Image image;

        public Sprite onGraphic, offGraphic;

        // Use this for initialization
        void Awake()
        {
            image = GetComponent<Button>().targetGraphic.GetComponent<Image>();
        }

        public void Set(bool on)
        {
            image.sprite = on ? onGraphic : offGraphic;
        }
    }
}

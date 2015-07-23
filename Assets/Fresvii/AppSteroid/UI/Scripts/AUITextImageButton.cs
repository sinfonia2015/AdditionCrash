using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUITextImageButton : MonoBehaviour
    {
        public Text text;

        public Image image;
        
        // Update is called once per frame
        void Update()
        {
            text.color = image.canvasRenderer.GetColor();
        }
    }
}
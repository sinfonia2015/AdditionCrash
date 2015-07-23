using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent (typeof(RawImage))]
    public class AUIAppIcon : MonoBehaviour
    {
        void Start()
        {
            RawImage image = GetComponent<RawImage>();

            image.texture = FASSettings.Settings.appIcon;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(LayoutElement))]
    public class AUILayoutElementHelper : MonoBehaviour
    {
        public enum Mode { Width, Height, Both };

        public Mode mode;

        void Awake()
        {
            LayoutElement lm = GetComponent<LayoutElement>();

            if (mode == Mode.Width)
            {
                lm.minWidth *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

                lm.preferredWidth *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);
            }
            else if (mode == Mode.Height)
            {
                lm.minHeight *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

                lm.preferredHeight *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);
            }
            else if (mode == Mode.Both)
            {
                lm.minWidth *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

                lm.preferredWidth *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

                lm.minHeight *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

                lm.preferredHeight *= Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);
            }
        }
    }
}

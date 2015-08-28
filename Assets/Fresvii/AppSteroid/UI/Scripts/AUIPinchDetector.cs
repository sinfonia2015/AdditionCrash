using UnityEngine;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIPinchDetector : MonoBehaviour
    {
        public event Action OnPinchStart;

        public event Action<float> OnPinch;

        public event Action OnPinchEnd;

        public float zoomSpeed = 0.5f;  // The rate of change of the field of view in perspective mode.

        public bool Piching = false;

        float initDistance;

        void Update()
        {
            // If there are two touches on the device...
            if (Input.touchCount == 2)
            {
                if (!Piching)
                {
                    Piching = true;

                    if (OnPinchStart != null)
                        OnPinchStart();

                    initDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                }

                float currentDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
               
                if(OnPinch != null){

                    OnPinch(currentDistance / initDistance);
                }
            }
            else
            {
                if (Piching)
                {
                    Piching = false;

                    if (OnPinchEnd != null)
                    {
                        OnPinchEnd();
                    }
                }
            }

#if UNITY_EDITOR

            float enlarge = Input.GetAxis("Mouse ScrollWheel");

            if (OnPinch != null)
            {
                if (enlarge < 0f)
                {
                    OnPinch(5f);
                }
                if (enlarge > 0f)
                {
                    OnPinch(-5f);
                }
            }
#endif
        }
    }
}

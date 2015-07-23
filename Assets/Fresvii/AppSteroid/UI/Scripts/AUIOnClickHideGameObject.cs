using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIOnClickHideGameObject : MonoBehaviour
    {
        bool hide = false;

        // Update is called once per frame
        void Update()
        {
            if (hide)
            {
                this.gameObject.SetActive(false);

                hide = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                hide = true;
            }
        }
    }
}
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIToggleButtonGroup : MonoBehaviour
    {
        public AUIToggleButton[] buttons;

        public bool init;

        public int selectedIndex = 0;

        public event Action<int> OnChanged;

        void Start()
        {
            if (init)
            {
                Set();
            }
        }

        public void SetIndex(int index)
        {
            if (selectedIndex != index)
            {
                selectedIndex = index;

                if (OnChanged != null)
                {
                    OnChanged(selectedIndex);
                }

                Set();
            }
        }

        void Set()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Set(i == selectedIndex);
            }
        }       
    }
}

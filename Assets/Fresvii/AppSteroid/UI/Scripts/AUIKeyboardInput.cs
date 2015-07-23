using UnityEngine;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIKeyboardInput : MonoBehaviour
    {
        private static AUIKeyboardInput instance;

        private TouchScreenKeyboard keyboard;

        private Action<string> callback;

        private string initString;

        public static AUIKeyboardInput Show(string text, bool multiline, Action<string> callback)
        {
            instance = (new GameObject()).AddComponent<AUIKeyboardInput>();

            instance.callback = callback;

            instance.initString = text;

            instance.keyboard = TouchScreenKeyboard.Open(text, TouchScreenKeyboardType.Default, false, multiline, false, false);

            return instance;
        }

        void Hide()
        {
            if (keyboard != null)
            {
                keyboard.active = false;

                keyboard = null;
            }

            Destroy(this.gameObject);
        }

        void Update()
        {
            if (keyboard == null) return;

            if (keyboard.wasCanceled)
            {
                callback(initString);

                Hide();
            }
            else if (keyboard.done)
            {
                callback(keyboard.text);

                Hide();
            }

            if (Input.GetMouseButtonDown(0))
            {
                callback(initString);

                Hide();
            }

        }
    }
}
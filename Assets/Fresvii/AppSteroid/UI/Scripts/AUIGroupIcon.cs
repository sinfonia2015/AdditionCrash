using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGroupIcon : MonoBehaviour
    {
        public GameObject groupIcon1, groupIcon2, groupIcon3;

        public AUIRawImageTextureSetter[] userIcons1;

        public AUIRawImageTextureSetter[] userIcons2;

        public AUIRawImageTextureSetter[] userIcons3;

        public void Set(string[] urls)
        {
            if (urls == null)
            {
                return;
            }
            else if (urls.Length <= 1)
            {
                groupIcon1.SetActive(true);

                for (int i = 0; i < urls.Length; i++)
                {
                    userIcons1[i].Set(urls[i]);
                }
            }
            else if (urls.Length == 2)
            {
                groupIcon2.SetActive(true);

                for (int i = 0; i < urls.Length; i++)
                {
                    userIcons2[i].Set(urls[i]);
                }
            }
            else
            {
                groupIcon3.SetActive(true);

                for (int i = 0; i < urls.Length; i++)
                {
                    userIcons3[i].Set(urls[i]);
                }
            }
        }
    }
}

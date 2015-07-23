using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFriendRequestCell : MonoBehaviour
    {
        public Text textRequestCount;

        private uint requestCount = 0;

        [HideInInspector]
        public AUIFriendList parentFrame;

        public uint RequestCount
        {
            get { return requestCount;}

            set
            {
                requestCount = value;

                textRequestCount.text = (requestCount > 25) ? "25+" : requestCount.ToString();
            }
        }

        public void OnTap()
        {
            parentFrame.GoFriendRequest();
        }
    }
}
    
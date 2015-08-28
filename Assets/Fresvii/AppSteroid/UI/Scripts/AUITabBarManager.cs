using UnityEngine;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUITabBarManager : MonoBehaviour
    {
        public bool on = true;

        void OnEnable()
        {
            if (AUITabBar.Instance != null)
            {
                AUITabBar.Instance.gameObject.SetActive(on);
            }
            else
            {
                StartCoroutine(WaitGetInstance());
            }
        }

        IEnumerator WaitGetInstance()
        {
            yield return 1;

            OnEnable();
        }

        public void Off()
        {
            AUITabBar.Instance.gameObject.SetActive(false);
        }
    }
}
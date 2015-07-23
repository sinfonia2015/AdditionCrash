using UnityEngine;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUITabBarManager : MonoBehaviour
    {
        public bool on = true;

        int retryCount = 3;

        void OnEnable()
        {
            retryCount = 3;
        }

        void LateUpdate()
        {
            if (--retryCount > 0)
            {
                if (on)
                {
                    AUITabBar.Instance.transform.SetParent(transform.parent);

                    AUITabBar.Instance.transform.SetAsLastSibling();
                }
                else
                {
                    AUITabBar.Instance.transform.SetParent(transform.parent);

                    AUITabBar.Instance.transform.SetAsFirstSibling();
                }
            }
            else
            {
                if (on && AUITabBar.Instance.transform.GetSiblingIndex() != AUITabBar.Instance.transform.childCount - 1)
                {
                    AUITabBar.Instance.transform.SetParent(transform.parent);

                    AUITabBar.Instance.transform.SetAsLastSibling();
                }
                else if (!on && AUITabBar.Instance.transform.GetSiblingIndex() != 0)
                {
                    AUITabBar.Instance.transform.SetParent(transform.parent);

                    AUITabBar.Instance.transform.SetAsFirstSibling();
                }
            }

        }

        public void On()
        {
            on = true;

            StartCoroutine(SetTabOn());
        }

        IEnumerator SetTabOn()
        {
            yield return new WaitForEndOfFrame();

            if (this == null) yield break;

        }

        public void Off()
        {
            on = false;

            StartCoroutine(SetTabOff());
        }

        IEnumerator SetTabOff()
        {
            yield return new WaitForEndOfFrame();

            if (this == null) yield break;

            AUITabBar.Instance.transform.SetParent(transform.parent);

            AUITabBar.Instance.transform.SetAsFirstSibling();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIPageViewSender : MonoBehaviour
    {
        public bool showOfflineDialog = true;

        public string path;

        public string objectId;

        void OnEnable()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable && showOfflineDialog)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), delegate(bool del) { });
            }
            else
            {
                StartCoroutine(SendCoroutine());
            }
        }

        IEnumerator SendCoroutine()
        {
            while (!AUIManager.Instance.Initialized)
                yield return 1;

            yield return new WaitForSeconds(1f);

            if (this.gameObject.activeSelf)
            {
                FASUtility.SendPageView(path, objectId, System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                    {
                        Debug.LogError(e.ToString());
                    }
                });
            }
        }
        
    }
}

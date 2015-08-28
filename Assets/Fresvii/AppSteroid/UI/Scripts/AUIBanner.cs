using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIBanner : MonoBehaviour
    {
        public AUIFrame parentFrame;

        public GameObject prfbBannerCell;

        public Transform contents;

        List<AUIBannerCell> cells = new List<AUIBannerCell>();

        private static float interval = 20f;

        void OnEnable()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            yield return 1;

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASAdvertisement.GetAppList(1, OnGetAppList);
        }

        void OnGetAppList(IList<Fresvii.AppSteroid.Models.App> apps, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null || this.enabled == false)
            {
                return;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            foreach (var app in apps)
            {
                if (string.IsNullOrEmpty(app.BannerImageUrl))
                    continue;

                var cell = cells.Find(x => x.App.Id == app.Id);

                if (cell != null)
                {
                    cell.SetApp(app, parentFrame);

                    continue;
                }

                var item = ((GameObject)Instantiate(prfbBannerCell)).GetComponent<RectTransform>();

                item.transform.SetParent(contents, false);

                item.transform.SetSiblingIndex(0);

                cell = item.GetComponent<AUIBannerCell>();

                cell.SetApp(app, parentFrame);

                cells.Add(cell);
            }

            if(this.gameObject.activeInHierarchy)
                StartCoroutine(BannerAnimation());
        }

        int showIndex = -1;

        float shownTime;

        IEnumerator BannerAnimation()
        {
            while (cells.Count == 0)
                yield return 1;

            while (true)
            {
                showIndex = Random.Range(0, cells.Count);

                cells[showIndex].Show();

                StartCoroutine(SendImpression(cells[showIndex].App.Id));

                yield return new WaitForSeconds(interval);

                if (cells.Count != 1)
                {
                    bool hiding = true;

                    cells[showIndex].Hide(() =>
                    {
                        hiding = false;
                    });

                    while (hiding)
                        yield return 1;
                }                
            }
        }

        IEnumerator SendImpression(string addId)
        {
            while (!AUIManager.Instance.Initialized)
                yield return 1;

            FASUtility.SendPageView("event.ad.impression", addId, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });    
        }
    }
}
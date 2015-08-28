using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIRecommendedApps : MonoBehaviour
    {
        public enum Mode { AdvertisementApps, DeveloperApps };

        public Mode mode;

        public AUIFrame frame;

        public Transform[] appCells;

        public Text title;

        public Text bannerText;

        public AUIRecommendedAppLayoutCell[] layoutCells;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            AUIManager.Instance.HideLoadingSpinner();
        }

        IEnumerator Start()
        {
            AUIManager.Instance.ShowLoadingSpinner();

			for (int i = 0; i < layoutCells.Length; i++) 
			{
				layoutCells[i].gameObject.SetActive(false);
			}

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            if(mode == Mode.AdvertisementApps)
            {
                if(title != null)
                    title.text = FASText.Get("HotApps");

                if(bannerText != null)
                    bannerText.text = FASText.Get("HotAppsThisWeek");

                FASAdvertisement.GetAppList(1, OnGetAppList);

                FASUtility.SendPageView("pv.app_gellery.apps", "", System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                        Debug.LogError(e.ToString());
                });
            }
            else if (mode == Mode.DeveloperApps)
            {
                title.text = FASText.Get("MoreApps");

                bannerText.text = FASText.Get("MoreAppsThisWeek");

                FASUtility.GetDeveloperAppList(1, OnGetAppList);

                FASUtility.SendPageView("pv.community.apps", "", System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                        Debug.LogError(e.ToString());
                });
            }
        }

        void OnGetAppList(IList<Fresvii.AppSteroid.Models.App> apps, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            AUIManager.Instance.HideLoadingSpinner();

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

            if (apps.Count > 0 && mode == Mode.DeveloperApps)
            {
                title.text = FASText.Get("MoreAppsBy").Replace("%developer", apps[0].GameDeveloper.Name);
            }

            // Random layout
            List<int> bag = new List<int>();

            for (int i = 0; i < appCells.Length; i++)
                bag.Add(i);

            List<int> order = new List<int>();

            while (bag.Count > 0)
            {
                int index = Random.Range(0, bag.Count);

                order.Add(bag[index]);

                bag.RemoveAt(index);
            }

            for (int i = 0; i < order.Count; i++)
                appCells[order[i]].SetSiblingIndex(appCells.Length + 1);


            // padding apps from upper
            List<Fresvii.AppSteroid.Models.App> randomApps = new List<Models.App>();

            int it = 0;

            while (apps.Count > 0 && it < 6)
            {
                int index = Random.Range(0, apps.Count);

                randomApps.Add(apps[index]);

                apps.RemoveAt(index);

                it++;
            }

            int appIndex = 0;

            for(int i = 0; i < layoutCells.Length; i++)
            {
                int activeCellCount = 0;

                for (int j = 0; j < layoutCells[order[i]].cells.Length; j++)
                {
                    if(appIndex < randomApps.Count)
                    {
                        layoutCells[order[i]].cells[j].SetApp(randomApps[appIndex], this);

                        activeCellCount++;
                    }
                    else
                    {
                        layoutCells[order[i]].cells[j].SetApp(null, this);
                    }

                    if (j == 0 && appIndex >= randomApps.Count)
                    {
                        layoutCells[order[i]].gameObject.SetActive(false);
                    }
                    else
                    {
                        layoutCells[order[i]].gameObject.SetActive(true);
                    }

                    appIndex++;
                }

                layoutCells[order[i]].gameObject.SetActive(activeCellCount > 0);

                /*if (activeCellCount > 0 && appIndex < randomApps.Count)
                {
                    layoutCells[order[i]].line.SetActive(true);
                }*/
            }

            /*bool hasTwoLine = false;

            for (int i = 0; i < 2; i++)
            {
                hasTwoLine |= layoutCells[1].cells[i].IsTwoLine();
            }

            if (hasTwoLine)
            {
                layoutCells[1].GetComponent<LayoutElement>().preferredHeight += app2OffSetHeight;

                layoutCells[1].GetComponent<LayoutElement>().minHeight += app2OffSetHeight;
            }*/
        }

        public float app2OffSetHeight = -20f;

        public void Back()
        {
            if (frame.Animating) return;

            if (frame.backFrame != null)
            {
                frame.backFrame.gameObject.SetActive(true);

                RectTransform rectTransform = GetComponent<RectTransform>();

                frame.backFrame.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
                {
                    Destroy(this.gameObject);
                });
            }
        }

        public GameObject prfbAppDetail;

        public void GoToAppDetail(Fresvii.AppSteroid.Models.App app)
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiAppDetail = ((GameObject)Instantiate(prfbAppDetail)).GetComponent<AUIAppDetail>();

            auiAppDetail.SetApp(app);

            auiAppDetail.transform.SetParent(transform.parent, false);

            auiAppDetail.transform.SetAsLastSibling();

            auiAppDetail.frame.backFrame = this.frame;

            auiAppDetail.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

    }
}
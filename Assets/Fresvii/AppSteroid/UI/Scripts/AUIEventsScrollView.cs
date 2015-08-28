using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Fresvii.AppSteroid.UI
{
    public class AUIEventsScrollView : MonoBehaviour
    {        
        public AUIEvents auiEvents;

        public GameObject prfbEventCell;

        public AUIScrollViewContents contents;

        public Fresvii.AppSteroid.Models.GameEvent.Status mode;

        private List<AUIEventCell> cells = new List<AUIEventCell>();

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public Graphic fade;

        public float fadeDuration = 0.3f;

        bool initialized;

        IEnumerator FadeOutMask(Graphic g)
        {
            g.CrossFadeAlpha(0f, fadeDuration, true);

            yield return new WaitForSeconds(fadeDuration);

            g.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.Instance.HideLoadingSpinner();

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;
        }

        void OnPullUpReflesh()
        {
            pullReflesh.PullRefleshCompleted();
        }

        void Awake()
        {
            fade.gameObject.SetActive(true);
        }

        IEnumerator Init()
        {
            if(!initialized)
                AUIManager.Instance.ShowLoadingSpinner();

            yield return 1;

            while (!FASUser.IsLoggedIn() || auiEvents.frame.Animating)
            {
                yield return 1;
            }

            if (!this.gameObject.activeSelf)
            {
                yield break;
            }

            FASLeaderboard.GetEventList(mode, OnGetEventList);
        }

        void OnGetEventList(IList<Fresvii.AppSteroid.Models.GameEvent> events, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            initialized = true;

            AUIManager.Instance.HideLoadingSpinner();

            if(fade.gameObject.activeSelf)
                StartCoroutine(FadeOutMask(fade));

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

            foreach (Fresvii.AppSteroid.Models.GameEvent gameEvent in events)
            {
                var cell = cells.Find(x => x.GameEvent.Id == gameEvent.Id);

                if (cell != null)
                {
                    cell.SetGameEvent(gameEvent, this.auiEvents, this.contents, () => { });

                    return;
                }

                var item = ((GameObject)Instantiate(prfbEventCell)).GetComponent<RectTransform>();

                cell = item.GetComponent<AUIEventCell>();

                cell.SetGameEvent(gameEvent, this.auiEvents, this.contents, () =>
                {

                });

                contents.AddItem(item);

                cells.Add(cell);

            }

            if (mode == Models.GameEvent.Status.Upcoming)
            {
                cells.Sort((a, b) => System.DateTime.Compare(a.GameEvent.StartAt, b.GameEvent.StartAt));
            }
            else if (mode == Models.GameEvent.Status.Past)
            {
                cells.Sort(SortEventsCondition);
            }
            else
            {
                cells.Sort(SortEventsCondition);
            }

            foreach (var obj in cells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            contents.ReLayout();
        }

        int SortEventsCondition(AUIEventCell a, AUIEventCell b)
        {
            int ret = System.DateTime.Compare(a.GameEvent.EndAt, b.GameEvent.EndAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = System.DateTime.Compare(a.GameEvent.StartAt, b.GameEvent.StartAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = string.Compare(a.GameEvent.Id, b.GameEvent.Id);

            return ret;
        }
    }
}

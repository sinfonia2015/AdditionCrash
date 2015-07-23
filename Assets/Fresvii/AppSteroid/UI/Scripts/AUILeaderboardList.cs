using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Fresvii.AppSteroid.UI
{
    public class AUILeaderboardList : MonoBehaviour
    {
        public AUIFrame frameTween;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public GameObject prfbLeaderboardListCell;

        public AUIScrollViewContents contents;

        private List<AUILeaderboardListCell> cells = new List<AUILeaderboardListCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public GameObject prfbLeaderboard;

        public Text title;

        bool initilized;

        void OnEnable()
        {
            //AUIManager.OnEscapeTapped += Back;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            //AUIManager.OnEscapeTapped -= Back;
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                isPullRefleshProc = true;

                FASLeaderboard.GetLeaderboards((uint)listMeta.NextPage, OnGetLeaderboards);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }
       
        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASLeaderboard.GetLeaderboards(OnGetLeaderboards);
        }

        void OnGetLeaderboards(IList<Fresvii.AppSteroid.Models.Leaderboard> leaderboards, Fresvii.AppSteroid.Models.ListMeta meta,  Fresvii.AppSteroid.Models.Error error)
        {
            initilized = true;

            if (this == null || this.enabled == false || !this.gameObject.activeInHierarchy)
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

            if (this.listMeta == null)
            {
                this.listMeta = meta;
            }
            else if (this.listMeta.CurrentPage != 0)
            {
                this.listMeta = meta;
            }

            foreach (Fresvii.AppSteroid.Models.Leaderboard leaderboard in leaderboards)
            {
                UpdateLeaderboard(leaderboard);
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }
        }

        private bool UpdateLeaderboard(Fresvii.AppSteroid.Models.Leaderboard leaderboard)
        {
            AUILeaderboardListCell cell = cells.Find(x => x.Leaderboard.Id == leaderboard.Id);

            if (cell != null)
            {
                cell.SetLeaderboard(leaderboard, this);

                return false;
            }

            GameObject go = Instantiate(prfbLeaderboardListCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUILeaderboardListCell>();

            cell.SetLeaderboard(leaderboard, this);

            cells.Add(cell);

            cell.gameObject.SetActive(false);

            return true;
        }

        public void GoToLeaderboard(Fresvii.AppSteroid.Models.Leaderboard leaderboard)
        {
            if (frameTween.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUILeaderboard auiLeaderboard = ((GameObject)Instantiate(prfbLeaderboard)).GetComponent<AUILeaderboard>();

            auiLeaderboard.SetLeaderboard(leaderboard);

            auiLeaderboard.transform.SetParent(transform.parent, false);

            auiLeaderboard.transform.SetAsLastSibling();

            auiLeaderboard.parentFrameTween = this.frameTween;

            auiLeaderboard.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public void Back()
        {
            if (frameTween.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }
    }
}

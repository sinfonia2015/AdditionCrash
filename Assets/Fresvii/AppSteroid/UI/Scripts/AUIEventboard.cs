using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIEventboard : MonoBehaviour
    {
        public AUIFrame frame;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public Fresvii.AppSteroid.Models.Eventboard Eventboard { get; protected set; }

        public Text title;

        //public Text backButtonText;

        public GameObject nodeAll, nodeFriendOnly;

        public AUISegmentedControl segmentedControl;

        public GameObject prfbMyPage, prfbUserPage;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            segmentedControl.OnChanged += OnSegmentedControlChanged;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            segmentedControl.OnChanged -= OnSegmentedControlChanged;
        }

        IEnumerator Start()
        {
            while (!AUIManager.Instance.Initialized)
                yield return 1;

            FASUtility.SendPageView("pv.leaderboards.show", this.Eventboard.Leaderboard.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });
        }

        void OnSegmentedControlChanged(int index)
        {
            nodeAll.gameObject.SetActive(index == 0);

            nodeFriendOnly.gameObject.SetActive(index == 1);
        }

        public void SetBackButton(string text)
        {
            //backButtonText.text = text;
        }

        public void SetEventboard(Fresvii.AppSteroid.Models.Eventboard eventboard)
        {
            this.Eventboard = eventboard;

            title.text = eventboard.Leaderboard.Name;
        }

        public void Back()
        {
            if (frame.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (user.Id == FAS.CurrentUser.Id)
            {
                AUIMyPage myPage = ((GameObject)Instantiate(prfbMyPage)).GetComponent<AUIMyPage>();

                myPage.transform.SetParent(transform.parent, false);

                myPage.transform.SetAsLastSibling();

                myPage.parentFrameTween = this.frame;

                myPage.backButtonText.text = title.text;

                myPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }
            else
            {
                AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

                userPage.transform.SetParent(transform.parent, false);

                userPage.Set(user, title.text, this.frame);

                userPage.transform.SetAsLastSibling();

                userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }
    }
}

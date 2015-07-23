using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFriendRequest : MonoBehaviour
    {
        public AUIFrame frameTween;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public GameObject prfbMessageCell;

        private List<AUIFriendRequestedCell> requestedCells = new List<AUIFriendRequestedCell>();

        private Fresvii.AppSteroid.Models.ListMeta meta;

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public Text title;

        //public Text backButtonText;

        public GameObject prfbUserPage;

        public AUISegmentedControl segmentedControl;

        public GameObject nodeRequestView, nodeHiddenView;

        public void SetBackButton(string text)
        {
            //backButtonText.text = text;
        }

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            segmentedControl.OnChanged += OnSegmentChaned;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            segmentedControl.OnChanged -= OnSegmentChaned;
        }

        public void OnSegmentChaned(int index)
        {
            nodeRequestView.gameObject.SetActive(index == 0);

            nodeHiddenView.gameObject.SetActive(index == 1);
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

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frameTween.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

            userPage.transform.SetParent(transform.parent, false);

            userPage.Set(user, FASText.Get("FriendRequests"), frameTween);

            userPage.transform.SetAsLastSibling();

            userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }
    }
}

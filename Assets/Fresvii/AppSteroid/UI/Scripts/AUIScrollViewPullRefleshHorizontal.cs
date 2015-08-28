using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(AUIScrollRect))]
    public class AUIScrollViewPullRefleshHorizontal : MonoBehaviour
    {
        private enum PullRefleshType { None, PullLeft, PullRight }

        private PullRefleshType pullRefleshing = PullRefleshType.None;

        public event Action OnPullRightReflesh;

        public event Action OnPullLeftReflesh;

        public RectTransform content;

        public int pullRefleshLength;

        private bool isTouching;

        private GameObject loadingSpinner;

        private AUIScrollRect scrollRect;

        private RectTransform scrollView;

        private RectTransform loadingSpinnerRectTransform;

        public bool showLoadingSpinner = true;

        // Use this for initialization
        void Start()
        {
            scrollView = GetComponent<RectTransform>();

            scrollRect = GetComponent<AUIScrollRect>();
        }

        void Update()
        {
            if (content.anchoredPosition.x > pullRefleshLength && pullRefleshing == PullRefleshType.None && !isTouching && scrollRect.IsDrag)
            {
                pullRefleshing = PullRefleshType.PullRight;

                if (OnPullRightReflesh != null)
                {
                    OnPullRightReflesh();
                }

                isTouching = true;
            }
            else if (content.sizeDelta.x > scrollView.rect.width && content.anchoredPosition.x + content.sizeDelta.x - scrollView.rect.width < -pullRefleshLength && pullRefleshing == PullRefleshType.None && !isTouching && scrollRect.IsDrag)
            {
                pullRefleshing = PullRefleshType.PullLeft;

                if (OnPullLeftReflesh != null)
                {
                    OnPullLeftReflesh();
                }

                isTouching = true;
            }

            if (isTouching && !scrollRect.IsDrag)
            {
                isTouching = false;
            }
        }

        private bool pinned;

        private Vector2 prevPos;

        public void PullRefleshCompleted(bool pinned = false)
        {            
            pullRefleshing = PullRefleshType.None;
        }
    }
}
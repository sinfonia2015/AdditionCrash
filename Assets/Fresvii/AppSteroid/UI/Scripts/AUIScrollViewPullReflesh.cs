using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(AUIScrollRect))]
    public class AUIScrollViewPullReflesh : MonoBehaviour
    {
        private enum PullRefleshType {None, PullDown, PullUp}

        private PullRefleshType pullRefleshing = PullRefleshType.None;
       
        public event Action OnPullDownReflesh;

        public event Action OnPullUpReflesh;

        public RectTransform content;

        public int pullRefleshHeight;

        private bool isTouching;

        private GameObject loadingSpinner;

        private AUIScrollRect scrollRect;

        private RectTransform scrollView;

        private AUIScrollViewContents scrollViewContents;

        private RectTransform loadingSpinnerRectTransform;

        public bool showLoadingSpinner = true;

        public bool autoPullUp = false;

        // Use this for initialization
        void Start()
        {
            scrollView = GetComponent<RectTransform>();

            scrollRect = GetComponent<AUIScrollRect>();

            scrollViewContents = content.GetComponent<AUIScrollViewContents>();
        }

        void OnEnable()
        {
            pullRefleshing = PullRefleshType.None;
        }

        void Update()
        {
            if (content.anchoredPosition.y < -pullRefleshHeight && pullRefleshing == PullRefleshType.None && !isTouching && scrollRect.IsDrag)
            {
                pullRefleshing = PullRefleshType.PullDown;

                if (OnPullDownReflesh != null)
                {
                    if (showLoadingSpinner)
                    {
                        loadingSpinner = (GameObject)Instantiate(Resources.Load("AUIPullRefleshLoadingSpinner"));

                        loadingSpinnerRectTransform = loadingSpinner.GetComponent<RectTransform>();

                        scrollViewContents.AddItem(loadingSpinnerRectTransform, 0);

                        scrollRect.velocity = Vector2.zero;

                        scrollViewContents.ReLayout();
                    }

                    OnPullDownReflesh();
                }

                isTouching = true;
            }
            else if (content.sizeDelta.y > scrollView.rect.height && content.anchoredPosition.y - content.sizeDelta.y + scrollView.rect.height > pullRefleshHeight && pullRefleshing == PullRefleshType.None && !isTouching && scrollRect.IsDrag)
            {
                pullRefleshing = PullRefleshType.PullUp;

                if (OnPullUpReflesh != null)
                {
                    if (showLoadingSpinner)
                    {
                        loadingSpinner = (GameObject)Instantiate(Resources.Load("AUIPullRefleshLoadingSpinner"));

                        loadingSpinnerRectTransform = loadingSpinner.GetComponent<RectTransform>();

                        scrollViewContents.AddItem(loadingSpinnerRectTransform);

                        scrollRect.velocity = Vector2.zero;

                        scrollViewContents.ReLayout();
                    }

                    OnPullUpReflesh();
                }

                isTouching = true;
            }
            else if (autoPullUp && content.sizeDelta.y > scrollView.rect.height && content.anchoredPosition.y - content.sizeDelta.y + scrollView.rect.height > - 5f * scrollView.rect.height && pullRefleshing == PullRefleshType.None && !isTouching && scrollRect.IsDrag)
            {
                pullRefleshing = PullRefleshType.PullUp;

                if (OnPullUpReflesh != null)
                {
                    OnPullUpReflesh();
                }
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

            if (!showLoadingSpinner)
            {
                return;
            }

            if (this == null || this.enabled == false)
            {
                return;
            }

            if (!this.gameObject.activeInHierarchy) return;

            this.pinned = pinned;


            if (!pinned)
            {
                iTween.ValueTo(this.gameObject, iTween.Hash("from", pullRefleshHeight, "to", 0f, "time", 0.5f, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "TweenPullRefleshDone", "oncomplete", "OnCompleteTweenPullRefleshDone"));
            }
            else
            {
                if (loadingSpinner != null)
                {
                    scrollViewContents.RemoveItem(loadingSpinnerRectTransform);

                    Destroy(loadingSpinner);
                }

                scrollViewContents.ReLayout();

                StartCoroutine(StopScrollCoroutine());
            }
        }

        IEnumerator StopScrollCoroutine()
        {
            yield return new WaitForEndOfFrame();

            scrollRect.vertical = true;

            scrollRect.StopMovement();

            scrollRect.velocity = Vector2.zero;
        }

        void TweenPullRefleshDone(float value)
        {
            if (this == null || loadingSpinnerRectTransform == null)
            {
                return;
            }

            loadingSpinnerRectTransform.sizeDelta = new Vector2(loadingSpinnerRectTransform.sizeDelta.x, value);
            
            scrollViewContents.ReLayout();
        }

        void OnCompleteTweenPullRefleshDone()
        {
            if (loadingSpinner != null)
            {
                scrollViewContents.RemoveItem(loadingSpinnerRectTransform);

                Destroy(loadingSpinner);
            }

            pullRefleshing = PullRefleshType.None;

            scrollViewContents.ReLayout();

            StartCoroutine(StopScrollCoroutine());
        }
    }
}
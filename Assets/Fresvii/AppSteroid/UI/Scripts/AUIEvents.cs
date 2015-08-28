using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIEvents : MonoBehaviour
    {
        public AUIScrollViewContents contents;

        [HideInInspector]
        public AUICommunityTop auiCommunityTop;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIFrame frame;

        public Text title;

        //public Text backButtonText;

        public AUISegmentedControl segmentedControl;

        public GameObject nodeOngoing, nodeUpcoming, nodeArchive;

        void OnEnable()
        {            
            AUIManager.OnEscapeTapped += Back;

            segmentedControl.OnChanged += OnSegmentedControlChanged;
        }

        void OnDisable()
        {
            segmentedControl.OnChanged -= OnSegmentedControlChanged;

            AUIManager.OnEscapeTapped -= Back;
        }

        void Start()
        {
            title.text = frame.title = FASConfig.Instance.appName + " " + FASText.Get("GameEvents");
        }

        public void Back()
        {
            if (frame.Animating) return;

            auiCommunityTop.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            auiCommunityTop.GetComponent<AUIFrame>().Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        void OnSegmentedControlChanged(int index)
        {
            nodeOngoing.gameObject.SetActive(index == 0);

            nodeUpcoming.gameObject.SetActive(index == 1);

            nodeArchive.gameObject.SetActive(index == 2);
        }

        public GameObject prfbAUIGameEvent;

        public void GoToGameEvent(Fresvii.AppSteroid.Models.GameEvent gameEvent)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject goGameEvent = Instantiate(prfbAUIGameEvent) as GameObject;

            AUIGameEvent auiGameEvent = goGameEvent.GetComponent<AUIGameEvent>();

            auiGameEvent.GameEvent = gameEvent;

            goGameEvent.transform.SetParent(transform.parent, false);

            goGameEvent.gameObject.SetActive(true);

            goGameEvent.transform.SetAsLastSibling();

            AUIFrame nextFrame = auiGameEvent.frame;

            nextFrame.backFrame = this.frame;

            nextFrame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }
    }
}
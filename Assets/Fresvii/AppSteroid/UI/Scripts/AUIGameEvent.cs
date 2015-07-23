using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGameEvent : MonoBehaviour
    {
        public AUIFrame frame;

        public Text title;

        //public Text backButtonText;

        public Fresvii.AppSteroid.Models.GameEvent GameEvent { get; set;}

        public GameObject prfbAUIEventboardListCell;

        public GameObject prfbEventboard;

        private Fresvii.AppSteroid.Models.ListMeta eventboardListMeta;

        private List<AUIGameEventboardCell> eventboardCells = new List<AUIGameEventboardCell>();

        public AUIScrollViewContents eventboardContents;

        public RectTransform contents;

        bool initialized = false;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            if (!initialized)
                StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;
        }

        IEnumerator Init()
        {
            title.text = frame.title = "";
            //backButtonText.text = frame.backFrame.title;

            eventBoardPanel.SetActive(false);

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (GameEvent == null)
            {
                yield return 1;
            }

            contents.gameObject.SetActive(false);

            title.text = frame.title = GameEvent.Name;

            while (frame.Animating)
            {
                yield return 1;
            }

            if (string.IsNullOrEmpty(GameEvent.ImageLargeUrl) || string.IsNullOrEmpty(GameEvent.Description) || string.IsNullOrEmpty(GameEvent.Name))
            {
                FASLeaderboard.GetEvent(GameEvent.Id, (ge, error) =>
                {
                    if (error == null)
                    {
                        contents.gameObject.SetActive(true);

                        this.GameEvent = ge;

                        SetGameEvent();

                        SetLayout();
                    }
                    else
                    {
                        Debug.LogError(error.ToString());

                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                        Back();
                    }
                });
            }
            else 
            {
                contents.gameObject.SetActive(true);

                SetGameEvent();

                SetLayout();
            }

            FASLeaderboard.GetEventboardList(GameEvent.Id, OnGetEventboardList);

            FASUtility.SendPageView("pv.community.events.show", this.GameEvent.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });
        }

        public Text textGameEventName;

        public Text duration;
        
        public Text description;

        public AUIRawImageTextureSetter clipImage;

        public void SetGameEvent()
        {
            textGameEventName.text = GameEvent.Name;

            description.text = GameEvent.Description;

            duration.text = GameEvent.StartAt.ToShortDateString() + " - " + ((GameEvent.EndAt != System.DateTime.MaxValue) ? GameEvent.EndAt.ToShortDateString() : "");

            clipImage.Set(GameEvent.ImageLargeUrl);
        }

        void OnScreenSizeChanged()
        {
            SetLayout();
        }

        public float margin;

        public RectTransform eventInfoRectTransform;

        public RectTransform bottomButtonRectTransform;

        public RectTransform eventboardsRectTransfrom;

        void SetLayout()
        {
            bottomButtonRectTransform.anchoredPosition = new Vector2(0f, description.rectTransform.anchoredPosition.y - description.preferredHeight - margin);

            eventInfoRectTransform.sizeDelta = new Vector2(eventInfoRectTransform.sizeDelta.x, -bottomButtonRectTransform.anchoredPosition.y + bottomButtonRectTransform.sizeDelta.y);

            eventboardsRectTransfrom.anchoredPosition = new Vector2(eventboardsRectTransfrom.anchoredPosition.x, -eventInfoRectTransform.sizeDelta.y);

            contents.sizeDelta = new Vector2(contents.sizeDelta.x, -eventboardsRectTransfrom.anchoredPosition.y + eventboardsRectTransfrom.sizeDelta.y);
        }

        public GameObject eventBoardPanel;

        void OnGetEventboardList(IList<Fresvii.AppSteroid.Models.Eventboard> eventboards, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            initialized = true;

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

            if (this.eventboardListMeta == null || this.eventboardListMeta.CurrentPage != 0)
            {
                this.eventboardListMeta = meta;
            }

            if (meta.TotalCount > 0)
            {
                eventBoardPanel.SetActive(true);
            }

            foreach (var eventboard in eventboards)
            {
                var cell = eventboardCells.Find(x => x.Eventboard.Id == eventboard.Id);

                if (cell != null)
                {
                    cell.SetEventboard(eventboard, this);

                    return;
                }

                var item = ((GameObject)Instantiate(prfbAUIEventboardListCell)).GetComponent<RectTransform>();

                eventboardContents.AddItem(item);

                cell = item.GetComponent<AUIGameEventboardCell>();

                cell.SetEventboard(eventboard, this);

                eventboardCells.Add(cell);
            }
        }

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

        public void GoToEventboard(Fresvii.AppSteroid.Models.Eventboard eventboard)
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiEventboard = ((GameObject)Instantiate(prfbEventboard)).GetComponent<AUIEventboard>();

            auiEventboard.SetEventboard(eventboard);

            auiEventboard.transform.SetParent(transform.parent, false);

            auiEventboard.transform.SetAsLastSibling();

            auiEventboard.parentFrameTween = this.frame;

            //auiEventboard.backButtonText.text = title.text;

            auiEventboard.frame.backFrame = this.frame;

            auiEventboard.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }


        public void ShareTwitter()
        {
            Debug.Log("ShareTwitter");

            string text = FASText.Get("GameEventShare").Replace("%gametitle", FASConfig.Instance.appName);

            Fresvii.AppSteroid.Util.SocialNetworkingService.ShareTwitterWithUI(text, this.GameEvent.WebSiteUrl, (result) =>
            {
                if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                {
                    Debug.LogError("FASSNS.ShareTwitter : error");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("TwitterError"), (del) => { });
                }
            });

        }

        public void ShareFacebook()
        {
            Debug.Log("ShareFacebook");

            string text = FASText.Get("GameEventShare").Replace("%gametitle", FASConfig.Instance.appName);

            Fresvii.AppSteroid.Util.SocialNetworkingService.ShareFacebook(text, this.GameEvent.WebSiteUrl, (result) =>
            {
                if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                {
                    Debug.LogError("FASSNS.ShareFacebook : error");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("FacebookError"), (del) => { });
                }
            });
        }
    }
}
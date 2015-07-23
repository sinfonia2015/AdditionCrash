using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIDirectMessages : MonoBehaviour
    {
        public AUIFrame frameTween;

        public Text title;

        //public Text backButtonText;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public AUIScrollViewPullReflesh pullReflesh;

        private bool pullRefleshing;

        public AUIScrollViewContents contents;

        public AUIScrollRect scrollView;

        public RectTransform prfbDirectMessageCell;

        public Fresvii.AppSteroid.Models.ListMeta listMeta;

        private List<AUIDirectMessageCell> cells = new List<AUIDirectMessageCell>();

        public IList<Fresvii.AppSteroid.Models.DirectMessage> directMessages;

        public float pollingDuration = 30f;

        private bool isPullDown;

        void OnEnable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            FASEvent.OnDirectMessageCreated += UpdateDirectMessage;

            AUIManager.OnEscapeTapped += BackPage;

            AUITabBar.Instance.DirectMessageRead();

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            FASEvent.OnDirectMessageCreated -= UpdateDirectMessage;

            AUIManager.OnEscapeTapped -= BackPage;

            AUITabBar.Instance.DirectMessageRead();

            FASDirectMessage.MarkAsReadAllDirectMessages((error) => { });
        }

        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASDirectMessage.GetDirectMessageList(1, false, OnGetDirectMessageList);

            FASDirectMessage.MarkAsReadAllDirectMessages((error) => { });
        }

        void OnPullDownReflesh()
        {
            if (pullRefleshing) return;

            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                pullRefleshing = true;

                isPullDown = true;

                if (listMeta.NextPage.HasValue)
                {
                    FASDirectMessage.GetDirectMessageList((uint)listMeta.NextPage, false, OnGetDirectMessageList);
                }
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        void OnPullUpReflesh()
        {
            if (pullRefleshing) return;

            pullRefleshing = true;

            FASDirectMessage.GetDirectMessageList(0, true, OnGetDirectMessageList);
        }

        public float newMessageTweenDuration = 0.25f;

        public void RemoveCell(AUIDirectMessageCell cell)
        {
            cells.Remove(cell);

            contents.RemoveItem(cell.GetComponent<RectTransform>());

            Destroy(cell.gameObject);
        }

        void GetDirectMessageList()
        {          
            if (!FASUser.IsLoggedIn())
            {
                return;
            }

            if (GetComponent<AUIFrame>().Animating)
            {
                return;
            }

            FASDirectMessage.GetDirectMessageList(1, true, OnGetDirectMessageList);
        }

        void OnGetDirectMessageList(IList<Fresvii.AppSteroid.Models.DirectMessage> directMessages, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (pullRefleshing)
            {
                pullReflesh.PullRefleshCompleted();

                pullRefleshing = false;
            }

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

            if (this.listMeta == null || this.listMeta.CurrentPage != 0)
            {
                this.listMeta = meta;
            }

            foreach (Fresvii.AppSteroid.Models.DirectMessage directMessage in directMessages)
            {
                UpdateDirectMessage(directMessage);                
            }

            if (!initialized)
            {
                initialized = true;

                scrollView.GoToBottom(0f);
            }

            if (isPullDown)
            {
                isPullDown = false;

                scrollView.Pinned();
            }

            cells.Sort((a, b) => System.DateTime.Compare(a.DirectMessage.CreatedAt, b.DirectMessage.CreatedAt));

            System.DateTime dt = System.DateTime.MinValue;

            foreach (var obj in cells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);

                bool hasTimeLine = (dt.Year != obj.DirectMessage.CreatedAt.Year || dt.Month != obj.DirectMessage.CreatedAt.Month || dt.Day != obj.DirectMessage.CreatedAt.Day);

                obj.SetTimeLine(hasTimeLine);

                dt = obj.DirectMessage.CreatedAt;

            }

            contents.ReLayout();
        }
        
        bool initialized;

       
        private void UpdateDirectMessage(Fresvii.AppSteroid.Models.DirectMessage directMessage)
        {
            AUIDirectMessageCell cell = cells.Find(x => x.DirectMessage.Id == directMessage.Id);

            if (cell != null)
            {
                cell.SetDirectMessage(directMessage);

                return;
            }
            else
            {
                var item = GameObject.Instantiate(prfbDirectMessageCell) as RectTransform;

                contents.AddItem(item);

                cell = item.GetComponent<AUIDirectMessageCell>();

                cell.SetDirectMessage(directMessage);

                cells.Add(cell);

                cell.gameObject.SetActive(false);

                return;
            }
        }

        public void BackPage()
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
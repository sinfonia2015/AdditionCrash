using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIDirectMessageCell : MonoBehaviour
    {
        [HideInInspector]
        public AppSteroid.Models.DirectMessage DirectMessage;

        [HideInInspector]
        public AUIMessages auiMessages;

        public RectTransform prfbDateTimeLine;

        protected bool hasTimeLine;

        protected GameObject dateTimeLine;

        public Image balloonBg;

        public Text comment;

        protected RectTransform rectTransform;

        public float margin;

        public float maxBalloonWidth = 500f;

        public AUIRawImageTextureSetter icon;

        public Vector2 balloonReferenceSize = new Vector2(500f, 300f);

        public Vector2 clipImageReferenceSize = new Vector2(460f, 260f);

        public void SetTimeLine(bool hasTimeLine)
        {
            if (this.hasTimeLine != hasTimeLine)
            {
                if (this.hasTimeLine)
                {
                    Destroy(dateTimeLine);
                }
                else
                {
                    dateTimeLine = (Instantiate(prfbDateTimeLine) as RectTransform).gameObject;

                    dateTimeLine.transform.SetParent(this.transform, false);

                    dateTimeLine.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    dateTimeLine.GetComponent<AUIMessageDateTimeLine>().SetDateTime(DirectMessage.CreatedAt);
                }

                this.hasTimeLine = hasTimeLine;

                SetLayout();
            }
        }

        void SetLayout()
        {
            RectTransform rtBalloonBg = balloonBg.gameObject.GetComponent<RectTransform>();

            float w = balloonReferenceSize.x * Mathf.Min(AUIManager.Instance.auiCanvasScaleManager.scale, 1.0f);

            rtBalloonBg.sizeDelta = new Vector2(w, rtBalloonBg.sizeDelta.y);

            comment.rectTransform.sizeDelta = new Vector2(-40f, comment.rectTransform.sizeDelta.y);

            rtBalloonBg.anchoredPosition = new Vector2(rtBalloonBg.anchoredPosition.x, ((hasTimeLine) ? -prfbDateTimeLine.sizeDelta.y - 40f : 0f));

            rtBalloonBg.sizeDelta = new Vector2(Mathf.Min(w, comment.preferredWidth + 40f), comment.preferredHeight + 40f);

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, -rtBalloonBg.anchoredPosition.y + rtBalloonBg.sizeDelta.y + margin);
        }

        public AUIRawImageTextureSetter userIcon;

        public Text subject;

        private RectTransform userIconRectTransform;

        private bool settle;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

            StartCoroutine(UpdateUpdatedAt());

            if(settle)
                StartCoroutine(ResetLayout());
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        public void SetDirectMessage(AppSteroid.Models.DirectMessage directMessage)
        {
            this.DirectMessage = directMessage;

            subject.text = directMessage.Subject;

            if (!string.IsNullOrEmpty(directMessage.Text))
            {
                comment.text = directMessage.Text;
            }
            else
            {
                comment.text = "";
            }

            comment.gameObject.SetActive(true);

            icon.Set(FAS.OfficialUser.ProfileImageUrl);

            SetLayout();

            settle = true;
        }

        void OnScreenSizeChanged()
        {
            StartCoroutine(ResetLayout());
        }

        IEnumerator ResetLayout()
        {
            yield return new WaitForEndOfFrame();

            SetDirectMessage(this.DirectMessage);
        }

        IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (DirectMessage != null)
                {
                    subject.text = DirectMessage.Subject + "・" + AUIUtility.CurrentTimeSpan(DirectMessage.CreatedAt);

                    yield return new WaitForSeconds(60f);
                }
                else
                {
                    yield return 1;
                }
            }
        }
    }
}

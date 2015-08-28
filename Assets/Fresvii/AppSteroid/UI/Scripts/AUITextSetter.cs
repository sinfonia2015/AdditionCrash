using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent(typeof(Text))]
    public class AUITextSetter : MonoBehaviour
    {
        public enum TruncateType { None, Width, Height };

        public FASText.FontStyle fontStyle;

        private Text text;

        public string key;

        public string keyForAndroid;

        public TruncateType truncate;

        public string truncatedReplacement = "...";

        private string originalText;

        public float truncateHeight;

        private string settleText;

        public bool dontFontUp;

        // Use this for initialization
        void Awake()
        {
            text = GetComponent<Text>();

            if (!string.IsNullOrEmpty(keyForAndroid) && Application.platform == RuntimePlatform.Android)
            {
                text.text = FASText.Get(keyForAndroid);
            }
            else if (!string.IsNullOrEmpty(key))
            {
                text.text = FASText.Get(key);
            }

            //text.font = GetFontType();
        }

        void Start()
        {
            text.font = GetFontType();
        }

        Font GetFontType()
        {
            if (fontStyle == FASText.FontStyle.Light)
            {
                if(!AUIManager.Instance.fontUp || dontFontUp)
                    return Fresvii.AppSteroid.FASSettings.Settings.lightFont;
                else
                    return Fresvii.AppSteroid.FASSettings.Settings.regularFont;
            }
            else if (fontStyle == FASText.FontStyle.Regular)
            {
                if (!AUIManager.Instance.fontUp || dontFontUp)
                    return Fresvii.AppSteroid.FASSettings.Settings.regularFont;
                else
                    return Fresvii.AppSteroid.FASSettings.Settings.semiboldFont;
            }
            else if (fontStyle == FASText.FontStyle.Semibold)
            {
                return Fresvii.AppSteroid.FASSettings.Settings.semiboldFont;
            }
            else if (fontStyle == FASText.FontStyle.Bold)
            {
                return Fresvii.AppSteroid.FASSettings.Settings.boldFont;
            }
            else if (fontStyle == FASText.FontStyle.Extrabold)
            {
                return Fresvii.AppSteroid.FASSettings.Settings.extraboldFont;
            }
            else 
            {
                return Fresvii.AppSteroid.FASSettings.Settings.regularFont;
            }
        }

        bool forceTruncate = false;

        public void Truncate()
        {
            forceTruncate = true;
        }

        public void TruncateImediately()
        {
            Truncate(originalText);
        }

        void LateUpdate()
        {
            if (truncate != AUITextSetter.TruncateType.None)
            {
				if(!string.IsNullOrEmpty(originalText) && !string.IsNullOrEmpty(settleText) && text.text == originalText)
				{
					text.text = settleText;			
				}
                else if (settleText != text.text && !string.IsNullOrEmpty(text.text) && this.gameObject.activeInHierarchy)
                {
		           	StartCoroutine(DelayTruncate());
                }
                else if (forceTruncate)
                {
                    forceTruncate = false;

                    Truncate(originalText);
                }
            }
        }

        IEnumerator DelayTruncate()
        {
            yield return new WaitForEndOfFrame();

            Truncate(text.text);
        }

        private void Truncate(string originalText)
        {
            if (string.IsNullOrEmpty(originalText) || originalText.Length + 2 < truncatedReplacement.Length)
            {
                settleText = text.text;

                this.originalText = originalText;

                return;
            }

            this.originalText = originalText;
            
            int truncatedLength = truncatedReplacement.Length;

            if (truncate == TruncateType.Width)
            {
                if (text.rectTransform.rect.width == 0f) return;

                text.text = originalText;

                while (text.preferredWidth > text.rectTransform.rect.width)
                {
                    truncatedLength++;

                    if (originalText.Length - truncatedLength <= 0)
                    {
                        break;
                    }

                    text.text = originalText.Substring(0, originalText.Length - truncatedLength) + truncatedReplacement;
                }
            }
            else if (truncate == TruncateType.Height)
            {
                text.text = originalText;

                while (text.preferredHeight > truncateHeight)
                {
                    truncatedLength++;

                    if (originalText.Length - truncatedLength <= 0)
                    {
                        break;
                    }

                    text.text = originalText.Substring(0, originalText.Length - truncatedLength) + truncatedReplacement;
                }
            }

            settleText = text.text;
        }

        private void OnScreenSizeChanged()
        {
            if(this.gameObject.activeInHierarchy)
                StartCoroutine(TruncateDelay());
        }

        IEnumerator TruncateDelay()
        {
            forceTruncate = true;

            yield return new WaitForEndOfFrame();

            Truncate(originalText);
        }

        void OnEnable()
        {
            if (truncate != AUITextSetter.TruncateType.None)
                AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;
        }

        void OnDisable()
        {
            if (truncate != AUITextSetter.TruncateType.None) 
                AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }
    }
}

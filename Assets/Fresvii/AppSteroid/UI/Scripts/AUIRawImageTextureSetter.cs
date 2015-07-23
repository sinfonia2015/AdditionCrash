using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Fresvii.AppSteroid.Util;

namespace Fresvii.AppSteroid.UI
{
    [RequireComponent (typeof(RawImage))]
    public class AUIRawImageTextureSetter : MonoBehaviour
    {
        public bool dontDestroy = false;

        public bool autoRelease = true;

        private Texture2D texture;

        private string url;

        public string Url { get { return url; } }

        private string loadedUrl;

        public bool clop = true;

        public float tweenTime = 0.25f;

        bool loading = false;

        public bool resize = false;

        public Vector2 shrinkSize;

        System.Action<AUIRawImageTextureSetter> OnLoaded;

        public void SetImmediately(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            this.url = url;

            LoadImage();
        }

        public void Set(string url)
        {
            if (string.IsNullOrEmpty(url) || this.url == url)
            {
                return;
            }

            this.url = url;
        }

        public void Set(string url, System.Action<AUIRawImageTextureSetter> callback)
        {
            if (string.IsNullOrEmpty(url) || this.url == url)
            {
                return;
            }

            this.OnLoaded = callback;

            this.url = url;
        }

        public int delayCount = 0;

        void LateUpdate()
        {
            if (!string.IsNullOrEmpty(url) && texture == null && !loading)
            {
                if (--delayCount < 0)
                {
                    LoadImage();
                }
            }
            else if (url != loadedUrl && !loading)
            {
                if (--delayCount < 0)
                {
                    if (texture != null)
                    {
                        ResourceManager.Instance.ReleaseTexture(loadedUrl);
                    }

                    LoadImage();
                }
            }
        }

        void LoadImage()
        {
            loading = true;

            if (!resize)
            {
                ResourceManager.Instance.TextureFromCacheOrDownloadOrMemory(url, autoRelease, (tex) =>
                {
                    loading = false;

                    this.texture = tex;

                    if (this.texture != null)
                    {
                        loadedUrl = url;

                        SetTexture(this.texture);
                    }

                    if (OnLoaded != null)
                        OnLoaded(this);
                });
            }
            else
            {
                ResourceManager.Instance.TextureFromCacheOrDownloadOrMemory(url, autoRelease, (int)shrinkSize.x, (int)shrinkSize.y, (tex) =>
                {
                    loading = false;

                    this.texture = tex;

                    if (this.texture != null)
                    {
                        loadedUrl = url;

                        SetTexture(this.texture);
                    }

                    if (OnLoaded != null)
                        OnLoaded(this);
                });
            }
        }

        public void SetTexture(Texture2D texture)
        {
            if (this == null) return;

            RawImage image = GetComponent<RawImage>();

            this.texture = texture;

            image.texture = texture;

            if (texture == null)
            {
                return;
            }

            if (clop)
            {
                Clop(image);
            }

            image.color = Color.white;
        }

        private void Clop(RawImage image)
        {
            if (image == null || texture == null) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (texture.width / texture.height < rectTransform.sizeDelta.x / rectTransform.sizeDelta.y)
            {
                float dy = texture.width * rectTransform.sizeDelta.y / rectTransform.sizeDelta.x;

                Vector2 offset = new Vector2(0f, (texture.height - dy) / texture.height * 0.5f);

                Vector2 scale = new Vector2(1.0f, 1.0f - offset.y * 2f);

                image.uvRect = new Rect(offset.x, offset.y, scale.x, scale.y);
            }
            else
            {
                float dx = texture.height * rectTransform.sizeDelta.x / rectTransform.sizeDelta.y;

                Vector2 offset = new Vector2((texture.width - dx) / texture.width * 0.5f, 0f);

                Vector2 scale = new Vector2(1.0f - offset.x * 2f, 1.0f);

                image.uvRect = new Rect(offset.x, offset.y, scale.x, scale.y);
            }
        }

        public void ReleaseTexture()
        {
            RawImage image = GetComponent<RawImage>();

            image.texture = null;

            if (texture != null)
            {
                if (!string.IsNullOrEmpty(loadedUrl))
                {
                    ResourceManager.Instance.ReleaseTexture(loadedUrl);

                    loadedUrl = url = "";
                }
            }

            texture = null;
        }

        public Texture2D GetTexture()
        {

            return texture;
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;

            loading = false;
        }

        void OnScreenSizeChanged()
        {
            if (clop)
            {
                Clop(GetComponent<RawImage>());
            }
        }

        void OnDestroy()
        {
            if (texture != null)
            {
                if (!ResourceManager.IsQuitting && !dontDestroy)
                    ResourceManager.Instance.ReleaseTexture(url);
            }
        }
    }
}
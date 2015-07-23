using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIInputComment : MonoBehaviour
    {
        public AUIRawImageTextureSetter clipImage;

        public InputField commentInputFiled;

        private Fresvii.AppSteroid.Models.Video video = null;

        public Button buttonDone;

        public event Action<string, Texture2D, Fresvii.AppSteroid.Models.Video> OnInputDone;

        public event Action<float> OnHeightChanged;

        public float normalHeight, withTextureHeight;

        public RectTransform rectTransform;

        private bool enable = true;

        public event Action OnChooseMovie;

        public void OnClickSelectItem()
        {
            List<string> buttons = new List<string>();

            buttons.Add(FASText.Get("ChoosePhoto"));

            buttons.Add(FASText.Get("TakePhoto"));

            if (OnChooseMovie != null)
                buttons.Add(FASText.Get("ChooseMovie"));

            AUIActionSheet.Show(buttons.ToArray(), (button) =>
            {
                if (button == FASText.Get("ChoosePhoto"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Gallery, (texture) =>
                    {
                        if (texture != null)
                        {
                            clipImage.ReleaseTexture();

                            clipImage.SetTexture(texture);
                        }

                        Validate();
                    });
                }
                else if (button == FASText.Get("TakePhoto"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Camera, (texture) =>
                    {
                        if (texture != null)
                        {
                            clipImage.ReleaseTexture();

                            clipImage.SetTexture(texture);
                        }

                        Validate();
                    });
                }
                else if (button == FASText.Get("ChooseMovie"))
                {
                    if (OnChooseMovie != null)
                    {
                        OnChooseMovie();
                    }
                }

            });
        }

        public void SetVideo(Fresvii.AppSteroid.Models.Video video, Texture2D videoThumbnail)
        {
            this.video = video;

            clipImage.SetTexture(videoThumbnail);

            Validate();
        }

        public void OnEndEdit(string text)
        {
            Validate();
        }

        public void OnClickDone()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            if (OnInputDone != null)
            {
                OnInputDone(commentInputFiled.text, clipImage.GetTexture(), video);
            }

            Clear();
        }

        void OnEnable()
        {
            Validate();
        }

        private void Validate()
        {
            if (!string.IsNullOrEmpty(commentInputFiled.text) || clipImage.GetTexture() != null || video != null)
            {
                buttonDone.interactable = enable;
            }
            else
            {
                buttonDone.interactable = false;
            }

            clipImage.gameObject.SetActive(clipImage.GetTexture() != null);

            float height = rectTransform.sizeDelta.y;

            if (clipImage.gameObject.activeSelf)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, withTextureHeight);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, normalHeight);
            }

            if (height != rectTransform.sizeDelta.y)
            {
                if (OnHeightChanged != null)
                {
                    OnHeightChanged(rectTransform.sizeDelta.y);
                }
            }
        }

        public void Clear()
        {
            commentInputFiled.text = "";

            clipImage.SetTexture(null);

            Validate();
        }

        public void SetEnable(bool on)
        {
            enable = on;

            Validate();
        }

        public void ClearClipImage()
        {
            clipImage.ReleaseTexture();

            Validate();
        }
    }
}
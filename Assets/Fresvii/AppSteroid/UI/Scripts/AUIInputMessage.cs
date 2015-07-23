using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIInputMessage : MonoBehaviour
    {
        public AUIFrame frameTween;

        public InputField inputFiled;

        public Button buttonDone;

        public event Action<string> OnInputTextDone;

        public event Action<Texture2D> OnTextureSelected;

        public event Action<Fresvii.AppSteroid.Models.Video, Texture2D> OnVideoSelected;

        public event Action<Fresvii.AppSteroid.Models.Sticker> OnStickerSelected;

        public event Action<float> OnHeightChanged;

        public RectTransform rtInputField, rtStickerPicker;

        public AUIStickerPicker stickerPicker;

        public GameObject prfbVideoList;

        public Button buttonBlocker;

        public event System.Action OnBlockTapped;

        public void OnClickSelectText()
        {
            inputFiled.gameObject.SetActive(true);

            buttonDone.gameObject.SetActive(true);

            stickerPicker.gameObject.SetActive(false);

            inputFiled.Select();

            inputFiled.ActivateInputField();

            if (OnHeightChanged != null)
            {
                OnHeightChanged(rtInputField.anchoredPosition.y + rtInputField.sizeDelta.y);
            }
        }

        public void OnClickSelectText(bool setInput)
        {
            inputFiled.gameObject.SetActive(true);

            buttonDone.gameObject.SetActive(true);

            stickerPicker.gameObject.SetActive(false);

            if (setInput)
            {
                inputFiled.Select();

                inputFiled.ActivateInputField();
            }

            if (OnHeightChanged != null)
            {
                OnHeightChanged(rtInputField.anchoredPosition.y + rtInputField.sizeDelta.y);
            }
        }

        public void OnClickSelectSticker()
        {
            inputFiled.gameObject.SetActive(false);

            buttonDone.gameObject.SetActive(false);

            stickerPicker.gameObject.SetActive(true);

            if (OnHeightChanged != null)
            {
                OnHeightChanged(rtStickerPicker.anchoredPosition.y + rtStickerPicker.sizeDelta.y);
            }
        }

        public void OnClickSelectCamera()
        {   
            Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Camera, (texture) =>
            {
                if (texture != null)
                {
                    if (OnTextureSelected != null)
                    {
                        OnTextureSelected(texture);
                    }
                }
            });
        }

        public void OnClickSelectImage()
        {           
            Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Gallery, (texture) =>
            {
                if (texture != null)
                {
                    if (OnTextureSelected != null)
                    {
                        OnTextureSelected(texture);
                    }
                }
            });
        }

        public void OnClickSelectVideo()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbVideoList) as GameObject;

            go.GetComponent<RectTransform>().SetParent(frameTween.transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIVideoList videoList = go.GetComponent<AUIVideoList>();

            videoList.IsModal = true;

            videoList.backButtonText.text = FASText.Get("Cancel");

            videoList.parentFrameTween = this.frameTween;

            videoList.mode = AUIVideoList.Mode.Select;

            videoList.frameTween.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () =>
            {
                frameTween.gameObject.SetActive(false);
            });

            videoList.OnVideoSelected += (video, thumbnail) =>
            {
                if (video != null && OnVideoSelected != null)
                {
                    OnVideoSelected(video, thumbnail);
                }
            };
        }

        public void OnEndEdit(string text)
        {
             Validate();
        }

        public void OnClickDone()
        {
            if (OnInputTextDone != null)
            {
                OnInputTextDone(inputFiled.text);
            }
        }

        void OnEnable()
        {
            Validate();

            stickerPicker.OnStickerSelected += StickerSelected;
        }

        void OnDisable()
        {
            stickerPicker.OnStickerSelected -= StickerSelected;
        }

        void StickerSelected(Fresvii.AppSteroid.Models.Sticker sticker)
        {
            if (this.OnStickerSelected != null)
            {
                this.OnStickerSelected(sticker);
            }
        }

        private void Validate()
        {
            if (!string.IsNullOrEmpty(inputFiled.text))
            {
                buttonDone.interactable = true;
            }
            else
            {
                buttonDone.interactable = false;
            }
        }

        public void Clear()
        {
            inputFiled.text = "";

            Validate();
        }

        public void OnTapBlockButton()
        {
            if (OnBlockTapped != null)
            {
                OnBlockTapped();
            }
        }

        public void SetBlock(bool on)
        {
            buttonBlocker.gameObject.SetActive(on);
        }
    }
}
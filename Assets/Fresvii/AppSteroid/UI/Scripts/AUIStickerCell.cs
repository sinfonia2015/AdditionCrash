using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIStickerCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Sticker Sticker { get; protected set; }

        public AUIRawImageTextureSetter image;

        private AUIStickerSelector stickerSelector;

        public void SetSticker(Fresvii.AppSteroid.Models.Sticker sticker, AUIStickerSelector stickerSelector)
        {
            this.Sticker = sticker;

            this.stickerSelector = stickerSelector;

            image.Set(sticker.Url);
        }

        public void OnClicked()
        {
            this.stickerSelector.OnStickerSelected(this.Sticker);
        }
    }
}

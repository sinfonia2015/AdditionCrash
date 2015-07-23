using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIStickerPicker : MonoBehaviour
    {
        public AUIStickerSetSelector stickerSetSelector;

        public event Action<Fresvii.AppSteroid.Models.Sticker> OnStickerSelected;

        public void StickerSelected(Fresvii.AppSteroid.Models.Sticker sticker)
        {
            if (OnStickerSelected != null)
            {
                OnStickerSelected(sticker);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIStickerSetCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.StickerSet StickerSet { get; protected set; }

        private AUIStickerSetSelector selector;

        public AUIRawImageTextureSetter image;

        public GameObject prfbAUIStickerSelector;

        private AUIStickerSelector stickerSelector;

        public void SetStickerSet(Fresvii.AppSteroid.Models.StickerSet stickerSet, AUIStickerSetSelector selector)
        {
            this.StickerSet = stickerSet;

            this.selector = selector;

            image.Set(this.StickerSet.Url);
        }

        public void Show()
        {
            if (stickerSelector == null)
            {
                GameObject go = Instantiate(prfbAUIStickerSelector) as GameObject;

                stickerSelector = go.GetComponent<AUIStickerSelector>();

                go.transform.SetParent(this.selector.transform.parent, false);

                stickerSelector.SetStickerSet(this.StickerSet, this.selector);

                go.transform.SetAsLastSibling();
            }

            stickerSelector.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (stickerSelector != null)
            {
                stickerSelector.gameObject.SetActive(false);
            }
        }

        public void OnClick()
        {
            selector.StickerSetSelected(this);
        }
    }
}

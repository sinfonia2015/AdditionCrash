using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIStickerSelector : MonoBehaviour
    {
        public GameObject prfbStickerCell;

        public Transform contents;

        private Fresvii.AppSteroid.Models.StickerSet stickerSet { get; set; }

        private List<AUIStickerCell> cells = new List<AUIStickerCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private AUIStickerSetSelector stickerSetSelector;

        private bool initialized;

        public void SetStickerSet(Fresvii.AppSteroid.Models.StickerSet stickerSet, AUIStickerSetSelector stickerSetSelector)
        {
            this.stickerSet = stickerSet;

            this.stickerSetSelector = stickerSetSelector;
        }

        void OnEnable()
        {
            if (!initialized)
            {
                StartCoroutine(Init());
            }
        }

        IEnumerator Init()
        {
            while (stickerSet == null)
            {
                yield return 1;
            }

            if (stickerSet.Stickers == null || stickerSet.Stickers.Count == 0)
            {
                FASSticker.GetStickerSet(stickerSet.Id, (_stickerSet, error) =>
                {
                    if (error == null)
                    {
                        this.stickerSet = _stickerSet;

                        SetCells();

                        initialized = true;
                    }
                    else
                    {
                        StartCoroutine(Init());
                    }
                });                
            }
            else
            {
                SetCells();

                initialized = true;
            }
        }

        void SetCells()
        {
            foreach (var sticker in this.stickerSet.Stickers)
            {
                var cell = cells.Find(x => x.Sticker.Id == sticker.Id);

                if (cell != null) continue;

                GameObject go = Instantiate(prfbStickerCell) as GameObject;

                var stickerCell = go.GetComponent<AUIStickerCell>();

                stickerCell.SetSticker(sticker, this);

                var item = go.GetComponent<RectTransform>();

                item.SetParent(contents, false);

                cells.Add(stickerCell);
            }
        }

        public void OnStickerSelected(Fresvii.AppSteroid.Models.Sticker sticker)
        {
            stickerSetSelector.stickerPicker.StickerSelected(sticker);
        }
    }
}

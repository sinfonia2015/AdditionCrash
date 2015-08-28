using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIStickerSetSelector : MonoBehaviour
    {
        public AUIStickerPicker stickerPicker;

        public GameObject prfbStickerSetCell;

        public AUIScrollViewContents contents;

        private List<AUIStickerSetCell> cells = new List<AUIStickerSetCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        public AUIScrollViewPullRefleshHorizontal pullReflesh;
        
        IEnumerator Start()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASSticker.GetStickerSetList(1, OnGetStickerSetList);
        }

        void OnEnable()
        {
            pullReflesh.OnPullLeftReflesh += OnPullLeftReflesh;
        }

        void OnDisable()
        {
            pullReflesh.OnPullLeftReflesh -= OnPullLeftReflesh;
        }

        void OnPullLeftReflesh()
        {
            if (this.listMeta != null && this.listMeta.NextPage.HasValue)
            {
                FASSticker.GetStickerSetList((uint)this.listMeta.NextPage, OnGetStickerSetList);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        void OnGetStickerSetList(IList<Fresvii.AppSteroid.Models.StickerSet> stickerSets, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null)
            {
                return;
            }

            if (this.enabled == false)
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

            foreach (var stickerSet in stickerSets)
            {
                UpdateStickerSet(stickerSet);
            }

            pullReflesh.PullRefleshCompleted();
        }

        private bool UpdateStickerSet(Fresvii.AppSteroid.Models.StickerSet stickerSet)
        {
            var cell = cells.Find(x => x.StickerSet.Id == stickerSet.Id);

            if (cell != null)
            {
                cell.SetStickerSet(stickerSet, this);

                return false;
            }

            GameObject go = Instantiate(prfbStickerSetCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUIStickerSetCell>();

            cell.SetStickerSet(stickerSet, this);

            cells.Add(cell);

            cell.gameObject.SetActive(false);

            if (cells.Count == 1)
            {
                cell.Show();
            }

            return true;
        }

        public void StickerSetSelected(AUIStickerSetCell selectedCell)
        {
            foreach (var cell in cells)
            {
                if (cell == selectedCell)
                {
                    cell.Show();
                }
                else
                {
                    cell.Hide();
                }
            }
        }
    }
}

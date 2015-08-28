using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIUserSearch : MonoBehaviour
    {
        public AUIFrame frame;

        [HideInInspector]
        public AUIFrame parentFrame;

        public RectTransform prfbUserSearchCell;

        private List<AUIUserSearchCell> cells = new List<AUIUserSearchCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        public AUIScrollViewContents contents;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        private bool isPullRefleshProc;

        public InputField searchInputField;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                isPullRefleshProc = true;

                Search( (uint)listMeta.NextPage);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        public void OnClickSearch()
        {
            contents.Clear();

            foreach (var cell in cells)
            {
                Destroy(cell.gameObject);
            }

            cells.Clear();

            this.listMeta = null;

            Search(1);
        }

        void Search(uint page)
        {
            string query = "{\"where\":[{\"column\":\"name\",\"operator\":\"like\", \"value\":\"" + searchInputField.text + "%\"},{\"column\":\"user_code\",\"operator\":\"like\", \"value\":\"" + searchInputField.text + "%\"}],\"operation\": \"any\"}";

            FASUser.GetUserList("", "", query, page, OnGetUserList);
        }

        void OnGetUserList(IList<Fresvii.AppSteroid.Models.User> users, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null || this.enabled == false)
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
                if(meta != null)
                    this.listMeta = meta;
            }

            foreach (Fresvii.AppSteroid.Models.User user in users)
            {
                UpdateUser(user);
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }

            contents.ReLayout();
        }

        public bool UpdateUser(Fresvii.AppSteroid.Models.User user)
        {
            AUIUserSearchCell cell = cells.Find(x => x.User.Id == user.Id);

            if (cell != null)
            {
                cell.SetUser(user, this);

                return false;
            }

            var item = GameObject.Instantiate(prfbUserSearchCell) as RectTransform;

            contents.AddItem(item);

            cell = item.GetComponent<AUIUserSearchCell>();

            cell.SetUser(user, this);

            cells.Add(cell);

            cell.gameObject.SetActive(false);

            return true;
        }

        public GameObject prfbUserPage;

        public GameObject prfbMyPage;

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frame.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (user.Id == FAS.CurrentUser.Id)
            {
                AUIMyPage myPage = ((GameObject)Instantiate(prfbMyPage)).GetComponent<AUIMyPage>();

                myPage.transform.SetParent(transform.parent, false);

                myPage.transform.SetAsLastSibling();

                myPage.parentFrameTween = this.frame;

                myPage.backButtonText.text = FASText.Get("Search");

                myPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }
            else
            {
                AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

                userPage.transform.SetParent(transform.parent, false);

                userPage.Set(user, FASText.Get("Search"), this.frame);

                userPage.transform.SetAsLastSibling();

                userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public void Back()
        {
            parentFrame.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrame.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            frame.Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        public void Clear()
        {
            contents.Clear();

            foreach (var cell in cells)
            {
                Destroy(cell.gameObject);
            }

            cells.Clear();

            this.listMeta = null;

            searchInputField.text = "";
        }
    }
}
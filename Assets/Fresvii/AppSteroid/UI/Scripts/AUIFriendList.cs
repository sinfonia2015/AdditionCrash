using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFriendList : MonoBehaviour
    {
        public AUIFrame frame;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public GameObject prfbFriendCell;

        public GameObject prfbReqestedCell;

        public AUIScrollViewContents contents;

        public Fresvii.AppSteroid.Models.User User { get; set; }

        private List<AUIFriendListCell> friendCells = new List<AUIFriendListCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool pullRefleshing;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public GameObject prfbUserPage;

        public GameObject prfbMyPage;

        public Text title;

        public Text backButtonText;

        private GameObject requestCell;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += BackPage;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            ClearCells();

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= BackPage;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;
        }

        void ClearCells()
        {
            contents.Clear();

            if (requestCell != null)
            {
                Destroy(requestCell);

                requestCell = null;
            }

            foreach (var cell in friendCells)
            {
                Destroy(cell.gameObject);
            }

            friendCells.Clear();
        }

        void OnPullUpReflesh()
        {
            if (pullRefleshing) return;

            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                pullRefleshing = true;

                FASFriendship.GetAccountFriendList((uint) listMeta.NextPage, OnGetUserFriendList);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        void Awake()
        {
            contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;
        }

        // Use this for initialization
        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (GetComponent<AUIFrame>().Animating)
            {
                yield return 1;
            }

            while (User == null)
            {
                yield return 1;
            }

            FASFriendship.GetUserFriendList(User.Id, OnGetUserFriendList);

            if(User.Id == FAS.CurrentUser.Id)
            {
                FASFriendship.GetFriendshipRequestedUsersList(User.Id, (friends, meta, error) =>
                {
                    if (meta.TotalCount > 0)
                    {
                        requestCell = Instantiate(prfbReqestedCell) as GameObject;

                        var item = requestCell.GetComponent<RectTransform>();

                        contents.AddItem(item, 0);

                        var cell = item.GetComponent<AUIFriendRequestCell>();

                        cell.parentFrame = this;

                        cell.RequestCount = meta.TotalCount;

                        cell.gameObject.SetActive(false);

                        contents.ReLayout();
                    }
                });
            }

            FASUtility.SendPageView("pv.my_page.friends", this.User.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });    
        }

        void OnGetUserFriendList(IList<Fresvii.AppSteroid.Models.Friend> friends, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
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

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Friend friend in friends)
            {
                friend.FriendStatus = Models.User.FriendStatuses.Friend;

                added |= UpdateFriend(friend);
            }

            if (pullRefleshing)
            {
                pullReflesh.PullRefleshCompleted();

                pullRefleshing = false;
            }

            contents.ReLayout();
        }

        private bool UpdateFriend(Fresvii.AppSteroid.Models.Friend friend)
        {
            AUIFriendListCell cell = friendCells.Find(x => x.Friend.Id == friend.Id);

            if (cell != null)
            {
                cell.SetFriend(friend, this);

                return false;
            }

            GameObject go = Instantiate(prfbFriendCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUIFriendListCell>();

            cell.FriendList = this;

            cell.SetFriend(friend, this);

            friendCells.Add(cell);

            cell.gameObject.SetActive(false);

            return true;
        }

        public void BackPage()
        {
            if (frame.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

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

                myPage.backButtonText.text = title.text;

                myPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }
            else
            {
                AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

                userPage.transform.SetParent(transform.parent, false);

                userPage.Set(user, title.text, this.frame);

                userPage.transform.SetAsLastSibling();

                userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public GameObject prfbFriendRequest;

        public void GoFriendRequest()
        {
            if (frame.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            var friendRequest = ((GameObject)Instantiate(prfbFriendRequest)).GetComponent<AUIFriendRequest>();

            friendRequest.parentFrameTween = this.frame;

            //friendRequest.backButtonText.text = FASText.Get("Friends");

            friendRequest.transform.SetParent(transform.parent, false);

            friendRequest.transform.SetAsLastSibling();

            friendRequest.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
           
            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public GameObject prfbUserSearch;

        public void OnClickUserSearch()
        {
            if (frame.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            var friendSearch = ((GameObject)Instantiate(prfbUserSearch)).GetComponent<AUIUserSearch>();

            friendSearch.parentFrame = this.frame;

            friendSearch.backButtonText.text = FASText.Get("Friends");

            friendSearch.transform.SetParent(transform.parent, false);

            friendSearch.transform.SetAsLastSibling();

            friendSearch.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMatchmakingSelectFriends : MonoBehaviour
    {
        public enum Status { Initializing, Setting, Matching, None };

        public AUIFrame frameTween;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public AUIRawImageTextureSetter bgImage;

        public RectTransform prfbSelectFriendsCell;

        private List<AUIMatchmakingSelectFriendsCell> cells = new List<AUIMatchmakingSelectFriendsCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        public AUIScrollViewContents contents;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public List<Fresvii.AppSteroid.Models.User> selectedUsers = new List<Models.User>();

        private bool isPullRefleshProc;

        public Button buttonSubmit;

        public Text cancelButtonText;

        public event Action<List<Fresvii.AppSteroid.Models.User>> OnSubmit;

        public uint maxNumberOfPlayers;

        private int initFriendCount = 0;

        public InputField searchInputField;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Cancel;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Cancel;
        }

        public void OnEditDoneSearchInputField()
        {
            contents.Clear();

            string input = searchInputField.text;

            foreach (var cell in cells)
            {
                if (cell.Friend.Name.IndexOf(input) == 0)
                {
                    contents.AddItem(cell.GetComponent<RectTransform>());

                    cell.gameObject.SetActive(true);
                }
                else
                {
                    cell.gameObject.SetActive(false);
                }
            }
        }

        IEnumerator Start()
        {
            AUITabBar.Instance.gameObject.SetActive(false);

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                yield break;
            }

            FASFriendship.GetAccountFriendList(OnGetFriendList);

            initFriendCount = selectedUsers.Count;
        }

        void OnGetFriendList(IList<Fresvii.AppSteroid.Models.Friend> friends, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
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

            foreach (Fresvii.AppSteroid.Models.Friend friend in friends)
            {
                UpdateFriend(friend);
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }

            contents.ReLayout();

            buttonSubmit.interactable = (selectedUsers.Count > 0);
        }

        public bool UpdateFriend(Fresvii.AppSteroid.Models.Friend friend)
        {
            AUIMatchmakingSelectFriendsCell cell = cells.Find(x => x.Friend.Id == friend.Id);

            if (cell != null)
            {
                cell.SetFriend(friend, this, cell.IsSelected);

                return false;
            }

            var item = GameObject.Instantiate(prfbSelectFriendsCell) as RectTransform;

            contents.AddItem(item);

            cell = item.GetComponent<AUIMatchmakingSelectFriendsCell>();

            bool isSelected = (selectedUsers.Find(x => x.Id == friend.Id) != null);

            cell.SetFriend(friend, this, isSelected);

            cells.Add(cell);

            cell.gameObject.SetActive(false);

            cell.OnCheckStateChanged += (_isSelected, _user) =>
            {
                var containedUser = selectedUsers.Find(x => x.Id == friend.Id);

                if (_isSelected && containedUser == null)
                {
                    if (selectedUsers.Count >= maxNumberOfPlayers - 1)
                    {
                        string tooMuch = FASText.Get("SelectFrindsOvered").Replace("%num", (maxNumberOfPlayers - 1).ToString());

                        Debug.LogWarning(tooMuch);

                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(tooMuch, FASText.Get("OK"), FASText.Get("OK"), FASText.Get("OK"), (del) => { });

                        cell.IsSelected = false;

                        return;
                    }

                    selectedUsers.Add(_user);
                }
                else if (!_isSelected && containedUser != null)
                {
                    selectedUsers.Remove(containedUser);
                }

                buttonSubmit.interactable = (selectedUsers.Count != initFriendCount || selectedUsers.Count > 0);
            };

            return true;
        }

        public GameObject prfbUserPage;

        public GameObject prfbMyPage;

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frameTween.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (user.Id == FAS.CurrentUser.Id)
            {
                AUIMyPage myPage = ((GameObject)Instantiate(prfbMyPage)).GetComponent<AUIMyPage>();

                myPage.transform.SetParent(transform.parent, false);

                myPage.transform.SetAsLastSibling();

                myPage.parentFrameTween = this.frameTween;

                myPage.backButtonText.text = FASText.Get("Matchmaking");

                myPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }
            else
            {
                AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

                userPage.transform.SetParent(transform.parent, false);

                userPage.Set(user, FASText.Get("Matchmaking"), this.frameTween);

                userPage.transform.SetAsLastSibling();

                userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }

            this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public void Submit()
        {
            if (OnSubmit != null)
            {
                OnSubmit(selectedUsers);
            }

            Back();
        }

        public void Cancel()
        {
            if (OnSubmit != null)
            {
                OnSubmit(null);
            }

            Back();
        }

        void Back()
        {
            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            frameTween.Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }
    }
}
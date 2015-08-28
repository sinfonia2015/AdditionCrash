using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIAddGroupMember : MonoBehaviour
    {
        public AUIFrame frameTween;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public GameObject prfbFriendCell;
        
        private List<AUIMessageFriendCell> friendCells = new List<AUIMessageFriendCell>();

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public GameObject prfbUserPage;

        public Text title;

        public AUIScrollViewContents contents;

        private List<Fresvii.AppSteroid.Models.Friend> selectedFriends = new List<Models.Friend>();

        public Fresvii.AppSteroid.Models.Group Group { get; set; }

        public Button buttonDone;

        [HideInInspector]
        public AUIGroupInfo groupInfo;

        public InputField searchInputField;

        void Awake()
        {
            //contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;
        }

        IEnumerator Start()
        {
            buttonDone.interactable = false;

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASFriendship.GetUserFriendList(FAS.CurrentUser.Id, OnGetUserFriendList);

            FASUtility.SendPageView("pv.messages.members.add", this.Group.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });    
        }

        void OnEnable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;
        }

        void OnDisable()
        {
            pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;
        }

        void OnPullDownReflesh()
        {
            pullReflesh.PullRefleshCompleted();

            return;
        }

        void OnPullUpReflesh()
        {
            pullReflesh.PullRefleshCompleted();

            return;
        }

        void OnGetUserFriendList(IList<Fresvii.AppSteroid.Models.Friend> friends, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
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

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Friend friend in friends)
            {
                List<Fresvii.AppSteroid.Models.Member> members = (List<Fresvii.AppSteroid.Models.Member>) Group.Members;

                if (members.Find(x => x.Id == friend.Id) == null)
                {
                    added |= UpdateFriend(friend);
                }
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }

            contents.ReLayout();
        }

        private bool UpdateFriend(Fresvii.AppSteroid.Models.Friend friend)
        {
            AUIMessageFriendCell cell = friendCells.Find(x => x.Friend.Id == friend.Id);

            if (cell != null)
            {
                cell.SetFriend(friend);

                return false;
            }

            GameObject go = Instantiate(prfbFriendCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUIMessageFriendCell>();

            cell.SetFriend(friend);

            friendCells.Add(cell);

            cell.gameObject.SetActive(false);

            cell.OnSelectStateChanged += OnSelectedFriendStateChanged;

            cell.OnGoToUserPage += GoToUserPage;

            return true;
        }

        public void OnSelectedFriendStateChanged(bool selected, Fresvii.AppSteroid.Models.Friend friend)
        {
            if (selected)
            {
                if (!selectedFriends.Contains(friend))
                {
                    selectedFriends.Add(friend);
                }
            }
            else
            {
                if (selectedFriends.Contains(friend))
                {
                    selectedFriends.Remove(friend);
                }
            }

            buttonDone.interactable = (selectedFriends.Count > 0);
        }

        public void OnClickCancel()
        {
            if (frameTween.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            frameTween.Animate(Vector2.zero, new Vector2(0f, - rectTransform.rect.height), () =>
            {
                Destroy(this.gameObject);   
            });
        }

        public void OnClickDone()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            buttonDone.interactable = false;

            int doneCount = 0;

            AUIManager.Instance.ShowLoadingSpinner();

            foreach (Fresvii.AppSteroid.Models.Friend friend in selectedFriends)
            {
                FASGroup.AddMember(this.Group.Id, friend.Id, (member, error)=>
                {
                    doneCount++;

                    if (error == null)
                    {
                        Group.Members.Add(member);

                        Group.MembersCount++;
                    }
                    else
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                        if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.NetworkNotReachable)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });
                        }
                        else
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(error.Detail, (del)=>{ });
                        }

                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                        {
                            Debug.LogError("GroupService.AddMember : " + error.ToString());
                        }
                    }

                    if (doneCount == selectedFriends.Count)
                    {
                        AUIManager.Instance.HideLoadingSpinner();

                        buttonDone.interactable = true;

                        OnClickCancel();
                    }
                });
            }
        }

        public void OnEditDoneSearchInputField()
        {
            contents.Clear();

            string input = searchInputField.text;

            foreach (AUIMessageFriendCell cell in friendCells)
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

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frameTween.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

            userPage.transform.SetParent(transform.parent, false);

            userPage.Set(user, title.text, this.frameTween);

            userPage.transform.SetAsLastSibling();

            userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }
    }
}
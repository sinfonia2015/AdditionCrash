using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIFriendRequestScrollView : MonoBehaviour
    {
        public enum Mode {Requested, Hidden};

        public AUIFriendRequest auiFriendRequest;

        public GameObject prfbFriendRequestCell;

        public AUIScrollViewContents contents;

        public Mode mode;

        private List<AUIFriendRequestedCell> requestedCells = new List<AUIFriendRequestedCell>();

        private Fresvii.AppSteroid.Models.ListMeta meta;

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        
        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASFriendship.GetFriendshipRequestedUsersList(FAS.CurrentUser.Id, 0, mode == Mode.Hidden, OnGetFriendshipRequestedUsersList);
        }

        void OnEnable()
        {
            StartCoroutine(Init());
        }

        void OnGetFriendshipRequestedUsersList(IList<Fresvii.AppSteroid.Models.Friend> friends, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null)
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

            auiFriendRequest.StartCoroutine(SetRequestedUsersList(friends, meta, error));
        }

        IEnumerator SetRequestedUsersList(IList<Fresvii.AppSteroid.Models.Friend> friends, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            while (this != null && !this.gameObject.activeInHierarchy)
            {
                yield return 1;
            }

            if (this == null)
            {
                yield break;
            }

            this.listMeta = meta;

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
        }

        private bool UpdateFriend(Fresvii.AppSteroid.Models.Friend friend)
        {
            AUIFriendRequestedCell cell = requestedCells.Find(x => x.Friend.Id == friend.Id);

            if (cell != null)
            {
                cell.SetFriend(friend, this, mode);

                return false;
            }

            GameObject go = Instantiate(prfbFriendRequestCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUIFriendRequestedCell>();

            cell.SetFriend(friend, this, mode);

            requestedCells.Add(cell);

            cell.gameObject.SetActive(false);

            return true;
        }
      
        public void AcceptFriendshipRequest(AUIFriendRequestedCell cell)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            AUIManager.Instance.ShowLoadingSpinner();

            FASFriendship.AcceptFriendshipRequest(cell.Friend.Id, (request, error) =>
            {
                AUIManager.Instance.HideLoadingSpinner();

                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });
                }
                else
                {
                    cell.cellDeleteAnimator.Animate(this.contents, (size) =>
                    {
                        requestedCells.Remove(cell);

                        contents.RemoveItem(cell.GetComponent<RectTransform>(), size);

                        Destroy(cell.gameObject);
                    });

                    if (AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count > 0)
                        AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count--;
                }
            });
        }

        public void HideFriendshipRequest(AUIFriendRequestedCell cell)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            AUIManager.Instance.ShowLoadingSpinner();

            FASFriendship.HideFriendshipRequest(cell.Friend.Id, (request, error) =>
            {
                AUIManager.Instance.HideLoadingSpinner();

                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });
                }
                else
                {
                    cell.cellDeleteAnimator.Animate(this.contents, (size) =>
                    {
                        requestedCells.Remove(cell);

                        contents.RemoveItem(cell.GetComponent<RectTransform>(), size);

                        Destroy(cell.gameObject);
                    });

                    if (AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count > 0)
                        AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count--;
                }
            });
        }
    }
}

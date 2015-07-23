using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMessageCreate : MonoBehaviour
    {
        public AUIFrame frameTween;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public GameObject prfbFriendCell;

        public AUIScrollViewContents contents;

        public AUIInputMessage auiInputMessage;

        private List<AUIMessageFriendCell> friendCells = new List<AUIMessageFriendCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public GameObject prfbUserPage;

        public Text title;

        //public Text backButtonText;

        public Text toUserText;

        private List<Fresvii.AppSteroid.Models.Friend> selectedFriends = new List<Models.Friend>();

        public GameObject prfbMessages;

        public Button buttonLiveHelp;

        private bool isLiveHelp = false;

        public Text textLiveHelp;
       
        IEnumerator Start()
        {
            buttonLiveHelp.gameObject.SetActive(false);

            AUIManager.Instance.HideLoadingSpinner();

            toUserText.text = FASText.Get("To");

            auiInputMessage.SetBlock(true);

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (GetComponent<AUIFrame>().Animating)
            {
                yield return 1;
            }

            contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;

            FASFriendship.GetUserFriendList(FAS.CurrentUser.Id, OnGetUserFriendList);

            while (FAS.OfficialUser == null)
            {
                yield return 1;
            }

            buttonLiveHelp.gameObject.SetActive(FASConfig.Instance.officialChat);
        }

        void OnEnable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            auiInputMessage.OnInputTextDone += OnInputTextDone;

            auiInputMessage.OnStickerSelected += OnStickerSelected;

            auiInputMessage.OnTextureSelected += OnTextureSelected;

            auiInputMessage.OnVideoSelected += OnVideoSelected;

            auiInputMessage.OnHeightChanged += OnInputHeightChanged;

            auiInputMessage.OnBlockTapped += OnBlockTapped;
        }

        void OnDisable()
        {
            pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            auiInputMessage.OnInputTextDone -= OnInputTextDone;

            auiInputMessage.OnStickerSelected -= OnStickerSelected;

            auiInputMessage.OnTextureSelected -= OnTextureSelected;

            auiInputMessage.OnVideoSelected -= OnVideoSelected;

            auiInputMessage.OnHeightChanged -= OnInputHeightChanged;

            auiInputMessage.OnBlockTapped -= OnBlockTapped;
        }

        void OnBlockTapped()
        {
            Debug.Log("OnBlockTapped");

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("NoSelectedMember"), FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"), (del) => { });

            return;
        }

        void OnPullDownReflesh()
        {
            isPullRefleshProc = true;

            FASFriendship.GetUserFriendList(FAS.CurrentUser.Id, OnGetUserFriendList);
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                isPullRefleshProc = true;

                FASFriendship.GetUserFriendList(FAS.CurrentUser.Id, (uint)listMeta.NextPage, OnGetUserFriendList);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        void OnInputTextDone(string text)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            List<string> userIds = new List<string>();

            if (isLiveHelp)
            {
                userIds.Add(FAS.OfficialUser.Id);
            }
            else
            {
                foreach (Fresvii.AppSteroid.Models.Friend friend in selectedFriends)
                {
                    userIds.Add(friend.Id);
                }
            }

            userIds.Add(FAS.CurrentUser.Id);

            AUIManager.Instance.ShowLoadingSpinner();

            if (isLiveHelp)
            {
                FASGroup.CreatePair(FAS.OfficialUser.Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, text, null, null, null);
                    }
                });
            }
            else if (selectedFriends.Count > 1)
            {
                FASGroup.CreateGroup(userIds.ToArray(), (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, text, null, null, null);
                    }
                });
            }
            else
            {
                FASGroup.CreatePair(selectedFriends[0].Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, text, null, null, null);
                    }
                });
            }
        }

        void OnTextureSelected(Texture2D texture)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            List<string> userIds = new List<string>();

            if (isLiveHelp)
            {
                userIds.Add(FAS.OfficialUser.Id);
            }
            else
            {
                foreach (Fresvii.AppSteroid.Models.Friend friend in selectedFriends)
                {
                    userIds.Add(friend.Id);
                }
            }

            userIds.Add(FAS.CurrentUser.Id);

            AUIManager.Instance.ShowLoadingSpinner();

            if (isLiveHelp)
            {
                FASGroup.CreatePair(FAS.OfficialUser.Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", texture, null, null);
                    }
                });
            }
            else if (selectedFriends.Count > 1)
            {
                FASGroup.CreateGroup(userIds.ToArray(), (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", texture, null, null);
                    }
                });
            }
            else
            {
                FASGroup.CreatePair(selectedFriends[0].Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", texture, null, null);
                    }
                });
            }
        }

        void OnStickerSelected(Fresvii.AppSteroid.Models.Sticker sticker)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            List<string> userIds = new List<string>();

            if (isLiveHelp)
            {
                userIds.Add(FAS.OfficialUser.Id);
            }
            else
            {
                foreach (Fresvii.AppSteroid.Models.Friend friend in selectedFriends)
                {
                    userIds.Add(friend.Id);
                }
            }

            userIds.Add(FAS.CurrentUser.Id);

            AUIManager.Instance.ShowLoadingSpinner();

            if (isLiveHelp)
            {
                FASGroup.CreatePair(FAS.OfficialUser.Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", null, null, sticker);
                    }
                });
            }
            else if (selectedFriends.Count > 1)
            {
                FASGroup.CreateGroup(userIds.ToArray(), (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", null, null, sticker);
                    }
                });
            }
            else
            {
                FASGroup.CreatePair(selectedFriends[0].Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", null, null, sticker);
                    }
                });
            }
        }

        void OnVideoSelected(Fresvii.AppSteroid.Models.Video video, Texture2D thumbnail)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            List<string> userIds = new List<string>();

            if (isLiveHelp)
            {
                userIds.Add(FAS.OfficialUser.Id);
            }
            else
            {
                foreach (Fresvii.AppSteroid.Models.Friend friend in selectedFriends)
                {
                    userIds.Add(friend.Id);
                }
            }

            userIds.Add(FAS.CurrentUser.Id);

            AUIManager.Instance.ShowLoadingSpinner();

            if (isLiveHelp)
            {
                FASGroup.CreatePair(FAS.OfficialUser.Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", null, video, null);
                    }
                });
            }
            else if (selectedFriends.Count > 1)
            {
                FASGroup.CreateGroup(userIds.ToArray(), (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", null, video, null);
                    }
                });
            }
            else
            {
                FASGroup.CreatePair(selectedFriends[0].Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error.ToString());

                        AUIManager.Instance.HideLoadingSpinner();
                    }
                    else
                    {
                        OnGroupCreated(group, error, "", null, video, null);
                    }
                });
            }
        }
        
        private bool commentSending;

        private bool imageSending;

        private void OnGroupCreated(Fresvii.AppSteroid.Models.Group group, Fresvii.AppSteroid.Models.Error error, string text, Texture2D clipImage, Fresvii.AppSteroid.Models.Video video, Fresvii.AppSteroid.Models.Sticker sticker)
        {
            if (error != null)
            {
                AUIManager.Instance.HideLoadingSpinner();

                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.WWWTimeOut)
                {
                    if (this.gameObject.activeInHierarchy && this.enabled)
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("TimeOut"), delegate(bool del) { });
                    }
                }
                else
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), delegate(bool del) { });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(text))
                {
                    FASGroup.SendGroupMessage(group.Id, text, (message, _error) =>
                    {
                        AUIManager.Instance.HideLoadingSpinner();

                        if (_error != null)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                        }
                        else
                        {
                            GoToMessage(group);
                        }                       
                    });
                }
                else if(clipImage != null)
                {
                    FASGroup.SendGroupMessage(group.Id, clipImage, delegate(Fresvii.AppSteroid.Models.GroupMessage message, Fresvii.AppSteroid.Models.Error _error)
                    {
                        AUIManager.Instance.HideLoadingSpinner();

                        if (_error != null)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                        }
                        else
                        {
                            GoToMessage(group);
                        }              
                    });
                }
                else if(video != null)
                {
                    FASGroup.SendGroupMessageVideo(group.Id, video.Id, delegate(Fresvii.AppSteroid.Models.GroupMessage message, Fresvii.AppSteroid.Models.Error _error)
                    {
                        if (_error != null)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                        }
                        else
                        {
                            GoToMessage(group);
                        }         
                    });
                }
                else if(sticker != null)
                {
                    FASGroup.SendGroupMessageSticker(group.Id, sticker.Id, delegate(Fresvii.AppSteroid.Models.GroupMessage message, Fresvii.AppSteroid.Models.Error _error)
                    {
                        if (_error != null)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                        }
                        else
                        {
                            GoToMessage(group);
                        }
                    });
                }
            }
        }

        public void GoToMessage(AppSteroid.Models.Group group)
        {
            if (frameTween.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIMessages messagesPage = ((GameObject)Instantiate(prfbMessages)).GetComponent<AUIMessages>();

            messagesPage.Group = group;

            messagesPage.transform.SetParent(transform.parent, false);

            messagesPage.transform.SetAsLastSibling();

            messagesPage.parentFrameTween = this.parentFrameTween;

            messagesPage.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        void OnInputHeightChanged(float height)
        {
            contents.padding.bottom = (int)height;
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

            this.listMeta = meta;

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Friend friend in friends)
            {
                added |= UpdateFriend(friend);
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
            if(selected)
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

            SetToUsersName();

            auiInputMessage.SetBlock(selectedFriends.Count == 0);
        }

        private void SetToUsersName()
        {
            string names = FASText.Get("To") + " ";

            for (int i = 0; i < selectedFriends.Count; i++)
            {
                names += selectedFriends[i].Name;

                if (i != selectedFriends.Count - 1)
                {
                    names += ", ";
                }
            }

            toUserText.text = names;
        }

        public void BackPage()
        {
            if (frameTween.Animating) return;

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

        public void OnClickLiveHelp()
        {
            isLiveHelp = !isLiveHelp;

            scrollView.gameObject.SetActive(!isLiveHelp);

            if (isLiveHelp)
            {
                textLiveHelp.text = FASText.Get("Friends");

                toUserText.text = FASText.Get("To") + " " + FAS.OfficialUser.Name;

                auiInputMessage.SetBlock(false);
            }
            else
            {
                auiInputMessage.SetBlock(selectedFriends.Count == 0);

                textLiveHelp.text = FASText.Get("LiveHelp");

                SetToUsersName();
            }
        }
    }
}
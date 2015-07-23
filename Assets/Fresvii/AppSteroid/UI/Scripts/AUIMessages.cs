using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMessages : MonoBehaviour
    {
        public AUIFrame frame;
        
        public Text title;

        public Text downBackButtonText;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIInputMessage auiInputMessage;

        private bool pullRefleshing;

        public AUIScrollViewContents contents;

        public AUIScrollRect scrollView;

        public RectTransform prfbOtherMessageCell;

        public RectTransform prfbMyMessageCell;

        public Fresvii.AppSteroid.Models.Group Group { get; set; }

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private List<AUIGroupMessageCell> groupMessageCells = new List<AUIGroupMessageCell>();

        public float pollingDuration = 30f;

        private bool isPullDown;

        public GameObject prfbGroupInfo;

        public GameObject leftBackButton, downBackButton;

        public bool isModal;

        public float bottomMargin = 40f;

        bool init = false;

        public AUITextSetter titleTextSetter;

        void OnEnable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            auiInputMessage.OnInputTextDone += OnInputTextDone;

            auiInputMessage.OnStickerSelected += OnStickerSelected;

            auiInputMessage.OnTextureSelected += OnTextureSelected;

            auiInputMessage.OnVideoSelected += OnVideoSelected;

            auiInputMessage.OnHeightChanged += OnInputHeightChanged;

            FASEvent.OnGroupMessageCreated += OnGroupMessageCreated;

            AUIManager.OnEscapeTapped += BackPage;

            InvokeRepeating("GetGroupMessageList", 0f, pollingDuration);

            if (!init)
            {
                AUIManager.Instance.ShowLoadingSpinner(false);

                init = true;
            }

            StartCoroutine(Init());
        }
      
        void OnDisable()
        {
            AUIManager.Instance.HideLoadingSpinner();

            pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            auiInputMessage.OnInputTextDone -= OnInputTextDone;

            auiInputMessage.OnStickerSelected -= OnStickerSelected;

            auiInputMessage.OnTextureSelected -= OnTextureSelected;

            auiInputMessage.OnVideoSelected -= OnVideoSelected;

            auiInputMessage.OnHeightChanged -= OnInputHeightChanged;

            FASEvent.OnGroupMessageCreated -= OnGroupMessageCreated;

            AUIManager.OnEscapeTapped -= BackPage;

            if(Group != null)
                AUITabBar.Instance.GroupMessageRead(Group.Id);
        }

        IEnumerator Init()
        {
            while (this.Group == null)
            {
                yield return 1;
            }

            SetGroupName();
        }

        void SetGroupName()
        {
            string groupName = "";

            if (Group.Pair)
            {
                foreach (var member in Group.MessageMembers)
                {
                    if (member.Id != FAS.CurrentUser.Id)
                    {
                        groupName = member.Name;

                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Group.MessageMembers.Count; i++)
                {
                    groupName += Group.MessageMembers[i].Name;

                    if (i != Group.MessageMembers.Count - 1)
                        groupName += ", ";
                }

                titleTextSetter.truncatedReplacement = "...(" + Group.MembersCount.ToString() + ")";
            }

            title.text = groupName;
        }

        void OnPullDownReflesh()
        {
            if (pullRefleshing) return;

            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                pullRefleshing = true;

                isPullDown = true;

                FASGroup.GetGroupMessageList(Group.Id, (uint)listMeta.NextPage, OnGetGroupMessageList);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        void OnPullUpReflesh()
        {
            if (pullRefleshing) return;

            pullRefleshing = true;

            FASGroup.GetGroupMessageList(Group.Id, OnGetGroupMessageList);
        }

        public float newMessageTweenDuration = 0.25f;

        void OnInputTextDone(string text)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var item = GameObject.Instantiate(prfbMyMessageCell) as RectTransform;

            contents.AddItem(item);

            AUIGroupMessageCell cell = item.GetComponent<AUIGroupMessageCell>();

            Fresvii.AppSteroid.Models.GroupMessage groupMessage = null;

            //  Create Text message
            groupMessage = new Fresvii.AppSteroid.Models.GroupMessage(FAS.CurrentUser, text, System.DateTime.Now, Fresvii.AppSteroid.Models.GroupMessage.ContentType.Text);

            FASGroup.SendGroupMessage(Group.Id, text, (_groupMessage, error) =>
            {
                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("OK"), FresviiGUIText.Get("Cancel"), FresviiGUIText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), ( del ) => { });

                    RemoveCell(cell);
                }
                else
                {
                    cell.SetGroupMessage(_groupMessage);
                }

                Sort();
            });

            cell.auiMessages = this;

            cell.SetGroupMessage(groupMessage);

            groupMessageCells.Add(cell);

            cell.gameObject.SetActive(false);

            scrollView.GoToBottom(newMessageTweenDuration);

            auiInputMessage.Clear();
        }

        void OnStickerSelected(Fresvii.AppSteroid.Models.Sticker sticker)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            var item = GameObject.Instantiate(prfbMyMessageCell) as RectTransform;

            contents.AddItem(item);

            AUIGroupMessageCell cell = item.GetComponent<AUIGroupMessageCell>();

            Fresvii.AppSteroid.Models.GroupMessage groupMessage = null;

            //  Create Text message
            groupMessage = new Fresvii.AppSteroid.Models.GroupMessage(FAS.CurrentUser, "", System.DateTime.Now, Fresvii.AppSteroid.Models.GroupMessage.ContentType.Sticker);

            groupMessage.Sticker = sticker;

            FASGroup.SendGroupMessageSticker(Group.Id, sticker.Id, (_groupMessage, error) =>
            {
                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("OK"), FresviiGUIText.Get("Cancel"), FresviiGUIText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });

                    Debug.LogError(error.ToString());

                    RemoveCell(cell);
                }
                else
                {
                    cell.SetGroupMessage(_groupMessage);
                }

                Sort();
            });

            cell.auiMessages = this;

            cell.SetGroupMessage(groupMessage);

            groupMessageCells.Add(cell);

            cell.gameObject.SetActive(false);

            scrollView.GoToBottom(newMessageTweenDuration);
        }

        void OnTextureSelected(Texture2D texture)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            if (texture == null)
            {
                return;
            }

            var item = GameObject.Instantiate(prfbMyMessageCell) as RectTransform;

            contents.AddItem(item);

            AUIGroupMessageCell cell = item.GetComponent<AUIGroupMessageCell>();

            Fresvii.AppSteroid.Models.GroupMessage groupMessage = null;

            //  Create Text message
            groupMessage = new Fresvii.AppSteroid.Models.GroupMessage(FAS.CurrentUser, "", System.DateTime.Now, Fresvii.AppSteroid.Models.GroupMessage.ContentType.Image);
            
            Fresvii.AppSteroid.Util.DialogManager.ShowProgressSpinnerDialog("", FASText.Get("Uploading"), false);

            FASGroup.SendGroupMessage(Group.Id, texture, (_groupMessage, error) =>
            {
                Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();
               
                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("OK"), FresviiGUIText.Get("Cancel"), FresviiGUIText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });

                    RemoveCell(cell);
                }
                else
                {
                    cell.SetGroupMessage(_groupMessage);
                }

                Sort();
            });

            cell.auiMessages = this;

            cell.SetGroupMessage(groupMessage);

            cell.clipImage.SetTexture(texture);

            groupMessageCells.Add(cell);

            cell.gameObject.SetActive(false);

            scrollView.GoToBottom(newMessageTweenDuration);
        }

        void OnVideoSelected(Fresvii.AppSteroid.Models.Video video, Texture2D thumbnail)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            var item = GameObject.Instantiate(prfbMyMessageCell) as RectTransform;

            contents.AddItem(item);

            AUIGroupMessageCell cell = item.GetComponent<AUIGroupMessageCell>();

            Fresvii.AppSteroid.Models.GroupMessage groupMessage = null;

            //  Create Text message
            groupMessage = new Fresvii.AppSteroid.Models.GroupMessage(FAS.CurrentUser, "", System.DateTime.Now, Fresvii.AppSteroid.Models.GroupMessage.ContentType.Video);

            groupMessage.Video = video;

            FASGroup.SendGroupMessageVideo(Group.Id, video.Id, (_groupMessage, error) =>
            {
                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FresviiGUIText.Get("OK"), FresviiGUIText.Get("Cancel"), FresviiGUIText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });

                    RemoveCell(cell);
                }
                else
                {
                    cell.SetGroupMessage(_groupMessage);
                }

                Sort();
            });

            cell.auiMessages = this;

            cell.SetGroupMessage(groupMessage);

            cell.clipImage.SetTexture(thumbnail);

            groupMessageCells.Add(cell);

            cell.gameObject.SetActive(false);

            scrollView.GoToBottom(newMessageTweenDuration);
        }

        public void RemoveCell(AUIGroupMessageCell cell)
        {
            groupMessageCells.Remove(cell);

            contents.RemoveItem(cell.GetComponent<RectTransform>());

            Destroy(cell.gameObject);
        }

        void OnInputHeightChanged(float height)
        {
            contents.padding.bottom = (int)(height + bottomMargin);

            contents.ReLayout();

            scrollView.GoToBottom(newMessageTweenDuration);
        }

        IEnumerator Start()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            buttonCall.gameObject.SetActive(false);

            leftBackButton.SetActive(!isModal);

            downBackButton.SetActive(isModal);

            while (GetComponent<AUIFrame>().Animating)
            {
                yield return 1;
            }

            while(Group == null || string.IsNullOrEmpty( Group.Id ))
            {
                yield return 1;
            }

            FASGroup.GetGroupMessageList(Group.Id, OnGetGroupMessageList);

            AUITabBar.Instance.GroupMessageRead(Group.Id);

            FASUtility.SendPageView("pv.messages.show", this.Group.Id, System.DateTime.UtcNow, (e) =>
            {
				if (e != null)
					Debug.LogError(e.ToString());
			});

#if GROUP_CONFERENCE
            bool includingOficial = false;
             
            Group.FetchMembers((error) =>
            {
                if (error == null)
                {
                    foreach (var member in Group.Members)
                    {
                        if (member.Official)
                        {
                            includingOficial = true;

                            break;
                        }
                    }

                    if (!includingOficial)
                    {
                        buttonCall.gameObject.SetActive(true);
                    }
                }
            });
#endif
        }

        void GetGroupMessageList()
        {
            if (!FASUser.IsLoggedIn())
            {
                return;
            }

            if (GetComponent<AUIFrame>().Animating)
            {
                return;
            }

            if(Group == null || string.IsNullOrEmpty( Group.Id ))
            {
                return;
            }

            FASGroup.GetGroupMessageList(Group.Id, OnGetGroupMessageList);        
        }

        void OnGetGroupMessageList(IList<Fresvii.AppSteroid.Models.GroupMessage> groupMessages, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            AUIManager.Instance.HideLoadingSpinner();

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

                if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.NotFound || (Application.platform == RuntimePlatform.Android && error.Detail.IndexOf("FileNotFound") >= 0))
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                    {
                        Debug.LogError(error.ToString());
                    }

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("GroupNone"), (del) => { });

                    BackPage();
                }

                return;
            }

            if (this.listMeta == null || this.listMeta.CurrentPage != 0)
            {
                this.listMeta = meta;
            }

            added = false;

            foreach (Fresvii.AppSteroid.Models.GroupMessage groupMessage in groupMessages)
            {
                UpdateGroupMessage(groupMessage);
            }

            if (pullRefleshing)
            {
                pullReflesh.PullRefleshCompleted(true);

                pullRefleshing = false;
            }

            added = false;
            
            Sort();
        }

        bool added = false;

        string latestMessageId = "";

        bool initialized;

        private void Sort()
        {
            // Sort
            groupMessageCells.Sort((a, b) => System.DateTime.Compare(a.GroupMessage.UpdatedAt, b.GroupMessage.UpdatedAt));

            System.DateTime dt = System.DateTime.MinValue;

            foreach (var obj in groupMessageCells)
            {
                bool hasTimeLine = (dt.Year != obj.GroupMessage.CreatedAt.Year || dt.Month != obj.GroupMessage.CreatedAt.Month || dt.Day != obj.GroupMessage.CreatedAt.Day);

                obj.SetTimeLine(hasTimeLine);

                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);

                dt = obj.GroupMessage.CreatedAt;
            }

            if (groupMessageCells.Count > 0)
            {
                if (!string.IsNullOrEmpty(groupMessageCells[groupMessageCells.Count - 1].GroupMessage.Id))
                {
                    FASGroup.MarkAsReadGroupMessage(Group.Id, groupMessageCells[groupMessageCells.Count - 1].GroupMessage.Id, (e) =>
                    {
                        if (e != null)
                        {
                            Debug.LogError(e.ToString());
                        }
                    });
                }

                if (!initialized)
                {
                    initialized = true;

                    scrollView.GoToBottom(0f);
                }

                if (isPullDown)
                {
                    isPullDown = false;

                    scrollView.Pinned();
                }
                else if (latestMessageId != groupMessageCells[groupMessageCells.Count - 1].GroupMessage.Id)
                {
                    latestMessageId = groupMessageCells[groupMessageCells.Count - 1].GroupMessage.Id;

                    scrollView.GoToBottom(newMessageTweenDuration);
                }
            }

            contents.ReLayout();
        }

        void OnGroupMessageCreated(Fresvii.AppSteroid.Models.GroupMessage groupMessage)
        {
            FASGroup.MarkAsReadGroupMessage(groupMessage.GroupId, groupMessage.Id, (e) => { });

            UpdateGroupMessage(groupMessage);

            Sort();
        }

        private void UpdateGroupMessage(Fresvii.AppSteroid.Models.GroupMessage groupMessage)
        {
            AUIGroupMessageCell cell = groupMessageCells.Find(x => x.GroupMessage.Id == groupMessage.Id);

            if (cell != null)
            {
                cell.SetGroupMessage(groupMessage);
            }
            else
            {
                RectTransform prfbGroupMessage = (groupMessage.User.Id == FAS.CurrentUser.Id) ? prfbMyMessageCell : prfbOtherMessageCell;

                var item = GameObject.Instantiate(prfbGroupMessage) as RectTransform;

                contents.AddItem(item);

                cell = item.GetComponent<AUIGroupMessageCell>();

                cell.auiMessages = this;

                cell.SetGroupMessage(groupMessage);

                groupMessageCells.Add(cell);

                cell.gameObject.SetActive(false);

                added = true;

                AUITabBar.Instance.GroupMessageRead(groupMessage.GroupId);
            }
        }

        public void BackPage()
        {
            if (frame.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (isModal)
            {
                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(0f, - rectTransform.rect.height), () =>
                {
                    Destroy(this.gameObject);
                });
            }
            else
            {
                parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
                {
                    Destroy(this.gameObject);
                });
            }
        }

        public void GoGroupInfo()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIGroupInfo groupInfo = ((GameObject)Instantiate(prfbGroupInfo)).GetComponent<AUIGroupInfo>();

            groupInfo.transform.SetParent(transform.parent, false);

            groupInfo.transform.SetAsLastSibling();

            groupInfo.Set(Group, this.frame);

            groupInfo.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public GameObject prfbMyPage, prfbUserPage;

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frame.Animating) return;

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

        public GameObject prfbGroupConference;

        public void OnClickCall()
        {
#if GROUP_CONFERENCE

            if (frame.Animating) return;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            if (Group.MembersCount > Fresvii.AppSteroid.Services.GroupConferenceService.MaxCallMemberCount)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("GroupConferenceMemberLimitation"), (del) =>
                {

                });

                return;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIGroupConference groupConference = ((GameObject)Instantiate(prfbGroupConference)).GetComponent<AUIGroupConference>();

            groupConference.SetGroup(Group);

            groupConference.transform.SetParent(transform.parent, false);

            groupConference.transform.SetAsLastSibling();

            groupConference.parentFrameTween = this.frame;

            groupConference.frameTween.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () =>
            {
                this.gameObject.SetActive(false);
            });
#endif
        }

        public Button buttonCall;

        void Update()
        {
#if GROUP_CONFERENCE
            buttonCall.interactable = !FASConference.IsCalling();
#endif
        }
    }
}
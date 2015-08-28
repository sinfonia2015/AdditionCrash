using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIUserPage : MonoBehaviour
    {
        public AUIRawImageTextureSetter auiRawImage;

        public Text userName;

        public Text userCode;

        public Text description;

        public Text friendsNum, videosNum;
       
        public AUIFrame frameTween;

        public Fresvii.AppSteroid.Models.User User;

        public Text backButtonText;

        private AUIFrame parentFrameTween;

        public AUIRawImageTextureSetter bgImage;

        public GameObject prfbFriendList;

        public GameObject prfbMessages;

        public GameObject prfbGroupConference;

        public GameObject prfbVideoList;

        public Sprite sendRequest, isFriend, unfriend, requested;

        public Button friendButton;

        public Text friendStatus;

        public int deleteGroupRetryCount = 5;

        public float deleteGroupRetryInterval = 5f;

        public Transform statsNode;

        public GameObject prfbAUIStatCell;

        public AUIGridLayoutHelper auiGridLayoutHelper;

        public Color[] cellColors;

        public Text statsTitle;

        public static event Action<Fresvii.AppSteroid.Models.User> OnUnfriended;

        void Awake()
        {
#if !GROUP_CONFERENCE
            buttonCall.gameObject.SetActive(false);
#endif
            statsTitle.text = FASConfig.Instance.appName + " " + FASText.Get("Stats");

            topContentsLayoutElement.preferredHeight = topContentsLayoutElement.minHeight = 820f;
        }

        public void Set(Fresvii.AppSteroid.Models.User user, string backButton, AUIFrame parentFrameTween)
        {
            this.User = user;

            this.parentFrameTween = parentFrameTween;

            backButtonText.text = backButton;

            SetCurrentUserInfo();

            if (this.User.Official)
            {
                 buttonCall.gameObject.SetActive(false);
            }
        }

        private void SetFriendButton()
        {
            if (User.FriendStatus == Models.User.FriendStatuses.Friend)
            {
                friendButton.image.sprite = unfriend;

                friendButton.interactable = true;

                friendStatus.text = FASText.Get("Unfriend");
            }
            else if (User.FriendStatus == Models.User.FriendStatuses.Requesting)
            {
                friendButton.image.sprite = sendRequest;

                friendButton.interactable = false;

                friendStatus.text = FASText.Get("RequestSent");
            }
            else if (User.FriendStatus == Models.User.FriendStatuses.Requested)
            {
                friendButton.image.sprite = requested;

                friendButton.interactable = true;

                friendStatus.text = FASText.Get("Respond");

                requestedPanel.SetActive(true);

                topContentsLayoutElement.preferredHeight = topContentsLayoutElement.minHeight = 1000f;
            }
            else
            {
                friendButton.image.sprite = isFriend;

                friendButton.interactable = true;

                friendStatus.text = FASText.Get("AddFriend");
            }
        }

        bool statInitialized = false;

        IEnumerator Init()
        {
            requestedPanel.SetActive(false);

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (User == null)
            {
                yield return 1;
            }

            FASUser.GetUser(User.Id, (user, error) =>
            {
                if (error == null)
                {
                    this.User = user;

                    SetCurrentUserInfo();
                }
            });

            List<string> userIds = new List<string>();

            userIds.Add(User.Id);

            FASPlayStats.GetStatList(userIds.ToArray(), (stats, meta, error) => 
            {
                if (error != null)
                {

                }
                else
                {
                    int i = 0;

                    foreach (var stat in stats)
                    {
                        GameObject statCell = Instantiate(prfbAUIStatCell) as GameObject;

                        var auiStatCell = statCell.GetComponent<AUIStatCell>();

                        auiStatCell.Set(stat, cellColors[i % cellColors.Length]);

                        statCell.transform.SetParent(statsNode, false);

                        i++;
                    }

                    if (!statInitialized)
                    {
                        int addCount = (3 - stats.Count % 3);

                        addCount %= 3;

                        for (int j = 0; j < addCount; j++)
                        {
                            GameObject statCell = Instantiate(prfbAUIStatCell) as GameObject;

                            statCell.transform.SetParent(statsNode, false);
                        }

                        statInitialized = true;
                    }
                }

                auiGridLayoutHelper.CalcSize();

            });

            FASUtility.SendPageView("pv.my_page", this.User.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });    
        }

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            FASEvent.OnFriendshipRequestCreated += OnFriendshipRequestCreated;

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            FASEvent.OnFriendshipRequestCreated -= OnFriendshipRequestCreated;
        }

        void OnFriendshipRequestCreated(Fresvii.AppSteroid.Models.User user)
        {
            if (user.Id == this.User.Id)
            {
                FASUser.GetUser(User.Id, (_user, _error) =>
                {
                    if (_error == null)
                    {
                        this.User = _user;

                        SetCurrentUserInfo();
                    }
                });
            }
        }

        void SetCurrentUserInfo()
        {
            auiRawImage.Set(User.ProfileImageUrl);

            bgImage.Set(User.ProfileImageUrl);

            userName.text = User.Name;

            userCode.text = User.UserCode;

            description.text = User.Description;

            videosNum.text = User.VideosCount.ToString();

            friendsNum.text = User.FriendsCount.ToString();

            SetFriendButton();
        }

        public void Back()
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

        public void GoToFriendList()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject friendList = Instantiate(prfbFriendList) as GameObject;

            friendList.GetComponent<RectTransform>().SetParent(transform.parent, false);

            friendList.transform.SetAsLastSibling();

            AUIFriendList auiFriendList = friendList.GetComponent<AUIFriendList>();

            auiFriendList.User = this.User;

            auiFriendList.parentFrameTween = this.frameTween;

            frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            auiFriendList.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });
        }

        public void GoToVideoList()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbVideoList) as GameObject;

            go.GetComponent<RectTransform>().SetParent(transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIVideoList videoList = go.GetComponent<AUIVideoList>();

            videoList.backButtonText.text = FASText.Get("Back");

            videoList.parentFrameTween = this.frameTween;

            videoList.User = this.User;

            frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            videoList.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });
        }

        public void GoToMessage()
        {
            if (frameTween.Animating) return;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            FASGroup.CreatePair(User.Id, (group, error) =>
            {
                if (error == null)
                {
                    RectTransform rectTransform = GetComponent<RectTransform>();

                    AUIMessages messagesPage = ((GameObject)Instantiate(prfbMessages)).GetComponent<AUIMessages>();

                    messagesPage.Group = group;

                    messagesPage.transform.SetParent(transform.parent, false);

                    messagesPage.transform.SetAsLastSibling();

                    messagesPage.parentFrameTween = this.frameTween;

                    messagesPage.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

                    this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
                    {
                        this.gameObject.SetActive(false);
                    });
                }
                else
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                }
            });
        }

        public GameObject prfbFriendRequest;

        void GoToFriendRequest()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbFriendRequest) as GameObject;

            go.GetComponent<RectTransform>().SetParent(transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIFriendRequest friendRequest = go.GetComponent<AUIFriendRequest>();

            friendRequest.SetBackButton(this.User.Name);

            friendRequest.parentFrameTween = this.frameTween;

            frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            friendRequest.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });
        }

        public void OnClickCall()
        {
            if (frameTween.Animating) return;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            FASGroup.CreatePair(User.Id, (group, error) =>
            {
                if (error == null)
                {
                    RectTransform rectTransform = GetComponent<RectTransform>();

                    AUIGroupConference groupConference = ((GameObject)Instantiate(prfbGroupConference)).GetComponent<AUIGroupConference>();

                    groupConference.SetGroup(group);

                    groupConference.transform.SetParent(transform.parent, false);

                    groupConference.transform.SetAsLastSibling();

                    groupConference.parentFrameTween = this.frameTween;

                    groupConference.frameTween.Animate(new Vector2(0f,-rectTransform.rect.height), Vector2.zero, () => 
                    { 
                        this.gameObject.SetActive(false); 
                    });
                }
                else
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                }
            });
        }

        public void OnClickFriendRequest()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            if (User.FriendStatus == Models.User.FriendStatuses.Friend) 
            {
				Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel (FASText.Get ("Unfriend"), FASText.Get ("Cancel"), FASText.Get ("Close"));

				Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog (FASText.Get ("ConfirmUnfriend"), (del) =>
				{
                    if (!del)
                    {
                        return;
                    }

                    User.SetFriendStatus(Fresvii.AppSteroid.Models.FriendshipRequest.Statuses.none.ToString());

                    SetFriendButton();

					FASFriendship.UnFriend (User.Id, (error) =>
					{
						if (error == null) 
                        {
							User.SetFriendStatus (Fresvii.AppSteroid.Models.FriendshipRequest.Statuses.none.ToString ());

							DeletePairGroupMessages (User.Id, deleteGroupRetryCount, deleteGroupRetryInterval);

                            if (OnUnfriended != null)
                            {
                                OnUnfriended(this.User);
                            }
						} 
                        else 
                        {						
                            if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error) 
                            {
								Debug.LogError (error.ToString ());
							}

							Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel (FASText.Get ("OK"), FASText.Get ("Cancel"), FASText.Get ("Close"));

							Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog (FASText.Get ("UnknownError"), (_del) => { });

                            User.SetFriendStatus(Fresvii.AppSteroid.Models.FriendshipRequest.Statuses.friend.ToString());
                        }

						SetFriendButton ();
					});
				});
			} 
			else if (User.FriendStatus == Models.User.FriendStatuses.Requesting) 
			{

			}
            else if (User.FriendStatus == Models.User.FriendStatuses.Requested)
            {
                AcceptFriendshipRequest();
            }
			else 
			{
				User.SetFriendStatus (Fresvii.AppSteroid.Models.FriendshipRequest.Statuses.requesting.ToString ());

                SetFriendButton();
				
				FASFriendship.SendFriendshipRequest (User.Id, (friendshipRequest, error) =>
				{
					if (error == null) 
					{
						User.SetFriendStatus (friendshipRequest.Status.ToString ());
					} 
					else 
					{
						if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error) 
                        {
							Debug.LogError (error.ToString ());
						}
						
						Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog (FASText.Get ("UnknownError"), (del) => { });
						
						User.SetFriendStatus (Fresvii.AppSteroid.Models.FriendshipRequest.Statuses.none.ToString ());
					}
					
					SetFriendButton ();
				});
			} 
        }

        private IEnumerator DeleteGroupMessgesRetryCoroutine(string userId, int retry, float retryInterval)
        {
            yield return new WaitForSeconds(retryInterval);

            DeletePairGroupMessages(userId, retry, retryInterval);
        }

        private void DeletePairGroupMessages(string userId, int retry, float retryInterval)
        {
            FASGroup.CreatePair(User.Id, (group, error) =>
            {
                if (error == null)
                {
                    DeleteGroupMessages(group.Id, deleteGroupRetryCount, deleteGroupRetryInterval);
                }
                else
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                    {
                        Debug.LogError(error.ToString());
                    }

                    if (--retry >= 0)
                    {
                        StartCoroutine(DeleteGroupMessgesRetryCoroutine(userId, retry, retryInterval));
                    }
                }
            });
        }

        private void DeleteGroupMessages(string groupId, int retry, float retryInterval)
        {
            FASGroup.DeleteMessages(groupId, (error) =>
            {
                if (error != null)
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                    {
                        Debug.LogError(error.ToString());
                    }

                    if (--retry >= 0)
                    {
                        StartCoroutine(DeleteGroupRetryCoroutine(groupId, retry, retryInterval));
                    }
                }
                else
                {
                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
                    {
                        Debug.Log("Success: DeleteGroupMessages");
                    }
                }
            });
        }

        private IEnumerator DeleteGroupRetryCoroutine(string groupId, int retry, float retryInterval)
        {
            yield return new WaitForSeconds(retryInterval);

            DeleteGroupMessages(groupId, retry, retryInterval);
        }

        public Button buttonCall;

        void Update()
        {            
#if GROUP_CONFERENCE
            buttonCall.interactable = !FASConference.IsCalling();
#endif
            if (Input.GetMouseButtonDown(0))
            {
                if (copyBalloon.gameObject.activeInHierarchy)
                {
                    StartCoroutine(DelayHideCopyBalloon());
                }
            }
        }

        public AUIFadeSetActive copyBalloon;

        public void OnLongTapUserCode()
        {
            copyBalloon.FadeIn();
        }

        public void CopyUserCode()
        {
            Fresvii.AppSteroid.Util.Clipboard.SetText(this.User.UserCode);
        }

        IEnumerator DelayHideCopyBalloon()
        {
            yield return new WaitForSeconds(0.5f);

            copyBalloon.FadeOut();
        }

        public GameObject requestedPanel;

        public void AcceptFriendshipRequest()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            AUIManager.Instance.ShowLoadingSpinner();

            FASFriendship.AcceptFriendshipRequest(this.User.Id, (request, error) =>
            {
                AUIManager.Instance.HideLoadingSpinner();

                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });
                }
                else
                {
                    StartCoroutine(FadeOutRequestedPanel());

                    if (AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count > 0)
                    {
                        AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count--;
                    }

                    User.SetFriendStatus(Models.User.FriendStatuses.Friend);

                    SetCurrentUserInfo();
                }
            });
        }

        public float requestedPanelDeleteDuration = 0.5f;

        public LayoutElement requestedPanelLayoutElement;

        public iTween.EaseType easetype;

        public Graphic requestedPanelFade;

        public LayoutElement topContentsLayoutElement;

        IEnumerator FadeOutRequestedPanel()
        {
            requestedPanelFade.gameObject.SetActive(true);

            requestedPanelFade.CrossFadeAlpha(0f, 0f, true);

            yield return 1;

            requestedPanelFade.CrossFadeAlpha(1f, 0.5f * requestedPanelDeleteDuration, true);

            yield return new WaitForSeconds(0.5f * requestedPanelDeleteDuration);

            iTween.ValueTo(this.gameObject, iTween.Hash("time", requestedPanelDeleteDuration, "from", topContentsLayoutElement.preferredHeight, "to", topContentsLayoutElement.preferredHeight - requestedPanelLayoutElement.preferredHeight, "easetype", easetype, "onupdate", "OnUpdateContentSize"));

            iTween.ValueTo(this.gameObject, iTween.Hash("time", requestedPanelDeleteDuration, "from", requestedPanelLayoutElement.preferredHeight, "to", 0f, "easetype", easetype, "onupdate", "OnUpdateDeleteCell", "oncomplete", "OnCompleteDeleteCell"));
        }

        void OnUpdateDeleteCell(float value)
        {
            requestedPanelLayoutElement.preferredHeight = requestedPanelLayoutElement.minHeight = value;
        }

        void OnUpdateContentSize(float value)
        {
            topContentsLayoutElement.preferredHeight = topContentsLayoutElement.minHeight = value;
        }

        void OnCompleteDeleteCell()
        {
            requestedPanel.SetActive(false);
        }

        public void HideFriendshipRequest()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                return;
            }

            AUIManager.Instance.ShowLoadingSpinner();

            FASFriendship.HideFriendshipRequest(this.User.Id, (request, error) =>
            {
                AUIManager.Instance.HideLoadingSpinner();

                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });
                }
                else
                {
                    StartCoroutine(FadeOutRequestedPanel());

                    if (AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count > 0)
                        AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count--;
                }
            });
        }
    }
}
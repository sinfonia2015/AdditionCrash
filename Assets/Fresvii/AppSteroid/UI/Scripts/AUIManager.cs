using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Fresvii.AppSteroid.Util;

namespace Fresvii.AppSteroid.UI
{
    public class AUIManager : MonoBehaviour
    {
        public static AUIManager Instance;

        public static event Action OnScreenSizeChanged;

        public static event Action OnEscapeTapped;

        public Transform FramesNode;

        public RectTransform sizedCanvas;

        public GameObject[] topNodes;

        public GameObject[] topFrames;

        public GameObject groupConferenceTopNode;

        public GameObject prfbGroupConference;

        public GameObject matchMakingTopNode;

        public GameObject prfbMatchMaking;

        public GameObject loadingSpinner;

        public Image loadingSpinnerDarken;

        private AUITabBar.TabButton selectedTab = AUITabBar.TabButton.None;

        private Fresvii.AppSteroid.Util.DeviceRotationSetting initDeviceRotationSetting;

        public AUICanvasScaleManager auiCanvasScaleManager;

        public bool canBackButton = true;

        public bool fontUp;

        public bool Initialized { get; protected set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);

                return;
            }

            Instance = this;

            AUITabBar.OnTabButtonClicked += OnTabButtonClicked;

            Application.targetFrameRate = 60;

#if UNITY_5 && UNITY_IOS && !UNITY_EDITOR
            fontUp = (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone5 || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone5C || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone5S || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone4S || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone4);
#elif UNITY_IOS && !UNITY_EDITOR
            fontUp = (iPhone.generation == iPhoneGeneration.iPhone5 || iPhone.generation == iPhoneGeneration.iPhone5C || iPhone.generation == iPhoneGeneration.iPhone5S || iPhone.generation == iPhoneGeneration.iPhone4S || iPhone.generation == iPhoneGeneration.iPhone4);
#endif


        }

        void OnEnable()
        {
            FASEvent.OnTapForumCommentCreated += ShowThreadOfComment;

            FASGui.OnShowGroupConferenceGui += ShowGroupConferenceGUI;

            FASEvent.OnUserAgreementRequired += ShowUserAgreementDialog;

            FASEvent.OnUserBanned += OnUserBanned;

            FASEvent.OnFriendshipRequestCreated += OnFriendshipRequestCreated;

            FASEvent.OnDirectMessageCreated += OnDirectMessageCreated;

            FASEvent.OnGroupMessageCreated += OnGroupMessageCreated;

            FASEvent.OnTapGroupMessageCreated += OnTapGroupMessageCreated;

            FASEvent.OnTapDirectMessageCreated += OnTapDirectMessageCreated;

            FASEvent.OnTapFriendshipRequestCreated += OnTapFriendshipRequestCreated;
        }

        void OnDisable()
        {
            FASEvent.OnTapForumCommentCreated -= ShowThreadOfComment;

            FASGui.OnShowGroupConferenceGui -= ShowGroupConferenceGUI;

            FASEvent.OnUserAgreementRequired -= ShowUserAgreementDialog;

            FASEvent.OnUserBanned -= OnUserBanned;

            FASEvent.OnFriendshipRequestCreated -= OnFriendshipRequestCreated;

            FASEvent.OnDirectMessageCreated -= OnDirectMessageCreated;

            FASEvent.OnGroupMessageCreated -= OnGroupMessageCreated;

            FASEvent.OnTapGroupMessageCreated -= OnTapGroupMessageCreated;

            FASEvent.OnTapDirectMessageCreated -= OnTapDirectMessageCreated;

            FASEvent.OnTapFriendshipRequestCreated -= OnTapFriendshipRequestCreated;
        }

        void OnDestroy()
        {
            Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

            HideLoadingSpinner();

            AUITabBar.OnTabButtonClicked -= OnTabButtonClicked;

            Fresvii.AppSteroid.Util.DeviceRotationControll.Set(initDeviceRotationSetting);
        }

        IEnumerator Start() 
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), delegate(bool del) { });

                Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                HideLoadingSpinner();

                FASGui.BackToGameScene();

                yield break;
            }

            // View (tab view) Set up
            if (FASGui.mode == FASGui.Mode.All)
            {
                FASGui.mode -= FASGui.Mode.MatchMaking;

                FASGui.mode -= FASGui.Mode.GroupConference;
            }

            if ((FASGui.mode & FASGui.Mode.MatchMaking) > 0)  // MatchMaking is exclusive value.
            {
                FASGui.mode &= FASGui.Mode.MatchMaking;

                ShowMatchMakingGUI();
            }

            if ((FASGui.mode & FASGui.Mode.GroupConference) > 0)  // GroupConference is exclusive value.
            {
                FASGui.mode &= FASGui.Mode.GroupConference;

                ShowGroupConferenceGUI();
            }
            
            SetTabButtons();

            SelectTab(FASGui.SelectedMode);

            selectedTab = AUITabBar.Instance.SelectedTabButton;

            initDeviceRotationSetting = new Fresvii.AppSteroid.Util.DeviceRotationSetting();

            initDeviceRotationSetting.orientation = ScreenOrientation.AutoRotation;

            initDeviceRotationSetting.portrait = Screen.autorotateToPortrait;

            initDeviceRotationSetting.portraitUpsideDown = Screen.autorotateToPortraitUpsideDown;

            initDeviceRotationSetting.landscapeLeft = Screen.autorotateToLandscapeLeft;

            initDeviceRotationSetting.landscapeRight = Screen.autorotateToLandscapeRight;

            Fresvii.AppSteroid.Util.DeviceRotationControll.Set(new Fresvii.AppSteroid.Util.DeviceRotationSetting(FASConfig.Instance.orientation, FASConfig.Instance.portrait, FASConfig.Instance.portraitUpsideDown, FASConfig.Instance.landscapeRight, FASConfig.Instance.landscapeLeft));    

            yield return 1;

            while (!FAS.Initialized)
            {
                yield return 1;
            }

            bool waitLogin = false;

            bool retryLogin = false;

            if (!string.IsNullOrEmpty(FASGui.LoginUserId) && !string.IsNullOrEmpty(FASGui.LoginUserToken) && !FASUser.IsLoggedIn())
            {
                waitLogin = true;

                FASUser.LogIn(FASGui.LoginUserId, FASGui.LoginUserToken, (error) =>
                {
                    if (error != null && error.Code != (int)Fresvii.AppSteroid.Models.Error.ErrorCode.Banned)
                    {
                        Debug.LogError(error.ToString());

#if !UNITY_EDITOR
						Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

						Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("LoginErrorAndRetry"), (retry) =>
                        {
                            if (retry)
                            {
								retryLogin = true;

								waitLogin = false;

								StartCoroutine(Start());
                            }
                            else
                            {
								Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                                FASGui.LoginUserId = "";

                                FASGui.LoginUserToken = "";

                                FASGui.BackToGameScene();
                            }
                        });					
#else
                        Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                        FASGui.LoginUserId = "";

                        FASGui.LoginUserToken = "";

                        FASGui.BackToGameScene();
#endif
                    }
                    else
                    {
                        FASUser.GetAccount((u, e) => { });

                        waitLogin = false;
                    }
                });
            }
            else if (FASUser.LoadSignedUpUsers().Count == 0)
            {
                waitLogin = true;

                FASUser.SignUp((user, e1) =>
                {
                    if (e1 != null)
                    {
                        Debug.LogError("Sign up error " + e1.ToString());
#if !UNITY_EDITOR
						Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

						Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("LoginErrorAndRetry"), (retry) =>
                        {
                            if (retry)
                            {
								retryLogin = true;

								waitLogin = false;

								StartCoroutine(Start());
                            }
                            else
                            {
								Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                                FASGui.LoginUserId = "";

                                FASGui.LoginUserToken = "";

                                FASGui.BackToGameScene();
                            }
                        });					
#else
                        Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                        FASGui.LoginUserId = "";

                        FASGui.LoginUserToken = "";
                                                
                        FASGui.BackToGameScene();
#endif
                    }
                    else
                    {
                        FASUser.LogIn(user.Id, user.Token, (e2) =>
                        {
                            if (e2 != null)
                            {
                                Debug.LogError("Log in error " + e2.ToString());
#if !UNITY_EDITOR
        						Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

						        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("LoginErrorAndRetry"), (retry) =>
                                {
                                    if (retry)
                                    {
								        retryLogin = true;

								        waitLogin = false;

								        StartCoroutine(Start());
                                    }
                                    else
                                    {
								        Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                                        FASGui.LoginUserId = "";

                                        FASGui.LoginUserToken = "";

                                        FASGui.BackToGameScene();
                                    }
                                });					
#else
                                Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                                FASGui.LoginUserId = "";

                                FASGui.LoginUserToken = "";

                                FASGui.BackToGameScene();
#endif
                            }
                            else
                            {
                                FASUser.GetAccount((u, e) => { });

                                waitLogin = false;
                            }
                        });
                    }
                });
            }
            else if ((string.IsNullOrEmpty(FASGui.LoginUserId) || string.IsNullOrEmpty(FASGui.LoginUserToken)) && !FASUser.IsLoggedIn())
            {
                waitLogin = true;

                FASUser.RelogIn((error) =>
                {
                    if (error != null && error.Code != (int)Fresvii.AppSteroid.Models.Error.ErrorCode.Banned && error.Code != (int)Fresvii.AppSteroid.Models.Error.ErrorCode.OtherLoginProcessIsRunning && error.Code != (int)Fresvii.AppSteroid.Models.Error.ErrorCode.OtherSignUpProcessIsRunning)
                    {
                        Debug.LogError(error.ToString());
#if !UNITY_EDITOR
						Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

						Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("LoginErrorAndRetry"), (retry) =>
                        {
                            if (retry)
                            {
								retryLogin = true;

								waitLogin = false;

								StartCoroutine(Start());
                            }
                            else
                            {
								Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                                FASGui.LoginUserId = "";

                                FASGui.LoginUserToken = "";

                                FASGui.BackToGameScene();
                            }
                        });					
#else
                        Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                        FASGui.LoginUserId = "";

                        FASGui.LoginUserToken = "";
                                                
                        FASGui.BackToGameScene();
#endif
                    }
                    else
                    {
                        FASUser.GetAccount((u, e) => { });

                        waitLogin = false;
                    }
                });
            }

            while (waitLogin)
            {
                yield return 1;
            }

            if (retryLogin || !FASUser.IsLoggedIn())
            {
                yield break;
            }

            Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

            FASGui.LoginUserId = "";

            FASGui.LoginUserToken = "";

            Initialized = true;
        }

        IEnumerator GetStickerSet(IList<Fresvii.AppSteroid.Models.StickerSet> stickerSets)
        {
            foreach (var stickerSet in stickerSets)
            {
                bool processing = true;

                FASSticker.GetStickerSet(stickerSet.Id, (_stickerSet, _error) =>
                {
                    if (this == null) return;

                    if (_error == null)
                    {
                        StartCoroutine(GetStickers(_stickerSet.Stickers, () =>
                        {
                            processing = false;
                        }));
                    }
                    else
                    {
                        processing = false;

                        Debug.LogError(_error.ToString());
                    }
                });

                while (processing)
                {
                    yield return 1;
                }
            }
        }

        IEnumerator GetStickers(List<Fresvii.AppSteroid.Models.Sticker> stickers, Action callback)
        {
            foreach (var sticker in stickers)
            {
                bool processing = true;

                ResourceManager.Instance.DownloadTextureToCache(sticker.Url, (success) =>
                {
                    processing = false;
                });

                yield return 1;

                while (processing)
                {
                    yield return 1;
                }
            }

            callback();
        }

        void LogIn(string userid, string token)
        {
            FASUser.LogIn(FASGui.LoginUserId, FASGui.LoginUserToken, (e) =>
            {
                if (e != null)
                {
                    LogIn(userid, token);
                }
            });
        }

        void SetTabButtons()
        {
            if ((FASGui.mode & FASGui.Mode.Forum) > 0)
            {
                AUITabBar.Instance.SetTabButtonEnable(AUITabBar.TabButton.Forum);
            }

            if ((FASGui.mode & FASGui.Mode.Leaderboards) > 0)
            {
                AUITabBar.Instance.SetTabButtonEnable(AUITabBar.TabButton.Leaderboards);
            }

            if ((FASGui.mode & FASGui.Mode.MyProfile) > 0)
            {
                AUITabBar.Instance.SetTabButtonEnable(AUITabBar.TabButton.MyPage);
            }

            if ((FASGui.mode & FASGui.Mode.GroupMessage) > 0)
            {
                AUITabBar.Instance.SetTabButtonEnable(AUITabBar.TabButton.Messages);
            }

            if ((FASGui.mode & FASGui.Mode.MoreApps) > 0)
            {
                AUITabBar.Instance.SetTabButtonEnable(AUITabBar.TabButton.MoreApps);
            }

        }

        void SelectTab(FASGui.Mode selected)
        {
            if (selected == FASGui.Mode.Forum)
            {
                AUITabBar.Instance.OnSelected(AUITabBar.TabButton.Forum);
            }
            else if (selected == FASGui.Mode.Leaderboards)
            {
                AUITabBar.Instance.OnSelected(AUITabBar.TabButton.Leaderboards);
            }
            else if (selected == FASGui.Mode.GroupMessage)
            {
                AUITabBar.Instance.OnSelected(AUITabBar.TabButton.Messages);
            }
            else if (selected == FASGui.Mode.MyProfile)
            {
                AUITabBar.Instance.OnSelected(AUITabBar.TabButton.MyPage);
            }
            else
            {
                AUITabBar.Instance.OnSelected(AUITabBar.TabButton.MoreApps);
            }
        }

        float postScreenWidth = Screen.width;

		float postScreenHeight = Screen.height;

		DeviceOrientation postDeviceOrientation = DeviceOrientation.Unknown;

#if GROUP_CONFERENCE
        private bool showVoiceChatNotification = false;
#endif
        public RectTransform notification;

        public Text notificationText;

        void Update()
        {
			if (postScreenWidth != Screen.width || postScreenHeight != Screen.height || Input.deviceOrientation != postDeviceOrientation)
            {
                auiCanvasScaleManager.SetCanvasScale();

                if (OnScreenSizeChanged != null)
                {                    
                    OnScreenSizeChanged();
                }

                postScreenWidth = Screen.width;

				postScreenHeight = Screen.height;

				postDeviceOrientation = Input.deviceOrientation;
            }

            if (Input.GetKeyUp(KeyCode.Escape) && OnEscapeTapped != null && canBackButton)
            {
                OnEscapeTapped();
            }

#if GROUP_CONFERENCE
            if (showVoiceChatNotification != FASConference.IsCalling())
            {
                showVoiceChatNotification = !showVoiceChatNotification;

                notification.gameObject.SetActive(showVoiceChatNotification);

                FramesNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, (showVoiceChatNotification) ? - notification.rect.height * 0.5f : 0f);

                if (showVoiceChatNotification)
                {
                    FramesNode.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, -notification.rect.height);
                }
                else
                {
                    FramesNode.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
                }
            }

            if (showVoiceChatNotification)
            {
                notificationText.color = new Color(notificationText.color.r, notificationText.color.g, notificationText.color.b, 0.5f + 0.5f * Mathf.Cos(Time.time * Mathf.PI));
            }
#endif

        }

        public void OnTapVoiceChatNotification()
        {
            groupConferenceTopNode.SetActive(true);

            groupConferenceTopNode.transform.SetAsLastSibling();

            if (AUIGroupConference.instance != null)
            {
                for (int i = groupConferenceTopNode.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = groupConferenceTopNode.transform.GetChild(i);

                    if (child != AUIGroupConference.instance.transform && child != AUITabBar.Instance.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }

                AUIGroupConference.instance.transform.SetAsLastSibling();

                AUIGroupConference.instance.frameTween.SetPosition(Vector2.zero);

                return;
            }

            AUIGroupConference groupConference = ((GameObject)Instantiate(prfbGroupConference)).GetComponent<AUIGroupConference>();

            groupConference.transform.SetParent(groupConferenceTopNode.transform, false);

            groupConference.transform.SetAsLastSibling();

            AUIFrame frameTween = null;

            RectTransform rectTransform = FramesNode.GetComponent<RectTransform>();

            for (int i = topNodes[(int)selectedTab].transform.childCount - 1; i >= 0; i--)
            {
                Transform child = topNodes[(int)selectedTab].transform.GetChild(i);

                if (child.gameObject.activeSelf)
                {
                    frameTween = child.gameObject.GetComponent<AUIFrame>();
                }
            }

            groupConference.parentFrameTween = frameTween;

            groupConference.frameTween.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () =>
            {
                if (frameTween != null)
                {
                    frameTween.gameObject.SetActive(false);
                }
            });

            groupConference.OnDone += () => { groupConferenceTopNode.SetActive(false); };
        }

        void OnTabButtonClicked(AUITabBar.TabButton selectedTab)
        {
            if (this.selectedTab == selectedTab)
            {
                GoToTopFrame();
            }
            else
            {
                this.selectedTab = selectedTab;

                for (int i = 0; i < topNodes.Length; i++)
                {
                    if (i == (int)selectedTab)
                    {
                        topNodes[i].SetActive(true);

                        topNodes[i].transform.SetAsLastSibling();
                    }
                    else
                    {
                        topNodes[i].SetActive(false);
                    }
                }
            }

            if (selectedTab == AUITabBar.TabButton.Forum)
            {
                FASGui.SelectedMode = FASGui.Mode.Forum;
            }
            else if (selectedTab == AUITabBar.TabButton.Leaderboards)
            {
                FASGui.SelectedMode = FASGui.Mode.Leaderboards;
            }
            else if (selectedTab == AUITabBar.TabButton.MoreApps)
            {
                FASGui.SelectedMode = FASGui.Mode.MoreApps;
            }
            else if (selectedTab == AUITabBar.TabButton.MyPage)
            {
                FASGui.SelectedMode = FASGui.Mode.MyProfile;
            }
            else if (selectedTab == AUITabBar.TabButton.Messages)
            {
                FASGui.SelectedMode = FASGui.Mode.GroupMessage;
            }
        }

        void GoToTopFrame()
        {
			AUIFrame currentTopFrame = null;

            for (int i = topNodes[(int)selectedTab].transform.childCount - 1; i >= 0; i--)
            {
                Transform child = topNodes[(int)selectedTab].transform.GetChild(i);

				if(currentTopFrame == null && child != topFrames[(int)selectedTab].transform)
				{
                    if(child.gameObject.activeSelf)
    					currentTopFrame = child.GetComponent<AUIFrame>();
				}

                if (child != topFrames[(int)selectedTab].transform && child != AUITabBar.Instance.transform)
                {
                    if (currentTopFrame != null && child != currentTopFrame.transform)
                        Destroy(child.gameObject);
                }
            }

            topFrames[(int)selectedTab].gameObject.SetActive(true);

			topNodes[(int)selectedTab].gameObject.SetActive(true);

			if (currentTopFrame == null || currentTopFrame == topFrames [(int)selectedTab].transform) 
			{
				topFrames [(int)selectedTab].GetComponent<AUIFrame> ().SetPosition (Vector2.zero);
			} 
			else 
			{
				RectTransform rectTransform = topFrames [(int)selectedTab].GetComponent<RectTransform>();

				topFrames [(int)selectedTab].GetComponent<AUIFrame> ().Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () =>
				{

				});

				currentTopFrame.Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () => 
				{
					Destroy(currentTopFrame.gameObject);
				});
			}
        }

        public void ShowLoadingSpinner(bool darken)
        {
            loadingSpinnerDarken.enabled = darken;

            loadingSpinner.SetActive(true);
        }

        public void ShowLoadingSpinner()
        {
            loadingSpinnerDarken.enabled = true;

            loadingSpinner.SetActive(true);
        }

        public void HideLoadingSpinner()
        {
            if(loadingSpinner != null)
                loadingSpinner.SetActive(false);
        }

        public void ShowGroupConferenceGUI()
        {
            groupConferenceTopNode.SetActive(true);

            groupConferenceTopNode.transform.SetAsLastSibling();

            if (AUIGroupConference.instance != null)
            {
                for (int i = groupConferenceTopNode.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = groupConferenceTopNode.transform.GetChild(i);

                    if (child != AUIGroupConference.instance.transform && child != AUITabBar.Instance.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }

                AUIGroupConference.instance.transform.SetAsLastSibling();


                AUIGroupConference.instance.frameTween.SetPosition(Vector2.zero);

                return;
            }

            AUIGroupConference groupConference = ((GameObject)Instantiate(prfbGroupConference)).GetComponent<AUIGroupConference>();

            groupConference.transform.SetParent(groupConferenceTopNode.transform, false);

            groupConference.transform.SetAsLastSibling();

            groupConference.frameTween.SetPosition(Vector2.zero);
        }

        public void ShowMatchMakingGUI()
        {
            if (AUIMatchMaking.Instance != null)
            {
                Destroy(AUIMatchMaking.Instance.gameObject);
            }

            matchMakingTopNode.SetActive(true);

            matchMakingTopNode.transform.SetAsLastSibling();

            AUIMatchMaking matchMaking = ((GameObject)Instantiate(prfbMatchMaking)).GetComponent<AUIMatchMaking>();

            Debug.Log(matchMakingTopNode);

            matchMaking.transform.SetParent(matchMakingTopNode.transform, false);

            matchMaking.transform.SetAsLastSibling();

            matchMaking.frameTween.SetPosition(Vector2.zero);
        }

        public void Show(FASGui.Mode mode, FASGui.Mode selectedTab)
        {
            FASGui.mode = mode;

            FASGui.SelectedMode = selectedTab;

            SetTabButtons();

            SelectTab(selectedTab);
        }

        public GameObject prfbUserAgreement;

        void ShowUserAgreementDialog(Fresvii.AppSteroid.Models.UserAgreement userAgreement)
        {
            int selectedTabIndex = 0;

            for (int i = 0; i < topNodes.Length; i++)
            {
                if (topNodes[i].activeInHierarchy)
                {
                    selectedTabIndex = i;
                }
            }

            var auiUserAgreement = ((GameObject)Instantiate(prfbUserAgreement)).GetComponent<AUIUserAgreement>();

            RectTransform rectTransform = FramesNode.GetComponent<RectTransform>();

            auiUserAgreement.SetUserAgreement(userAgreement, (agree) =>
            {
                topNodes[selectedTabIndex].SetActive(true);

                auiUserAgreement.frame.Animate(Vector2.zero, new Vector2(0f, -rectTransform.rect.height), () => 
                {
                    if (agree)
                    {
                        FASUtility.AcceptUserAgreement((userAgreementState, error) =>
                        {
                            if (error != null)
                            {
                                Debug.LogError("EULA Accept Error: " + error.ToString());
                            }
                            else
                            {
                                Debug.Log("EULA Accepted");
                            }
                        });
                    }
                    else
                    {
                        FASUtility.DeclineUserAgreement((userAgreementState, error) => { });

                        FASUser.LogOut();

                        FASGui.BackToGameScene();
                    }

                    Destroy(auiUserAgreement.gameObject);
                });
            });

            auiUserAgreement.transform.SetParent(FramesNode.transform.parent, false);

            auiUserAgreement.transform.SetAsLastSibling();

            auiUserAgreement.frame.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () => 
            {
                topNodes[selectedTabIndex].SetActive(false);
            });
        }

        void OnUserBanned(string message)
        {
            Debug.LogWarning(message);

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(message, FASText.Get("Close"), FASText.Get("Close"), FASText.Get("Close"), (del) =>
            {
                FASUser.LogOut();
            });

            FASGui.BackToGameScene();
        }

        public void ShowThreadOfComment(Fresvii.AppSteroid.Models.Comment comment)
        {
            if ((FASGui.mode & FASGui.Mode.Forum) > 0)
            {
                SelectTab(FASGui.Mode.Forum);

                GoToTopFrame();

                topFrames[(int)AUITabBar.TabButton.Forum].GetComponent<AUICommunityTop>().GoToThread(comment.ThreadId, comment, false);
            }
        }

        void OnFriendshipRequestCreated(Fresvii.AppSteroid.Models.User user)
        {
            FASUser.GetAccount((_user, _error) =>
            {
                if (_error == null)
                {
                    AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count = FAS.CurrentUser.FriendRequestsCount;
                }
            });
        }

        void OnTapFriendshipRequestCreated(Fresvii.AppSteroid.Models.User user)
        {
            if ((FASGui.mode & FASGui.Mode.MyProfile) > 0)
            {
                AUIMyPage.StartPage = AUIMyPage.Page.FriendRequest;

                SelectTab(FASGui.Mode.MyProfile);

                GoToTopFrame();
            }
        }

        void OnDirectMessageCreated(Fresvii.AppSteroid.Models.DirectMessage dm)
        {
            AUITabBar.Instance.AddUnreadDirectMessage(dm);            
        }

        void OnGroupMessageCreated(Fresvii.AppSteroid.Models.GroupMessage gm)
        {
            AUITabBar.Instance.AddUnredGroup(gm.GroupId);
        }

        void OnTapGroupMessageCreated(Fresvii.AppSteroid.Models.GroupMessage gm)
        {
            ShowGroupMessages(gm);
        }

        public void ShowGroupMessages(Fresvii.AppSteroid.Models.GroupMessage gm)
        {
            if ((FASGui.mode & FASGui.Mode.GroupMessage) > 0)
            {
                SelectTab(FASGui.Mode.GroupMessage);

                GoToTopFrame();

                FASGroup.GetGroup(gm.GroupId, (group, error) =>
                {
                    if (error == null)
                    {
                        topFrames[(int)AUITabBar.TabButton.Messages].GetComponent<AUIMessageList>().GoToMessage(group, false);
                    }
                    else
                    {
                        Debug.LogError(error.ToString());
                    }
                });
            }
        }

        void OnTapDirectMessageCreated(Fresvii.AppSteroid.Models.DirectMessage dm)
        {
            if ((FASGui.mode & FASGui.Mode.GroupMessage) > 0)
            {
                SelectTab(FASGui.Mode.GroupMessage);

                GoToTopFrame();

                topFrames[(int)AUITabBar.TabButton.Messages].GetComponent<AUIMessageList>().GoToDirectMessage(null, null, false);
            }
        }
    }
}
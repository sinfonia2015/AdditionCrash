using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGroupConference : MonoBehaviour
    {
        public static AUIGroupConference instance;

        public AUIFrame frameTween;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        private Fresvii.AppSteroid.Models.Group group;

        private static Fresvii.AppSteroid.Models.GroupConference groupConference;

#if GROUP_CONFERENCE
        private FASConference.ConferenceStates conferenceState = FASConference.ConferenceStates.None;

        private FASConference.CallStates myCallState = FASConference.CallStates.Idle;
#endif

        private static System.DateTime connectedDateTime;

        public GameObject userIcon2, userIcon3, userIcon4;

        public AUIRawImageTextureSetter[] userIcons2, userIcons3, userIcons4;

        public Text[] userNames2, userNames3, userNames4;

        public Text callStateText;

        public Button buttonCallStart, buttonCallEnd;

        public event Action OnDone;

        public AUIRawImageTextureSetter bgImage;

        public void SetGroup(Fresvii.AppSteroid.Models.Group group)
        {
            this.group = group;
        }

        #if GROUP_CONFERENCE

        void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
            }

            instance = this;
        }

        IEnumerator Start()
        {
            yield return 1;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Handheld.StopActivityIndicator();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();
            }

            bool groupGetting = false;

            buttonCallStart.gameObject.SetActive(false);

            buttonCallEnd.gameObject.SetActive(false);

            //--------------------------------------------------------------
            //  it is connecting some group, so that fetch the group by id
            //--------------------------------------------------------------
            if (!string.IsNullOrEmpty(FASConference.GetGroupId()))
            {
                AUIManager.Instance.ShowLoadingSpinner();

                groupGetting = true;

                FASGroup.GetGroup(FASConference.GetGroupId(), (group, error) =>
                {
                    groupGetting = false;

                    AUIManager.Instance.HideLoadingSpinner();

                    if (error != null)
                    {

                    }
                    else
                    {
                        this.group = group;
                    }
                });
            }

            while (groupGetting)
            {
                yield return 1;
            }

            //--------------------------------------------------------------
            //  Get group members information
            //--------------------------------------------------------------
            if (this.group != null)
            {
                if (group.Members.Count == 0)
                {
                    GroupFetchMembers();
                }
                else
                {
                    SetGroupMemberIcon();
                }

                FASUtility.SendPageView("pv.messages.call", this.group.Id, System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                        Debug.LogError(e.ToString());
                });    
            }
            else // Group is not settle
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"), (del) => { });

                yield break;
            }

            if (!(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
            {
                Debug.LogWarning("Voice chat is unable on Editor");

                myCallState = FASConference.CallStates.Calling;

                SetCallStateUI();

                yield break;
            }

            //buttonCallEnd.gameObject.SetActive(true);

            if (FASConference.IsConnected())
            {
                myCallState = FASConference.CallStates.Connected;

                SetCallStateUI();
            }
            else
            {
                myCallState = FASConference.CallStates.Calling;

                connectedDateTime = System.DateTime.MinValue;

                FASConference.GetCurrentConference(group.Id, (_groupConference, _error) =>
                {
                    groupConference = _groupConference;

                    if (_error == null)
                    {
                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
                        {
                            Debug.Log("GetCurrentConference: current conference exists");
                        }

                        FASConference.JoinConference(groupConference, FAS.OnVoiceChatError);
                    }
                    else
                    {
                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
                        {
                            Debug.Log("GetCurrentConference: Start new conference");
                        }

                        FASConference.StartConference(group.Id, FAS.OnVoiceChatError, FAS.OnVoiceChatError);
                    }

                    SetCallStateUI();
                });
            }

            SetMicrophoneIcon();
        }

        void OnEnable()
        {
            FASConference.OnCallStateChanged += OnCallStateChanged;

            FASConference.OnConferenceStateChanged += OnConferenceStateChanged;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Fresvii.AppSteroid.Util.Sensor.Init();

                Fresvii.AppSteroid.Util.Sensor.OnProximitySensorChanged += OnProximitySensorChanged;
            }
        }


        void OnCallStateChanged(string groupId, string userId, FASConference.CallStates callState)
        {
            if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
            {
                Debug.Log("OnCallStateChanged : " + callState.ToString() + " userId : " + userId);
            }

            if (callState == FASConference.CallStates.Connected)
            {
                FASConference.GetGroupConferenceParticipants(groupId, (participants, meta, error) =>
                {
                    if (error == null)
                    {
                        if (participants.Count >= 2)
                        {
                            foreach (Fresvii.AppSteroid.Models.GroupConferenceParticipant participant in participants)
                            {
                                if (participant.Id == FAS.CurrentUser.Id)
                                {
                                    myCallState = FASConference.CallStates.Connected;

                                    break;
                                }
                            }

                            if (myCallState == FASConference.CallStates.Connected)
                            {
                                if (connectedDateTime == System.DateTime.MinValue)
                                {
                                    connectedDateTime = System.DateTime.Now;
                                }
                            }
                            else
                            {
                                connectedDateTime = System.DateTime.MinValue;
                            }

                            if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
                            {
                                Debug.Log("OnCallStateChanged myCallState: " + myCallState.ToString());
                            }
                        }
                        else
                        {
                            myCallState = FASConference.CallStates.Idle;
                        }
                    }
                    else
                    {
                        myCallState = FASConference.CallStates.Idle;
                    }

                    SetCallStateUI();

                });
            }
            else
            {
                myCallState = FASConference.CallStates.Idle;

                SetCallStateUI();
            }
        }

        void SetCallStateUI()
        {
            if (myCallState == FASConference.CallStates.Idle)
            {
                callStateText.text = "";

                buttonCallStart.gameObject.SetActive(true);

                buttonCallEnd.gameObject.SetActive(false);
            }
            else if (myCallState == FASConference.CallStates.Calling || myCallState == FASConference.CallStates.Progressing)
            {
                callStateText.text = FASText.Get("Calling");

                buttonCallStart.gameObject.SetActive(false);

                buttonCallEnd.gameObject.SetActive(true);
            }
            else 
            {
                callStateText.text = FASText.Get("Connected");

                buttonCallStart.gameObject.SetActive(false);

                buttonCallEnd.gameObject.SetActive(true);
            }

        }

        void OnConferenceStateChanged(FASConference.ConferenceStates conferenceState)
        {
            if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
            {
                Debug.Log("OnConferenceStateChanged : " + conferenceState.ToString());
            }

            this.conferenceState = conferenceState;

            if (this.conferenceState == FASConference.ConferenceStates.Destroyed)
            {
                myCallState = FASConference.CallStates.Idle;

                connectedDateTime = System.DateTime.MinValue;
            }
        }

        private bool controlLock = false;

        void OnProximitySensorChanged(float value)
        {
            controlLock = (value == 0.0f);
        }

        void OnDisable()
        {
            FASConference.OnCallStateChanged -= OnCallStateChanged;

            FASConference.OnConferenceStateChanged -= OnConferenceStateChanged;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Fresvii.AppSteroid.Util.Sensor.OnProximitySensorChanged -= OnProximitySensorChanged;

                Fresvii.AppSteroid.Util.Sensor.Release();
            }

            AUIManager.Instance.HideLoadingSpinner();
        }

        void GroupFetchMembers()
        {
            group.FetchMembers(OnFetchMembers);
        }

        void OnFetchMembers(Fresvii.AppSteroid.Models.Error error)
        {
            if (this.gameObject == null || !this.gameObject.activeInHierarchy)
            {
                return;
            }

            if (error == null)
            {
                SetGroupMemberIcon();
            }
            else
            {
                Invoke("GroupFetchMembers", 3f);
            }
        }

        void SetGroupMemberIcon()
        {
            userIcon2.SetActive(group.Members.Count == 2);

            userIcon3.SetActive(group.Members.Count == 3);

            userIcon4.SetActive(group.Members.Count > 3);

            AUIRawImageTextureSetter[] userIcons;

            Text[] userNames;

            if (group.Members.Count == 2)
            {
                userIcons = userIcons2;

                userNames = userNames2;
            }
            else if (group.Members.Count == 3)
            {
                userIcons = userIcons3;

                userNames = userNames3;
            }
            else
            {
                userIcons = userIcons4;

                userNames = userNames4;
            }

            for (int i = 0; i < group.Members.Count; i++)
            {
                if (i == 4) break;

                userIcons[i].Set(group.Members[i].ProfileImageUrl);

                userNames[i].text = group.Members[i].Name;
            }

            bgImage.Set(group.Members[0].ProfileImageUrl);
        }

        public void OnClickCallEnd()
        {
            if (controlLock) return;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                FASConference.Leave();
            }

            myCallState = FASConference.CallStates.Idle;

            SetCallStateUI();
        }

        public void OnClickCallStart()
        {
            if (controlLock) return;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                FAS.Instance.Client.GroupConferenceService.GetCurrentConference(group.Id, (_groupConference, _error)=>
                {
                    if (_error == null)
                    {
                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
                        {
                            Debug.Log("GetCurrentConference: current conference exists");
                        }

                        FAS.Instance.Client.GroupConferenceService.DeleteConference(group.Id, (_error2)=>
                        {
                            if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
                            {
                                Debug.Log("DeleteConference: current conference delete process done");
                            }

                            FASConference.StartConference(group.Id, FAS.OnVoiceChatError, FAS.OnVoiceChatError);
                        });
                    }
                    else
                    {
                        FASConference.StartConference(group.Id, FAS.OnVoiceChatError, FAS.OnVoiceChatError);
                    }
                });
            }

            myCallState = FASConference.CallStates.Calling;

            SetCallStateUI();
        }

        private static bool isMute;

        public void OnClickMicrophone()
        {
            if (!controlLock)
            {
                isMute = !isMute;

                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {

                    if (isMute)
                    {
                        FASConference.Mute();
                    }
                    else
                    {
                        FASConference.Unmute();
                    }
                }

                SetMicrophoneIcon();
            }
        }

        public Button buttonMicrophone;

        public Sprite spriteMicrophoneOn, spriteMicrophoneOff;

        void SetMicrophoneIcon()
        {
            buttonMicrophone.image.sprite = (isMute) ? spriteMicrophoneOff : spriteMicrophoneOn;
        }

        public void Done()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (parentFrameTween != null)
            {
                parentFrameTween.gameObject.SetActive(true);

                parentFrameTween.SetPosition(Vector2.zero);

                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(0f, -rectTransform.rect.height), () =>
                {
                    if (OnDone != null)
                    {
                        OnDone();
                    }

                    Destroy(this.gameObject);
                });
            }
            else
            {
                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(0f, -rectTransform.rect.height), () =>
                {
                    transform.parent.SetAsFirstSibling();

                    if (OnDone != null)
                    {
                        OnDone();
                    }

                    Destroy(this.gameObject);

                    AUIManager.Instance.Show(FASGui.Mode.All, FASGui.Mode.Forum);
                });
            }
        }

        public GameObject prfbMessages;

        public void OnClickMessage()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIMessages messagesPage = ((GameObject)Instantiate(prfbMessages)).GetComponent<AUIMessages>();

            messagesPage.Group = group;

            messagesPage.isModal = true;

            messagesPage.downBackButtonText.text = FASText.Get("Back");

            messagesPage.transform.SetParent(transform.parent, false);

            messagesPage.transform.SetAsLastSibling();

            messagesPage.parentFrameTween = this.frameTween;

            messagesPage.frame.Animate(new Vector2(0f, - rectTransform.rect.height), Vector2.zero, () => { });
        }
#endif
    }
}

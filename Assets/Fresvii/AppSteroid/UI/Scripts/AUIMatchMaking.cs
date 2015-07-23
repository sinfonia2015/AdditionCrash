using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMatchMaking : MonoBehaviour
    {
        public static AUIMatchMaking Instance;

        public enum Status { Initializing, Setting, Matching, None };

        public Status State = Status.Initializing;

        public AUIFrame frameTween;

        private AUIFrame parentFrameTween;

        public AUIRawImageTextureSetter bgImage;

        public RectTransform prfbPlayerCell;

        private List<AUIMatchmakingPlayerCell> cells = new List<AUIMatchmakingPlayerCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        public AUIScrollViewContents contents;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public GameObject goMatchmakingStart;

        public GameObject goMatchmakingCancel;

        public List<Fresvii.AppSteroid.Models.User> selectedUsers;

        private FASMatchMaking.Recipient recipient;

        private Fresvii.AppSteroid.Models.MatchMakingRequest matchMakingRequest;

        private Fresvii.AppSteroid.Models.Match match;

        private Fresvii.AppSteroid.Models.MatchMakingInvitation invitaion;

        private uint? minNumberOfPlayers = 2;

        private uint? maxNumberOfPlayers = 2;

        private string[] inviteUsers;

        private string invitationMessage;

        private string segment;

        public AUIProgressBar progressBar;

        public static bool IsShowing = false;

        void OnEnable()
        {
            FASEvent.OnMatchMakingMatchUpdated += OnMatchMakingMatchUpdated;

            FASEvent.OnMatchMakingGameContextCreated += OnMatchMakingGameContextCreated;

            FASMatchMaking.latestGameContext = null;

            FASGui.MatchmakingIsShowing = true;

            IsShowing = true;
        }

        void OnDestroy()
        {
            FASGui.MatchmakingIsShowing = false;

            IsShowing = false;
        }

        void OnDisable()
        {
            FASEvent.OnMatchMakingMatchUpdated -= OnMatchMakingMatchUpdated;

            FASEvent.OnMatchMakingGameContextCreated -= OnMatchMakingGameContextCreated;

            AUIManager.Instance.HideLoadingSpinner();
        }

        void Awake()
        {
            Instance = this;
        }
        
        IEnumerator Start()
        {
            goMatchmakingStart.SetActive(false);

            goMatchmakingCancel.SetActive(false);

            AUITabBar.Instance.gameObject.SetActive(false);

            progressBar.gameObject.SetActive(false);

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (FAS.CurrentUser == null)
            {
                yield return 1;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                yield break;
            }

            if (FASGui.invitedMatch != null)
            {
                this.match = FASGui.invitedMatch;

                FASGui.invitedMatch = null;

                this.invitaion = FASGui.matchMakingInvitation;

                FASGui.matchMakingInvitation = null;

                this.minNumberOfPlayers = (uint)match.CurrentMinPlayers;

                this.maxNumberOfPlayers = (uint)match.CurrentMaxPlayers;

                State = Status.Matching;
            }
            else
            {
                this.minNumberOfPlayers = FASGui.MatchMakingMinNumberOfPlaysers;

                this.maxNumberOfPlayers = FASGui.MatchMakingMaxNumberOfPlaysers;

                this.inviteUsers = FASGui.MatchMakingInviteUsers;

                this.invitationMessage = FASGui.MatchMakingInvitationMessage;

                this.segment = FASGui.MatchMakingSegment;

                this.recipient = FASGui.MatchMakingRecipient;
            }

            AUIManager.Instance.ShowLoadingSpinner();

            if (this.match == null)
            {
                FASMatchMaking.GetMatchMakingRequest(OnGetMatchMakingRequest);
            }
            else // invitied match
            {
                AUIManager.Instance.HideLoadingSpinner();

                UpdatePlayers();

                State = Status.Matching;

                goMatchmakingStart.SetActive(false);

                goMatchmakingCancel.SetActive(true);

                StartCoroutine(MatchMakingGetMatchPolling(this.match.Id));

                progressBar.gameObject.SetActive(true);
            }

            yield return 1;

            FASUser.GetAccount((u, e) =>
            {
                if (e == null)
                {
                    for (int i = 0; i < this.maxNumberOfPlayers; i++)
                    {
                        if (i == 0)
                        {
                            CreatePlayerCells(new Fresvii.AppSteroid.Models.Player(FAS.CurrentUser));
                        }
                        else
                        {
                            CreatePlayerCells(new Fresvii.AppSteroid.Models.Player());
                        }
                    }
                }
            });
        }

        public Text subTitle;

        void SetSubTitle(uint min, uint max)
        {
            if (min == max)
            {
                subTitle.text = min.ToString() + " " + FASText.Get("PlayersAvailable");
            }
            else
            {
                subTitle.text = min.ToString() + " " + FASText.Get("to") + " " + max.ToString() + " " + FASText.Get("PlayersAvailable");
            }

        }

        public bool CreatePlayerCells(Fresvii.AppSteroid.Models.Player player)
        {
            if (cells.Count == maxNumberOfPlayers)
            {
                AUIMatchmakingPlayerCell cell = cells.Find(x => x.Player.User.Id == player.User.Id && !string.IsNullOrEmpty(x.Player.User.Id));

                if (cell != null)
                {
                    cell.SetPlayer(player, this);

                    return false;
                }

                cell = cells.Find(x => string.IsNullOrEmpty(x.Player.User.Id));

                if (cell != null)
                {
                    cell.SetPlayer(player, this);

                    return false;
                }

                return false;
            }
            else
            {
                var item = Instantiate(prfbPlayerCell) as RectTransform;

                contents.AddItem(item);

                AUIMatchmakingPlayerCell cell = item.GetComponent<AUIMatchmakingPlayerCell>();

                cell.SetPlayer(player, this);

                cells.Add(cell);

                cell.gameObject.SetActive(false);

                return true;
            }
        }

        private void OnGetMatchMakingRequest(Fresvii.AppSteroid.Models.MatchMakingRequest matchMakingRequest, Fresvii.AppSteroid.Models.Error error)
        {
            AUIManager.Instance.HideLoadingSpinner();

            if (this == null) return;

            if (error == null)
            {
                this.matchMakingRequest = matchMakingRequest;

                State = Status.Matching;

                goMatchmakingStart.SetActive(false);

                goMatchmakingCancel.SetActive(true);

                StartCoroutine(MatchMakingGetMatchPolling(matchMakingRequest.Match.Id));
            }
            else
            {
                State = Status.Setting;

                goMatchmakingStart.SetActive(true);

                goMatchmakingCancel.SetActive(false);
            }

            SetSubTitle((uint)this.minNumberOfPlayers, (uint)this.maxNumberOfPlayers);

            foreach (var cell in cells)
            {
                cell.UpdateCell();
            }
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

        public GameObject prfbSelectFriends;

        public void GoToSelectFriends()
        {
            if (frameTween.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIMatchmakingSelectFriends selectFriends = ((GameObject)Instantiate(prfbSelectFriends)).GetComponent<AUIMatchmakingSelectFriends>();

            selectFriends.transform.SetParent(transform.parent, false);

            selectFriends.transform.SetAsLastSibling();

            selectFriends.parentFrameTween = this.frameTween;

            selectFriends.cancelButtonText.text = FASText.Get("Cancel");

            selectFriends.maxNumberOfPlayers = (uint)this.maxNumberOfPlayers;

            if (this.selectedUsers != null)
            {
                selectFriends.selectedUsers = this.selectedUsers;
            }

            selectFriends.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });            

            this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            selectFriends.OnSubmit += (_selectedUsers) =>
            {
                if (_selectedUsers != null)
                {
                    this.selectedUsers = _selectedUsers;

                    foreach (var cell in cells)
                    {
                        if (cell.Player.User.Id != FAS.CurrentUser.Id)
                        {
                            cell.Clear();
                        }
                    }

                    int i = 1;

                    foreach (var user in selectedUsers)
                    {
                        if (i >= cells.Count)
                        {
                            break;
                        }

                        cells[i].SetPlayer(new Fresvii.AppSteroid.Models.Player(user), this);

                        i++;
                    }
                }
            };
        }

        public Button buttonMatchStart;

        public void OnClickMatchStart()
        {
            inviteUsers = null;

            if (selectedUsers == null)
            {

            }
            else if (selectedUsers != null || selectedUsers.Count > 0)
            {
                List<string> users = new List<string>();

                foreach (Fresvii.AppSteroid.Models.User selectedUser in selectedUsers)
                {
                    users.Add(selectedUser.Id);

                    inviteUsers = users.ToArray();
                }
            }

            buttonMatchStart.interactable = false;

            if (recipient == FASMatchMaking.Recipient.FriendOnly)
            {
                FASMatchMaking.CreateMatchMakingRequest(minNumberOfPlayers, (uint?)(inviteUsers.Length + 1), inviteUsers, invitationMessage, segment, recipient, OnCreateMatchMakingRequest);

                if (cells.Count > (inviteUsers.Length + 1))
                {
                    for (int i = cells.Count - 1; i >= inviteUsers.Length + 1; i--)
                    {
                        var cell = cells[i];

                        contents.RemoveItem(cell.GetComponent<RectTransform>());

                        cells.Remove(cell);

                        Destroy(cell.gameObject);
                    }

                    contents.ReLayout();
                }
            }
            else
            {
                FASMatchMaking.CreateMatchMakingRequest(minNumberOfPlayers, maxNumberOfPlayers, inviteUsers, invitationMessage, segment, recipient, OnCreateMatchMakingRequest);
            }
        }

        public void OnClickCancelMatchrequest()
        {
            if (invitaion != null)
            {
                AUIManager.Instance.ShowLoadingSpinner();

                FASMatchMaking.DeclineMatchMakingInvitation(invitaion.Id, (e) =>
                {
                    AUIManager.Instance.HideLoadingSpinner();

                    if (e != null)
                    {
                        if (e.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.CannotCancelMatch)
                        {
                            FASGui.BackToGameScene();
                        }
                        else
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                        }

                        Debug.LogError("DeclineMatchMakingInvitation : " + e.ToString());
                    }
                    else
                    {
                        FASGui.BackToGameScene();

                        Debug.Log("Invited match cancelled");
                    }
                });
            }
            else if (matchMakingRequest != null)
            {
                AUIManager.Instance.ShowLoadingSpinner();

                FASMatchMaking.CancelMatchMakingRequest(matchMakingRequest.Id, (e) =>
                {
                    AUIManager.Instance.HideLoadingSpinner();

                    if (e != null)
                    {
                        if (e.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.CannotCancelMatch)
                        {
                            FASGui.BackToGameScene();
                        }
                        else
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (del) => { });
                        }

                        goMatchmakingStart.SetActive(true);

                        goMatchmakingCancel.SetActive(false);

                        Debug.LogError("CancelMatchMakingRequest : " + e.ToString());
                    }
                    else
                    {
                        FASGui.BackToGameScene();

                        Debug.Log("Matchmaking was cancelled");
                    }
                });
            }
            else
            {
                FASGui.BackToGameScene();
            }
        }

        void Update()
        {
            if (progressBar.gameObject.activeInHierarchy)
            {
                progressBar.Set(MatchMakingProgress());
            }
        }

        private void OnCreateMatchMakingRequest(Fresvii.AppSteroid.Models.MatchMakingRequest matchMakingRequest, Fresvii.AppSteroid.Models.Error error)
        {
            if (error == null)
            {
                this.matchMakingRequest = matchMakingRequest;

                if (matchMakingRequest.MinNumberOfPlayers == matchMakingRequest.MaxNumberOfPlayers)
                {
                    subTitle.text = minNumberOfPlayers.ToString() + " " + FASText.Get("PlayersAvailable");
                }
                else
                {
                    subTitle.text = matchMakingRequest.MinNumberOfPlayers.ToString() + " " + FASText.Get("to") + " " + matchMakingRequest.MaxNumberOfPlayers.ToString() + " " + FASText.Get("PlayersAvailable");
                }

                StartCoroutine(MatchMakingGetMatchPolling(matchMakingRequest.Match.Id));

                State = Status.Matching;

                buttonMatchStart.interactable = true;

                goMatchmakingStart.SetActive(false);

                goMatchmakingCancel.SetActive(true);

                progressBar.gameObject.SetActive(true);
            }
            else
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("CreateMatchMakingRequestError"), (del) => { });

                if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError("CreateMatchMakingRequest Error : " + error.ToString());
                }

                State = Status.Matching;

                buttonMatchStart.interactable = true;
            }
        }

        public float pollingInterval = 3f;

        IEnumerator MatchMakingGetMatchPolling(string matchId)
        {
            bool polling = true;

            while (this.gameObject != null && this.gameObject.activeInHierarchy && polling)
            {
                // GetMatch
                FASMatchMaking.GetMatch(matchId, (match, error) =>
                {
                    if (this == null || !polling) return;

                    if (error == null && match != null)
                    {
                        OnMatchMakingMatchUpdated(match);

                        if (match.Status == Fresvii.AppSteroid.Models.Match.Statuses.Complete)
                        {
                            FASEvent.OnMatchMakingCompleteByPolling(match);
                        }
                        else if (match.Status == Fresvii.AppSteroid.Models.Match.Statuses.Disposed)
                        {
                            MatchDisposed();

                            polling = false;
                        }
                    }
                    else
                    {
                        MatchDisposed();

                        polling = false;
                    }
                });

                yield return new WaitForSeconds(pollingInterval);
            }
        }

        public void OnMatchMakingMatchUpdated(Fresvii.AppSteroid.Models.Match match)
        {
            if (matchDisposed) return;

            if (match.Status == Fresvii.AppSteroid.Models.Match.Statuses.Disposed)
            {
                matchDisposed = true;

                selectedUsers.Clear();

                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("MatchDisposed"), delegate(bool del)
                {
                    FASGui.BackToGameScene();
                });

                return;
            }

            this.match = match;

            UpdatePlayers();
        }

        void UpdatePlayers()
        {
            // Player status update
            foreach (var cell in cells)
            {
                if (cell.Player != null && !string.IsNullOrEmpty(cell.Player.User.Id))
                {
                    var player = this.match.Players.Find(x => x.User.Id == cell.Player.User.Id);

                    if (player == null) // already not inculded in match player list
                    {
                        cell.Player.State = Models.Player.Status.Cancelled;

                        cell.SetPlayer(cell.Player, this);

                        if (recipient == FASMatchMaking.Recipient.Everyone)
                        {
                            StartCoroutine(DelayClearPlayer(cell, 2.0f));
                        }
                    }
                }
            }

            foreach (var player in this.match.Players)
            {
                var cell = cells.Find(x => x.Player.User.Id == player.User.Id);

                if (cell != null) // player exists
                {
                    if (player.State == Fresvii.AppSteroid.Models.Player.Status.Cancelled)
                    {
                        cell.SetPlayer(player, this);

                        if (recipient == FASMatchMaking.Recipient.Everyone)
                        {
                            StartCoroutine(DelayClearPlayer(cell, 2.0f));
                        }
                    }
                    else
                    {
                        cell.SetPlayer(player, this); // update
                    }
                }
                else // new player
                {
                    var emptyCell = cells.Find(x => (x.Player == null) || (string.IsNullOrEmpty(x.Player.User.Id)));

                    if (emptyCell != null)
                    {
                        emptyCell.SetPlayer(player, this);
                    }
                }
            }

            var emptyCells = cells.FindAll(x => (x.Player == null) || (string.IsNullOrEmpty(x.Player.User.Id)));

            foreach (var cell in emptyCells)
            {
                cell.UpdateCell();
            }
        }

        IEnumerator DelaySetPlayer(AUIMatchmakingPlayerCell cell, Fresvii.AppSteroid.Models.Player player, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            cell.SetPlayer(player, this);
        }

        IEnumerator DelayClearPlayer(AUIMatchmakingPlayerCell cell, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            cell.ClearPlayer();
        }

        private bool matchDisposed;

        void MatchDisposed()
        {
            if (this.gameObject == null) return;

            progressBar.gameObject.SetActive(false);

            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

            if (!matchDisposed)
            {
                if (this.invitaion == null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("MatchDisposed"), delegate(bool del)
                    {
                        if (del)
                        {
                            FASGui.ShowMatchMakingGui(
                                this.minNumberOfPlayers,
                                this.maxNumberOfPlayers,
                                this.inviteUsers,
                                this.invitationMessage,
                                this.segment,
                                this.recipient
                            );
                        }
                        else
                        {
                            FASGui.BackToGameScene();
                        }
                    });
                }
                else
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("MatchMakingCanceled"), delegate(bool del)
                    {
                        if (del)
                        {
                            FASGui.ShowMatchMakingGui(
                                this.minNumberOfPlayers,
                                this.maxNumberOfPlayers,
                                this.inviteUsers,
                                this.invitationMessage,
                                this.segment,
                                this.recipient
                            );
                        }
                        else
                        {
                            FASGui.BackToGameScene();
                        }
                    });
                }

                matchDisposed = true;
            }
        }

        void OnMatchMakingGameContextCreated(Fresvii.AppSteroid.Models.GameContext gameContext)
        {
            FASMatchMaking.latestGameContext = gameContext;
        }        

        public float MatchMakingProgress()
        {
            if (match == null)
            {
                return 0.0f;
            }

            float timeoutSeconds = FASMatchMaking.GetMatchMakingTimeOutSeconds();

            System.TimeSpan ts = System.DateTime.Now.Subtract(match.CreatedAt);

            float subtractMilliSeconds = ts.Seconds * 1000f + ts.Milliseconds;

            subtractMilliSeconds = Mathf.Clamp(subtractMilliSeconds, 0f, (float)timeoutSeconds * 1000f);

            return subtractMilliSeconds / (timeoutSeconds * 1000f);
        }
    }
}
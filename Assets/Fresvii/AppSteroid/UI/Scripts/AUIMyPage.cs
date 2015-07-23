using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMyPage : MonoBehaviour
    {
        public enum Page { Top, FriendList, FriendRequest, VideoList };

        public bool isTop;

        public AUIRawImageTextureSetter auiRawImage;

        public Text userName;

        public Text userCode;

        public Text description;

        public Text friendsNum, requestsNum, videosNum, messagesNum;

        public GameObject prfbMyProfileEdit;

        public GameObject prfbFriendList;

        public GameObject prfbFriendRequest;

        public GameObject prfbMessageList;

        public GameObject prfbVideoList;

        public AUIFrame frameTween;

        public AUIRawImageTextureSetter bgImage;

        public GameObject backGameButton, backPageButton;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public Text backButtonText;

        public VerticalLayoutGroup contents;

        public static Page StartPage = Page.Top;

        public Transform statsNode;

        public GameObject prfbAUIStatCell;

        public AUIGridLayoutHelper auiGridLayoutHelper;

        List<Fresvii.AppSteroid.Models.Stat> stats = new List<Fresvii.AppSteroid.Models.Stat>();

        private List<AUIStatCell> statCells = new List<AUIStatCell>();

        public Text statsTitle;
 
        // Use this for initialization
        void Awake()
        {
            backGameButton.SetActive(isTop);

            backPageButton.SetActive(!isTop);

            statsTitle.text = FASConfig.Instance.appName + " " + FASText.Get("Stats");
        }

        void OnEnable()
        {
            StartCoroutine(Init());

            AUIManager.OnEscapeTapped += Back;

            FASEvent.OnFriendshipRequestCreated += OnFriendshipRequestCreated;

        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            FASEvent.OnFriendshipRequestCreated -= OnFriendshipRequestCreated;

        }

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            //contents.padding.bottom = (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;
        }

        void OnFriendshipRequestCreated(Fresvii.AppSteroid.Models.User user)
        {
            FASUser.GetAccount((_user, _e) =>
            {
                if (_e == null)
                {
                    SetCurrentUserInfo();
                }
            });
        }

        public Color[] cellColors;

        public bool statInitialized;

        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            if (FAS.CurrentUser != null)
            {
                SetCurrentUserInfo();
            }

            FASUser.GetAccount((user, e) =>
            {
                if (e == null)
                {
                    SetCurrentUserInfo();
                }
            });

            FASPlayStats.GetAccountStat((stats, meta, error) =>
            {
                if (error != null)
                {

                }
                else
                {
                    int i = 0;

                    foreach (var stat in stats)
                    {
                        var cell = statCells.Find(x => x.Stat.Label == stat.Label);

                        if (cell != null)
                        {
                            cell.Set(stat, cellColors[i % cellColors.Length]);
                        }
                        else
                        {
                            GameObject statCell = Instantiate(prfbAUIStatCell) as GameObject;

                            var auiStatCell = statCell.GetComponent<AUIStatCell>();

                            auiStatCell.Set(stat, cellColors[i % cellColors.Length]);

                            statCell.transform.SetParent(statsNode, false);

                            statCells.Add(auiStatCell);
                        }

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

                    auiGridLayoutHelper.CalcSize();
                }
            });            
        }

        void Update()
        {
            if (StartPage == Page.VideoList)
            {
                StartPage = Page.Top;

                GoToVideoList(false);
            }
            else if (StartPage == Page.FriendRequest)
            {
                StartPage = Page.Top;

                GoToFriendRequest(false);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (copyBalloon.gameObject.activeInHierarchy)
                {
                    StartCoroutine(DelayHideCopyBalloon());
                }
            }

        }

        public Text requestsTitleText;

        void SetCurrentUserInfo()
        {
            auiRawImage.Set(FAS.CurrentUser.ProfileImageUrl);

            bgImage.Set(FAS.CurrentUser.ProfileImageUrl);

            userName.text = FAS.CurrentUser.Name;

            userCode.text = FAS.CurrentUser.UserCode;

            description.text = FAS.CurrentUser.Description;

            friendsNum.text = FAS.CurrentUser.FriendsCount.ToString();

            videosNum.text = FAS.CurrentUser.VideosCount.ToString();

            requestsNum.text = FAS.CurrentUser.FriendRequestsCount.ToString();

            if (FAS.CurrentUser.FriendRequestsCount > 0)
            {
                requestsTitleText.fontStyle = requestsNum.fontStyle = FontStyle.Bold;
            }
            else
            {
                requestsTitleText.fontStyle = requestsNum.fontStyle = FontStyle.Normal;
            }

            AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count = FAS.CurrentUser.FriendRequestsCount;
        }

        public void GoToMyProfileEdit()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject myProfileEdit = Instantiate(prfbMyProfileEdit) as GameObject;

            myProfileEdit.GetComponent<RectTransform>().SetParent(transform.parent, false);

            myProfileEdit.transform.SetAsLastSibling();

            AUIMyProfileEdit auiMyProfileEdit = myProfileEdit.GetComponent<AUIMyProfileEdit>();

            auiMyProfileEdit.auiMyPage = this;

            frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            auiMyProfileEdit.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });            
        }

        public void GoToMyFriendList()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject friendList = Instantiate(prfbFriendList) as GameObject;

            friendList.GetComponent<RectTransform>().SetParent(transform.parent, false);

            friendList.transform.SetAsLastSibling();

            AUIFriendList auiFriendList = friendList.GetComponent<AUIFriendList>();

            auiFriendList.User = FAS.CurrentUser;

            auiFriendList.parentFrameTween = this.frameTween;

            auiFriendList.backButtonText.text = FASText.Get("MyPage");

            frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            auiFriendList.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });            
        }

        public void GoToFriendRequest()
        {
            GoToFriendRequest(true);
        }

        public void GoToFriendRequest(bool animation)
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbFriendRequest) as GameObject;

            go.GetComponent<RectTransform>().SetParent(transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIFriendRequest friendRequest = go.GetComponent<AUIFriendRequest>();

            friendRequest.SetBackButton(FASText.Get("MyPage"));

            friendRequest.parentFrameTween = this.frameTween;


            if (animation)
            {
                frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
                {
                    this.gameObject.SetActive(false);
                });

                friendRequest.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
                {

                });
            }
            else
            {
                this.gameObject.SetActive(false);

                friendRequest.frameTween.SetPosition(Vector2.zero);
            }
        }

        public void GoToMessageList()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbMessageList) as GameObject;

            go.GetComponent<RectTransform>().SetParent(transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIMessageList messageList = go.GetComponent<AUIMessageList>();

            messageList.SetBackButton(FASText.Get("MyPage"));

            messageList.parentFrameTween = this.frameTween;

            frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            messageList.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });
        }

        public void GoToVideoList()
        {
            GoToVideoList(true);
        }

        public void GoToVideoList(bool animation)
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbVideoList) as GameObject;

            go.GetComponent<RectTransform>().SetParent(transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIVideoList videoList = go.GetComponent<AUIVideoList>();

            videoList.backButtonText.text = FASText.Get("MyPage");

            videoList.parentFrameTween = this.frameTween;

            if (animation)
            {
                frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
                {
                    this.gameObject.SetActive(false);
                });

                videoList.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
                {

                });
            }
            else
            {
                this.gameObject.SetActive(false);

                videoList.frameTween.SetPosition(Vector2.zero);
            }
        }

        public void Back()
        {
            if (frameTween.Animating || isTop) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        public AUIFadeSetActive copyBalloon;
       
        public void OnLongTapUserCode()
        {
            copyBalloon.FadeIn();
        }

        public void CopyUserCode()
        {
            Debug.Log("Copy User Code");

            Fresvii.AppSteroid.Util.Clipboard.SetText(FAS.CurrentUser.UserCode);
        }

        IEnumerator DelayHideCopyBalloon()
        {
            yield return new WaitForSeconds(0.5f);

            copyBalloon.FadeOut();
        }
    }
}
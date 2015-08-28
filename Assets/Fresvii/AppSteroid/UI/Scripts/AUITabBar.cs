using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUITabBar : MonoBehaviour
    {
        public enum TabButton { None = -1, Forum = 0, Leaderboards, Messages, MyPage, MoreApps };

        public static AUITabBar Instance;

        private RectTransform rectTransform;

        public TabButton SelectedTabButton { get; protected set; }

        public GameObject[] buttons;

        public Image[] buttonImages;

        public AUITabBadge[] tabBadges;

        public Text[] texts;

        public Color selectedColor;

        public Color unselectedColor;

        public static event Action<TabButton> OnTabButtonClicked;

        private uint unreadDirectMessageCount = 0;

        private uint UnreadDirectMessageCount
        {
            get { return unreadDirectMessageCount; }

            set
            {
                if (value < 0) return;

                unreadDirectMessageCount = value;

                AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.Messages].Count = unreadGroupMessageCount + unreadDirectMessageCount;
            }
        }

        private uint unreadGroupMessageCount = 0;

        private uint UnreadGroupMessageCount
        {
            get { return unreadGroupMessageCount; }

            set
            {
                if (value < 0) return;

                unreadGroupMessageCount = value;

                AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.Messages].Count = unreadGroupMessageCount + unreadDirectMessageCount;
            }
        }
        
        private List<string> unreadGroupIds = new List<string>();

        private List<Fresvii.AppSteroid.Models.DirectMessage> unreadDirectMessages = new List<Fresvii.AppSteroid.Models.DirectMessage>();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);

                return;
            }

            Instance = this;

            foreach (var button in buttons)
            {
                button.SetActive(false);
            }
        }

        // Use this for initialization
        IEnumerator Start()
        {
            rectTransform = GetComponent<RectTransform>();

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            if (FAS.CurrentUser != null)
            {
                FASUser.GetAccount((user, e) =>
                {
                    if (e == null)
                    {
                        AUITabBar.Instance.tabBadges[(int)AUITabBar.TabButton.MyPage].Count = FAS.CurrentUser.FriendRequestsCount;
                    }
                });

                FASGroup.GetGroupMessageGroupList((groups, meta, error) =>
                {
                    if (error == null)
                    {
                        foreach (var group in groups)
                        {
                            if (group.LastReadMessageId != group.LatestMessage.Id)
                            {
                                AddUnredGroup(group.Id);
                            }
                        }
                    }
                });

                FASDirectMessage.GetDirectMessageList(0, true, (dms, meta, error) =>
                {
                    if (error == null)
                    {
                        foreach (var dm in dms)
                        {
                            AddUnreadDirectMessage(dm);
                        }
                    }
                });
            }
        }

        public void SetTabButtonEnable(TabButton tabButton)
        {
            buttons[(int)tabButton].gameObject.SetActive(true);
        }

        public int GetHeight()
        {
            return (int)rectTransform.sizeDelta.y;
        }

        public void OnSelected(TabButton tabButton)
        {
            OnSelected((int)tabButton);
        }

        public void OnSelected(int index)
        {
			if (AUIFrame.SomethingAnimationg)
				return;

            SelectedTabButton = (TabButton)index;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (!buttons[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (i == index)
                {
                    buttonImages[i].color = selectedColor;

                    texts[i].color = selectedColor;
                }
                else
                {
                    buttonImages[i].color = unselectedColor;

                    texts[i].color = unselectedColor;
                }
            }

            if (OnTabButtonClicked != null)
            {
                OnTabButtonClicked(SelectedTabButton);
            }
        }

        /*public void AddFriendshipRequest(string userId)
        {
            if (friendshipRequestUserIds.Find(x => x == userId) == null)
            {
                friendshipRequestUserIds.Add(userId);

                tabBadges[(int)TabButton.MyPage].Count = (uint)friendshipRequestUserIds.Count;
            }
        }*/

        public void AddUnredGroup(string groupId)
        {
            if (unreadGroupIds.Find(x => x == groupId) == null)
            {
                unreadGroupIds.Add(groupId);

                tabBadges[(int)TabButton.Messages].Count = (uint)(unreadGroupIds.Count + unreadDirectMessages.Count);
            }
        }

        public void AddUnreadDirectMessage(Fresvii.AppSteroid.Models.DirectMessage dm)
        {
            if (unreadDirectMessages.Find(x => x.Id == dm.Id) == null)
            {
                unreadDirectMessages.Add(dm);

                tabBadges[(int)TabButton.Messages].Count = (uint)(unreadGroupIds.Count + unreadDirectMessages.Count);
            }
        }

        /*public void FriendshipRequestDone(string userId)
        {
            var u = friendshipRequestUserIds.Find(x => x == userId);

            if (u != null)
            {
                friendshipRequestUserIds.Remove(u);

                tabBadges[(int)TabButton.MyPage].Count = (uint)friendshipRequestUserIds.Count;
            }
        }*/

        public void GroupMessageRead(string groupId)
        {
            var g = unreadGroupIds.Find(x => x == groupId);

            if (g != null)
            {
                unreadGroupIds.Remove(g);

                tabBadges[(int)TabButton.Messages].Count = (uint)(unreadGroupIds.Count + unreadDirectMessages.Count);
            }
        }

        public void DirectMessageRead()
        {
            unreadDirectMessages.Clear();

            tabBadges[(int)TabButton.Messages].Count = (uint)(unreadGroupIds.Count + unreadDirectMessages.Count);            
        }
    }
}
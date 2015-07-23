using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMessageListCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Group Group { get; set; }

        public AUIGroupIcon groupIcon;

        public Text groupName;

        public Text message;

        public Text updatedAt;

        AUIMessageList parantPage;

        public Color unreadUpdateAt, readUpdateAt;

        public AUITextSetter groupNameTextSetter;

        public void OnEnable()
        {
            StartCoroutine(UpdateUpdatedAt());
        }

        public void SetGroup(Fresvii.AppSteroid.Models.Group group, AUIMessageList parantPage)
        {
            this.Group = group;

            this.parantPage = parantPage;

            Fresvii.AppSteroid.Models.Member member = null;

            //  Icon set
            if (Group.MessageMembers != null)
            {
                for (int i = 0; i < Group.MessageMembers.Count; i++)
                {
                    if (Group.MessageMembers[i].Id != FAS.CurrentUser.Id)
                    {
                        member = Group.MessageMembers[i];

                        break;
                    }                    
                }

                List<string> userIcons = new List<string>();

                for (int i = 0; i < Group.MessageMembers.Count; i++)
                {
                    userIcons.Add(Group.MessageMembers[i].ProfileImageUrl);

                    if (i == 2)
                    {
                        break;
                    }
                }

                groupIcon.Set(userIcons.ToArray());
            }

            // Group member names set
            string memberNames = "";

            if (Group.MembersCount <= 1)
            {
                memberNames = FresviiGUIText.Get("NobodyElse");
            }
            else
            {
                if (Group.Pair && member != null)
                {
                    memberNames = member.Name;
                }
                else
                {
                    for (int i = 0; i < Group.MessageMembers.Count; i++)
                    {
                        memberNames += Group.MessageMembers[i].Name + ((i == Group.MessageMembers.Count - 1) ? "" : ", ");

                        if (i == 5) break;
                    }
                }

                /*if (!Group.Pair)
                {
                    memberNames += "(" + Group.MembersCount + ")";
                }*/
            }

            groupName.text = memberNames;

            if (group.LatestMessage != null)
            {
                if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Text)
                {
                      message.text = group.LatestMessage.Text;
                }
                else if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Image)
                {
                    if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
                    {
                        message.text = FASText.Get("UserSentAPhoto").Replace("%username", FASText.Get("You"));
                    }
                    else
                    {
                        message.text = FASText.Get("UserSentAPhoto").Replace("%username", Group.LatestMessage.User.Name);
                    }
                }
                else if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Video)
                {
                    if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
                    {
                        message.text = FASText.Get("UserSentAVideo").Replace("%username", FASText.Get("You"));
                    }
                    else
                    {
                        message.text = FASText.Get("UserSentAVideo").Replace("%username", Group.LatestMessage.User.Name);
                    }
                }
                else if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Sticker)
                {
                    if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
                    {
                        message.text = FASText.Get("UserSentASticker").Replace("%username", FASText.Get("You"));
                    }
                    else
                    {
                        message.text = FASText.Get("UserSentASticker").Replace("%username", Group.LatestMessage.User.Name);
                    }
                }
            }

            bool isUnred = IsUnread();

            groupName.fontStyle = message.fontStyle = (IsUnread()) ?  FontStyle.Bold : FontStyle.Normal;

            updatedAt.color = (IsUnread()) ? unreadUpdateAt : readUpdateAt;

            updatedAt.text = AUIUtility.CurrentTimeSpan(Group.LatestMessage.CreatedAt);

            if (isUnred)
            {
                AUITabBar.Instance.AddUnredGroup(Group.Id);
            }

            if (!Group.Pair)
            {
                groupNameTextSetter.truncatedReplacement = "...(" + Group.MembersCount.ToString() + ")";
            }
        }

        bool IsUnread()
        {
            bool unread;

            if (Group.LatestMessage == null)
            {
                unread = false;
            }
            else
            {
                unread = (Group.LastReadMessageId != Group.LatestMessage.Id) || string.IsNullOrEmpty(Group.LastReadMessageId);
            }

            if (Group.LatestMessage == null)
            {
                unread = false;
            }
            else if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
            {
                unread = false;
            }

            return unread;
        }

        public void Read()
        {
            AUITabBar.Instance.GroupMessageRead(Group.Id);

            message.fontStyle = FontStyle.Normal;

            Group.LastReadMessageId = Group.LatestMessage.Id;
        }

        IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (Group != null && Group.LatestMessage != null)
                {
                    updatedAt.text = AUIUtility.CurrentTimeSpan(Group.LatestMessage.CreatedAt);

                    yield return new WaitForSeconds(60f);
                }
                else
                {
                    yield return 1;
                }
            }
        }

        public void GoToMessage()
        {
            if (Group != null)
            {
                parantPage.GoToMessage(Group, true);
            }
        }
    }
}
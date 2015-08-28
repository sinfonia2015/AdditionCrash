using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGroupInfo : MonoBehaviour
    {
        public enum Mode { Normal, Deletable };

        public AUIGroupInfo.Mode mode = Mode.Normal;

        public AUIGroupIcon groupIcon;

        public Text groupName;

        public Text activeDate;

        public AUIFrame frame;

        private Fresvii.AppSteroid.Models.Group group;

        private AUIFrame parentFrameTween;

        public AUIRawImageTextureSetter bgImage;

        public RectTransform prfbMemberCell;

        private List<AUIGroupMemberCell> memberCells = new List<AUIGroupMemberCell>();

        private bool isPullRefleshProc;

        public RectTransform addMemberCell;

        public GameObject leaveButton;

        public Text editButtonText;

        public Button buttonCall;

        public GameObject prfbAddGroupMember;

        public GameObject goButtonEdit;

        public Transform scrollContents;

        public int cellIndexOffset = 4;

        // Use this for initialization
        void Awake()
        {
            buttonCall.gameObject.SetActive(false);
        }

        public void Set(Fresvii.AppSteroid.Models.Group group, AUIFrame parentFrameTween)
        {
            this.group = group;

            this.parentFrameTween = parentFrameTween;

            addMemberCell.gameObject.SetActive(false);

            leaveButton.gameObject.SetActive(false);

            StartCoroutine(Init());
        }

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;
        }

        IEnumerator Init()
        {
            while (group == null)
            {
                yield return 1;
            }

            if (group.Pair)
            {
                addMemberCell.gameObject.SetActive(false);

                goButtonEdit.SetActive(false);

                leaveButton.SetActive(false);
            }

#if GROUP_CONFERENCE
            {
                bool includingOficial = false;

                group.FetchMembers((error) =>
                {
                    if (error == null)
                    {
                        foreach (var _member in group.Members)
                        {
                            if (_member.Official)
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
            }
#endif


            FASGroup.GetGroupMemberList(this.group.Id, OnGetGroupMemberList);

            while (group.Members == null || group.Members.Count == 0)
            {
                yield return 1;
            }

            Fresvii.AppSteroid.Models.Member member = null;

            //  Icon set
            for (int i = 0; i < group.Members.Count; i++)
            {
                if (group.Members[i].Id != FAS.CurrentUser.Id)
                {
                    member = group.Members[i];

                    bgImage.Set(member.ProfileImageUrl);

                    break;
                }
            }

            List<string> userIcons = new List<string>();

            for (int i = 0; i < group.Members.Count; i++)
            {
                userIcons.Add(group.Members[i].ProfileImageUrl);

                if (i == 2)
                {
                    break;
                }
            }

            groupIcon.Set(userIcons.ToArray());

            // Group member names set
            string memberNames = "";

            if (group.Pair && member != null)
            {
                if (member != null)
                    memberNames = member.Name;
            }
            else if (group.MembersCount <= 1)
            {
                memberNames = FresviiGUIText.Get("NobodyElse");
            }
            else
            {
                for (int i = 0; i < group.Members.Count; i++)
                {
                    if(group.Members[i].Id != FAS.CurrentUser.Id)
                        memberNames += (string.IsNullOrEmpty(memberNames)) ? "" : ", " + group.Members[i].Name;

                    if (i == 5) break;
                }

                memberNames += "(" + group.MembersCount + ")";
            }

            groupName.text = memberNames;

            activeDate.text = FASText.Get("Active") + " " + AUIUtility.CurrentTimeSpan(group.UpdatedAt);

            FASUtility.SendPageView("pv.messages.members", this.group.Id, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });

        }      

        void OnGetGroupMemberList(IList<Fresvii.AppSteroid.Models.Member> members, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null || this.enabled == false)
            {
                return;
            }

            if (!group.Pair)
            {
                addMemberCell.gameObject.SetActive(true);

                leaveButton.SetActive(true);
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            bool added = false;

            group.Members = members;

            foreach (Fresvii.AppSteroid.Models.Member member in members)
            {
                added |= UpdateMember(member);
            }
        }
       
        public bool UpdateMember(Fresvii.AppSteroid.Models.Member member)
        {
            AUIGroupMemberCell cell = memberCells.Find(x => x.Member.Id == member.Id);

            if (cell != null)
            {
                cell.SetMember(member, this);

                return false;
            }

            var item = Instantiate(prfbMemberCell) as RectTransform;            

            cell = item.GetComponent<AUIGroupMemberCell>();

            cell.SetMember(member, this);

            memberCells.Add(cell);

            cell.transform.SetParent(scrollContents, false);

            cell.transform.SetSiblingIndex(scrollContents.childCount - cellIndexOffset);

            return true;
        }

        public void Back()
        {
            if (frame.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        public void BackToMessageTop()
        {
            AUIMessages messages = parentFrameTween.GetComponent<AUIMessages>();

            if (messages == null)
            {
                return;
            }

            AUIMessageList messageList = messages.parentFrameTween.GetComponent<AUIMessageList>();

            if (messageList != null)
            {
                Destroy(parentFrameTween.gameObject);

                messageList.gameObject.SetActive(true);

                messageList.RemoveGroup(group);

                RectTransform rectTransform = GetComponent<RectTransform>();

                messageList.frameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
                {
                    Destroy(this.gameObject);
                });
            }
        }

        public void LeaveGroup()
        {
            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Leave"), FASText.Get("Cancel"), FASText.Get("Close"));

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmLeaveGroup"), (del) =>
            {
                if (del)
                {
                    FASGroup.DeleteMember(this.group.Id, FAS.CurrentUser.Id, (error) =>
                    {
                        if (error != null)
                        {
                            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (d) => { });
                        }
                        else
                        {
                            BackToMessageTop();
                        }
                    });
                }
            });
        }

        public void RemoveMember(Fresvii.AppSteroid.Models.Member member)
        {
            AUIGroupMemberCell cell = memberCells.Find(x => x.Member.Id == member.Id);

            cell.button.interactable = false;

            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Delete"), FASText.Get("Cancel"), FASText.Get("Close"));

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmRemoveMember"), (del)=>
            {
                if (del)
                {
                    FASGroup.DeleteMember(this.group.Id, member.Id, (error) =>
                    {
                        if (error != null)
                        {
                            cell.button.interactable = true;

                            Debug.LogError(error.ToString());

                            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), (d) => { });
                        }
                        else
                        {
                            if (cell != null)
                            {
                                cell.gameObject.SetActive(false);

                                memberCells.Remove(cell);

                                Destroy(cell.gameObject);

                                /*cell.AnimateDelete((size) =>
                                {
                                    memberCells.Remove(cell);

                                    Destroy(cell.gameObject);
                                });*/
                            }
                        }
                    });
                }
                else
                {
                    cell.button.interactable = true;
                }
            });
        }

        public void OnClickEdit()
        {
            if (mode == Mode.Normal)
            {
                mode = Mode.Deletable;
            }
            else
            {
                mode = Mode.Normal;
            }

            foreach (AUIGroupMemberCell cell in memberCells)
            {
                cell.EditMember(mode);
            }

            if (mode == Mode.Deletable)
            {
                //contents.RemoveItem(addMemberCell);

                addMemberCell.gameObject.SetActive(false);

                //contents.padding.bottom -= (int)leaveButton.GetComponent<LayoutElement>().preferredHeight;
            }
            else
            {
                addMemberCell.gameObject.SetActive(true);

                //contents.AddItem(addMemberCell);

                //scrollView.GetComponent<LayoutElement>().preferredHeight = leaveButton.GetComponent<LayoutElement>().preferredHeight;

                //contents.padding.bottom += (int)leaveButton.GetComponent<LayoutElement>().preferredHeight;
            }

            leaveButton.SetActive(mode == Mode.Normal && !group.Pair);

            addMemberCell.gameObject.SetActive(mode == Mode.Normal);

            editButtonText.text = (mode == Mode.Normal) ? FASText.Get("Edit") : FASText.Get("Done");

            buttonCall.interactable = (mode == Mode.Normal);
        }

        public void GoToAddMember()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIAddGroupMember addGroupMember = ((GameObject)Instantiate(prfbAddGroupMember)).GetComponent<AUIAddGroupMember>();

            addGroupMember.groupInfo = this;

            addGroupMember.Group = group;

            addGroupMember.transform.SetParent(transform.parent, false);

            addGroupMember.transform.SetAsLastSibling();

            addGroupMember.parentFrameTween = this.frame;

            addGroupMember.frameTween.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () => 
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

            if (group.MembersCount > Fresvii.AppSteroid.Services.GroupConferenceService.MaxCallMemberCount)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("GroupConferenceMemberLimitation"), (del)=>
                {

                });

                return;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIGroupConference groupConference = ((GameObject)Instantiate(prfbGroupConference)).GetComponent<AUIGroupConference>();

            groupConference.SetGroup(group);

            groupConference.transform.SetParent(transform.parent, false);

            groupConference.transform.SetAsLastSibling();

            groupConference.parentFrameTween = this.frame;

            groupConference.frameTween.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () =>
            {
                this.gameObject.SetActive(false);
            });

#endif
        }

        void Update()
        {
#if GROUP_CONFERENCE
            buttonCall.interactable = !FASConference.IsCalling();
#endif
        }

        public GameObject prfbMyPage;

        public GameObject prfbUserPage;

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

                myPage.backButtonText.text = FASText.Get("Back");

                myPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }
            else
            {
                AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

                userPage.transform.SetParent(transform.parent, false);

                userPage.Set(user, FASText.Get("Back"), this.frame);

                userPage.transform.SetAsLastSibling();

                userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIGroupMemberCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Member Member { get; set; }

        private AUIGroupInfo parentPage;

        public Text userName;

        public AUIRawImageTextureSetter userIcon;

        public RectTransform pivotRectTransform;

        public RectTransform deleteIcon;

        public AUICellDeleteAnimator deleteAnimator;

        public Button button;

        private Fresvii.AppSteroid.Models.User user;

        private 

        void Awake()
        {
            button.interactable = true;
        }

        void OnEnable()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            while (!FASUser.IsLoggedIn() || Member == null)
                yield return 1;

            if (user == null)
            {
                GetUser();
            }
        }

        void GetUser()
        {
            FASUser.GetUser(this.Member.Id, (_user, _error) =>
            {
                if (_error == null)
                {
                    this.user = _user;
                }
                else
                {
                    Debug.LogError(_error.ToString());

                    Invoke("GetUser", 3f);
                }
            });
        }

        public void SetMember(Fresvii.AppSteroid.Models.Member member, AUIGroupInfo parentPage)
        {
            this.Member = member;

            this.parentPage = parentPage;

            userName.text = member.Name;

            userIcon.Set(member.ProfileImageUrl);
        }

        public void AnimateDelete(Action<Vector2> callback)
        {
            //deleteAnimator.Animate(parentPage.contents, callback);
        }

        public float slideDuration = 0.5f;

        public float slidePosNormalX = -60f;

        public float slidePosDeleteX = 0f;

        public Vector2 iconSize = new Vector2(44f, 44f);

        public iTween.EaseType easetype = iTween.EaseType.easeOutExpo;


        public void EditMember(AUIGroupInfo.Mode mode)
        {
            //button.interactable = (mode == AUIGroupInfo.Mode.Deletable);

            iTween.Stop(this.gameObject);

            if (this.gameObject.activeInHierarchy)
            {
                if (mode == AUIGroupInfo.Mode.Deletable)
                {
                    iTween.ValueTo(this.gameObject, iTween.Hash("from", pivotRectTransform.anchoredPosition.x, "to", slidePosDeleteX, "time", slideDuration, "easetype", easetype, "onupdate", "OnSlideUpdate"));

                    iTween.ValueTo(this.gameObject, iTween.Hash("from", deleteIcon.sizeDelta, "to", iconSize, "time", slideDuration, "easetype", easetype, "onupdate", "OnIconSizeUpdate"));
                }
                else
                {
                    iTween.ValueTo(this.gameObject, iTween.Hash("from", pivotRectTransform.anchoredPosition.x, "to", slidePosNormalX, "time", slideDuration, "easetype", easetype, "onupdate", "OnSlideUpdate"));

                    iTween.ValueTo(this.gameObject, iTween.Hash("from", deleteIcon.sizeDelta, "to", Vector2.zero, "time", slideDuration, "easetype", easetype, "onupdate", "OnIconSizeUpdate"));
                }
            }
            else
            {
                if (mode == AUIGroupInfo.Mode.Deletable)
                {
                    pivotRectTransform.anchoredPosition = new Vector2(slidePosDeleteX, pivotRectTransform.anchoredPosition.y);

                    deleteIcon.sizeDelta = iconSize;
                }
                else
                {
                    pivotRectTransform.anchoredPosition = new Vector2(slidePosNormalX, pivotRectTransform.anchoredPosition.y);

                    deleteIcon.sizeDelta = Vector2.zero;
                }
            }
        }

        public void OnSlideUpdate(float value)
        {
            pivotRectTransform.anchoredPosition = new Vector2(value, pivotRectTransform.anchoredPosition.y);
        }

        public void OnIconSizeUpdate(Vector2 size)
        {
            deleteIcon.sizeDelta = size;
        }

        public void Remove()
        {
            if (parentPage.mode == AUIGroupInfo.Mode.Deletable)
            {
                parentPage.RemoveMember(this.Member);
            }
        }

        public void GoToUserPage()
        {
            if (parentPage.mode == AUIGroupInfo.Mode.Normal)
            {
                if (this.user != null)
                {
                    parentPage.GoToUserPage(this.user);
                }

            }
        }
    }
}
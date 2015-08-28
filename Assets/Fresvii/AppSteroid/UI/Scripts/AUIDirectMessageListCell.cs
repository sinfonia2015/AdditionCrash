using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIDirectMessageListCell : MonoBehaviour
    {
        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        public Text subject;

        public AUIMessageList parantPage;

        private Fresvii.AppSteroid.Models.DirectMessage directMessage;

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private IList<Fresvii.AppSteroid.Models.DirectMessage> directMessages;

        public void OnEnable()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            FASDirectMessage.GetDirectMessageList(1, false, (directMessages, meta, error) =>
            {
                if (this.gameObject == null || !this.gameObject.activeInHierarchy)
                {
                    return;
                }

                if (error == null)
                {
                    this.directMessages = directMessages;

                    this.listMeta = meta;

                    if (directMessages.Count > 0)
                    {
                        this.directMessage = directMessages[0];

                        SetDirectMessage(this.directMessage);
                    }
                }                
            });

            while (FAS.OfficialUser == null)
            {
                yield return 1;
            }

            userIcon.Set(FAS.OfficialUser.ProfileImageUrl);

        }

        void SetDirectMessage(Fresvii.AppSteroid.Models.DirectMessage dm)
        {
            userName.fontStyle = subject.fontStyle = (dm.Unread) ? FontStyle.Bold : FontStyle.Normal;

            subject.text = dm.Subject;
        }

        public void GoToDirectMessage()
        {
            parantPage.GoToDirectMessage(directMessages, listMeta);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

using Fresvii.AppSteroid;

namespace Fresvii.AppSteroid.UI
{
    public class AUIUserAgreement : MonoBehaviour
    {
        public AUIFrame frame;

        public Fresvii.AppSteroid.Models.UserAgreement UserAgreement;

        public Text userAgreementText;

        Action<bool> callback;

        public void SetUserAgreement(Fresvii.AppSteroid.Models.UserAgreement userAgreement, Action<bool> callback)
        {
            this.UserAgreement = userAgreement;

            this.callback = callback;

            userAgreementText.text = userAgreement.Text;
        }

        public void OnClickAgree()
        {
            callback(true);
        }

        public void OnClickDecline()
        {
            callback(false);
        }


    }
}

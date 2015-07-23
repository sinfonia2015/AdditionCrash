using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMessageDateTimeLine : MonoBehaviour
    {
        System.DateTime dateTime;

        public Text dateTimeText;

        public void SetDateTime(System.DateTime dateTime)
        {
            this.dateTime = dateTime;

            if (Application.systemLanguage == SystemLanguage.Japanese)
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");

                dateTimeText.text = ((this.dateTime.Year != System.DateTime.Now.Year) ? this.dateTime.ToString("D", ci) : this.dateTime.ToString("m", ci));
            }
            else
            {
                dateTimeText.text = this.dateTime.ToString("MMM dd") + ((this.dateTime.Year != System.DateTime.Now.Year) ? ", " + this.dateTime.ToString("Y") : "");
            }

        }
    }
}
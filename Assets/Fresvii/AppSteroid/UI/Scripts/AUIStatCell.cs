using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIStatCell : MonoBehaviour {

        public Fresvii.AppSteroid.Models.Stat Stat { get; protected set; }

        public Text labelText;

        public Text valueText;

        public void Set(Fresvii.AppSteroid.Models.Stat stat, Color color)
        {
            this.Stat = stat;

            labelText.text = stat.Label;

            labelText.color = color;

            valueText.text = (string)stat.Value;

            valueText.color = color;
        }
    }
}
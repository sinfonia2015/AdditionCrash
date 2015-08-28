using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AUINavigationBar : MonoBehaviour {

    private static readonly Color lineColor = new Color(195f/255f, 195f/255f, 195f/255f);

    void Start()
    {
        Image line = transform.FindChild("Line").GetComponent<Image>();

        if (line != null)
        {
            line.color = lineColor;

            line.rectTransform.sizeDelta = new Vector2(line.rectTransform.sizeDelta.x, 1f);
        }
    }


}

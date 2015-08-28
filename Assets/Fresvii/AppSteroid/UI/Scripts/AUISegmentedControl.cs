using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class AUISegmentedControl : MonoBehaviour
{
    private static readonly Color lineColor = new Color(195f / 255f, 195f / 255f, 195f / 255f);

    public Button[] buttons;

    public Text[] texts;

    public bool init;

    public int selectedIndex = -1;

    public Sprite leftNormal, leftSelect, centerNormal, centerSelect, rightNormal, rightSelect;

    public Color textSelectColor, textNormalColor;

    public event Action<int> OnChanged;

    void Start()
    {
        if (init)
        {
            SetImage(selectedIndex);
        }


        Image line = transform.FindChild("Line").GetComponent<Image>();

        if (line != null)
        {
            line.color = lineColor;

            line.rectTransform.sizeDelta = new Vector2(line.rectTransform.sizeDelta.x, 1f);
        }
    }

    public void SetIndex(int index)
    {
        if (selectedIndex != index)
        {
            selectedIndex = index;

            if (OnChanged != null)
            {
                OnChanged(selectedIndex);
            }

            SetImage(index);
        }
    }

    public void SetImage(int index)
    {
        buttons[0].image.sprite = ((0 == index) ? leftSelect : leftNormal);

        texts[0].color = ((0 == index) ? textSelectColor : textNormalColor);

        for (int i = 1; i < buttons.Length - 1; i++)
        {
            buttons[i].image.sprite = ((i == index) ? centerSelect : centerNormal);

            texts[i].color = ((i == index) ? textSelectColor : textNormalColor);
        }

        buttons[buttons.Length - 1].image.sprite = ((buttons.Length - 1 == index) ? rightSelect : rightNormal);

        texts[buttons.Length - 1].color = ((buttons.Length - 1 == index) ? textSelectColor : textNormalColor);
    }
}

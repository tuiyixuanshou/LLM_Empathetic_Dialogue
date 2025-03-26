using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignalBubbleText : MonoBehaviour
{
    public Text text;
    public Image image;

    public float textmargin;

    public float maxWidth;

    public float gapSpacing;
    private void Start()
    {
        RectTransform rect = text.GetComponent<RectTransform>();
        maxWidth = rect.rect.width;
        UpdateSize();
    }

    private void Update()
    {
        UpdateSize();
    }

    private void UpdateSize()
    {
        float curWidth = text.preferredWidth;
        float curHeight = text.preferredHeight;

        if (curWidth < maxWidth)
        {
            image.rectTransform.sizeDelta
                = new Vector2(curWidth+ textmargin*2, curHeight+ textmargin * 2+gapSpacing);
            //text.rectTransform.sizeDelta = new Vector2(curWidth, curHeight);
        }
        else
        {
            image.rectTransform.sizeDelta = new Vector2(maxWidth+textmargin * 2, curHeight + textmargin * 2+ gapSpacing);
        }
    }
}

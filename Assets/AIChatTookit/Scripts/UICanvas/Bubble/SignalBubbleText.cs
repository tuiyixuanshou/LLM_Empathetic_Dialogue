using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignalBubbleText : MonoBehaviour
{
    public Text text;
    public Image image;

    public Image EmojiImage;

    public float textmargin;

    public float maxWidth;

    public float gapSpacing;

    public float EmojiMargin = 180f;

    public float OriHeight;
    public float OriWidth;

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
            float imagecurHeight = curHeight + textmargin * 2 + gapSpacing > OriHeight ? curHeight + textmargin * 2 + gapSpacing : OriHeight;
            float imagecurWeight = curWidth + textmargin * 2 + EmojiMargin > OriWidth ? curWidth + textmargin * 2 + EmojiMargin : OriWidth;
            image.rectTransform.sizeDelta
                = new Vector2(imagecurWeight, imagecurHeight);
            //text.rectTransform.sizeDelta = new Vector2(curWidth, curHeight);
        }
        else
        {
            float imagecurHeight = curHeight + textmargin * 2 + gapSpacing > OriHeight ? curHeight + textmargin * 2 + gapSpacing : OriHeight;
            image.rectTransform.sizeDelta = new Vector2(maxWidth+textmargin * 2+ EmojiMargin, imagecurHeight);
        }
    }
}

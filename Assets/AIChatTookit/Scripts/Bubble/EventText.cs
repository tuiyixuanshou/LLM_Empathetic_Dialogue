using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class EventText : MonoBehaviour
{
    [Header("Event组件")]
    public Text EventTextComponent;
    public Image EventImage;

    [Header("边界数值调整")]
    public float TextBackPadding;

    private float maxTextWidth;

    private void Start()
    {
        RectTransform rectTransform = EventTextComponent.GetComponent<RectTransform>();
        maxTextWidth = rectTransform.rect.width;
        updateImage();
    }

    private void Update()
    {
        updateImage();
    }

    private void updateImage()
    {
        float curWidth = EventTextComponent.preferredWidth;
        float curHeight = EventTextComponent.preferredHeight;

        if (curWidth < maxTextWidth)
        {
            EventTextComponent.rectTransform.sizeDelta = new Vector2(curWidth, curHeight);
            EventImage.rectTransform.sizeDelta = new Vector2(EventImage.rectTransform.sizeDelta.x, curHeight + TextBackPadding * 2);
        }
        else
        {
            EventTextComponent.rectTransform.sizeDelta = new Vector2(maxTextWidth, curHeight);
            EventImage.rectTransform.sizeDelta = new Vector2(EventImage.rectTransform.sizeDelta.x, curHeight+ TextBackPadding*2);
        }
    }
}

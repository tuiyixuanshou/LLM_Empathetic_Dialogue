using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleText : MonoBehaviour
{
    [Header("�Ի��������")]
    public Text BubbleTextComponent;
    public Image BubbleTextBack;
    public Image BubbleBack;
    public Text BubbleName;

    [Header("�߽���ֵ����")]
    public float TextBackPadding;
    public float TextNamePadding;

    [Header("�Ƿ�Ϊ�Ҳ�")]
    public bool isRight;

    private float maxWidthSize; //�Ի��������

    private void Start()
    {
        RectTransform rectTransform = BubbleTextComponent.GetComponent<RectTransform>();
        maxWidthSize = rectTransform.rect.width;
        UpdateBackGroundSize();
    }

    private void Update()
    {
        UpdateBackGroundSize();
    }

    void UpdateBackGroundSize()
    {
        if(BubbleTextComponent != null && BubbleTextBack != null && BubbleBack != null)
        {
            float curTextWidth = BubbleTextComponent.preferredWidth;
            float curTextHeight = BubbleTextComponent.preferredHeight;

            if (curTextWidth <= maxWidthSize)
            {
                BubbleTextBack.rectTransform.sizeDelta = new Vector2(curTextWidth + TextBackPadding*2, curTextHeight + TextBackPadding * 2);
                if (isRight)
                {
                    BubbleTextComponent.rectTransform.sizeDelta = new Vector2(curTextWidth, curTextHeight);
                }
            }
            else
            {
                BubbleTextBack.rectTransform.sizeDelta = new Vector2(maxWidthSize+ TextBackPadding*2, curTextHeight + TextBackPadding * 2);
                if (isRight)
                {
                    BubbleTextComponent.rectTransform.sizeDelta = new Vector2(maxWidthSize, curTextHeight);
                }
            }

            BubbleBack.rectTransform.sizeDelta = new Vector2(maxWidthSize+TextBackPadding * 2, curTextHeight + TextBackPadding * 2 + TextNamePadding);
        }
    }


}

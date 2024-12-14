using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[System.Serializable]
public class Bubble
{
    public string textName;
    public string textContend;
    public bool isRight;
    public Sprite textHead;

    //���캯��
    public Bubble(string name, string content, bool right, Sprite head)
    {
        textName = name;
        textContend = content;
        isRight = right;
        textHead = head;
    }
}

public class BubbleSlider : Singleton<BubbleSlider>
{
    public GameObject BubbleLeft;
    public GameObject BubbleRight;

    public Transform scrollViewTransform; //����
    public Transform content;        // Content
    public ScrollRect scrollRect;

    [Header("���ݼ��")]
    public float BubbleSpace; // ����֮��ľ���
    private float lowHeight;//��һ�����ݵ�λ��
    private RectTransform contentRect;

    public List<Bubble> bubbles;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        lowHeight = 0;
        contentRect = gameObject.GetComponent<RectTransform>();
        testBubble();
    }

    void testBubble()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

    }

    public void CreatBubble(Bubble bubble)
    {
        GameObject newbubble;
        if (!bubble.isRight) newbubble = Instantiate(BubbleLeft, content);
        else newbubble = Instantiate(BubbleRight, content);

        BubbleText bubbletest = newbubble.GetComponent<BubbleText>();
        bubbletest.BubbleTextComponent.text = bubble.textContend;
        bubbletest.BubbleName.text = bubble.textName;

        RectTransform rect = newbubble.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(0, -1 * lowHeight);
        lowHeight += bubbletest.TextBackPadding * 2 + bubbletest.BubbleTextComponent.preferredHeight + bubbletest.TextNamePadding+BubbleSpace;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, lowHeight);

        scrollRect.verticalNormalizedPosition = 0f;

    }
}

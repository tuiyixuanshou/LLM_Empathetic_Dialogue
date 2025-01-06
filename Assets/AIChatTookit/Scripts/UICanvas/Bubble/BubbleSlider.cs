using System.Collections;
using System.Collections.Generic;
using System.IO;
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

[System.Serializable]
public class DailyEventBlock
{
    public string Time;
    public string Event;

    //���캯��
    public DailyEventBlock(string time, string content)
    {
        Time = time;
        Event = content;
    }
}

[System.Serializable]
public class EventDataWrapper
{
    public DailyEventBlock[] items;
}

public class BubbleSlider : Singleton<BubbleSlider>
{
    [Header("���ݺ��¼�ʵ��")]
    public GameObject BubbleLeft;
    public GameObject BubbleRight;
    public GameObject EventBlock;

    [Header("������")]
    public Transform scrollViewTransform; //����
    public Transform content;        // Content
    public ScrollRect scrollRect;

    [Header("���ݼ��")]
    public float BubbleSpace; // ����֮��ľ���
    public float EventSpace;
    private float lowHeight;//��һ�����ݵ�λ��
    private RectTransform contentRect;

    public List<DailyEventBlock> dailyEvents;
    public List<Bubble> bubbles;

    private string savePath;

    protected override void Awake()
    {
        base.Awake();
        lowHeight = 0;
        contentRect = gameObject.GetComponent<RectTransform>();
        testBubble();
    }

    private void Start()
    {
        savePath = Application.dataPath + "/response.json";


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

        if (bubble.textHead != null)
        {
            //ʹ���ҵ�ͷ��
            bubbletest.BubbleHeadImage.sprite = bubble.textHead;
        }

        

        RectTransform rect = newbubble.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(0, -1 * lowHeight);
        lowHeight += bubbletest.TextBackPadding * 2 + bubbletest.BubbleTextComponent.preferredHeight + bubbletest.TextNamePadding+BubbleSpace;
        //Debug.Log(lowHeight);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, lowHeight);

        scrollRect.verticalNormalizedPosition = 0f;

    }

    public void ReadsavePath()
    {
        List<string> lines = new List<string>(); 

        using (StreamReader reader = new StreamReader(Application.dataPath + "/response.json"))
        {
            string line;
            while((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }
        string completeJson = string.Join("", lines);
        Debug.Log(completeJson);

        DailyEventBlock[] eventDataArray = JsonUtility.FromJson<EventDataWrapper>($"{{\"items\":{completeJson}}}").items;

        foreach (DailyEventBlock eventData in eventDataArray)
        {
            //Debug.Log($"ʱ��: {eventData.Time}, �¼�: {eventData.Event}");
            CreatEventBlock(eventData);
        }
    }

    public void CreatEventBlock(DailyEventBlock eventData)
    {
        GameObject newEventBlock;
        string completion = $"{eventData.Time},{Settings.Instance.AIName}{eventData.Event}";
        newEventBlock = Instantiate(EventBlock, content);
        newEventBlock.GetComponentInChildren<Text>().text = completion;

        RectTransform rect = newEventBlock.GetComponent<RectTransform>();
        EventText eventText = newEventBlock.GetComponent<EventText>();

        rect.anchoredPosition = new Vector2(0, -1 * lowHeight);
        lowHeight += eventText.EventTextComponent.preferredHeight+eventText.TextBackPadding*2+ EventSpace;
        //Debug.Log(lowHeight);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, lowHeight);

        scrollRect.verticalNormalizedPosition = 0f;
    }
}

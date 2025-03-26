using System.Collections;
using System.Collections.Generic;
//using System.Data;
//using System.Linq;
using UnityEngine;
//using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class BubbleHistory : MonoBehaviour
{
    public Settings settings;
    public LLMChat chat;
    [Header("气泡和事件实例")]
    public GameObject BubbleLeft;
    public GameObject BubbleRight;

    [Header("滑动框")]
    public Transform scrollViewTransform; //滑块
    public Transform content;        // Content
    public ScrollRect scrollRect;

    [Header("气泡间距")]
    public float BubbleSpace; // 气泡之间的距离
    private float lowHeight;//上一个气泡的位置
    private RectTransform contentRect;

    public List<Dictionary<string, string>> tempDialogue = new();

    private void OnEnable()
    {
        lowHeight = 0;
        contentRect = gameObject.GetComponent<RectTransform>();

        tempDialogue.Clear();
        //tempDialoguePos = chat.returntempDialogue();
        //tempDialoguePos = settings.tempDialoguePos;
        DestoryBubble();
        CreatHistory();
    }

    void DestoryBubble()
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
            //使用我的头像
            bubbletest.BubbleHeadImage.sprite = bubble.textHead;
        }
        RectTransform rect = newbubble.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(0, -1 * lowHeight);
        lowHeight += bubbletest.TextBackPadding * 2 + bubbletest.BubbleTextComponent.preferredHeight + bubbletest.TextNamePadding + BubbleSpace;
        //Debug.Log(lowHeight);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, lowHeight);

        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void CreatHistory()
    {
        Debug.Log(tempDialogue.Count);
        if(settings.tempDialogue.Count <= 16)
        {
            foreach(var dial in settings.tempDialogue)
            {
                if (dial != null)
                {
                    string role,content;
                    dial.TryGetValue("role", out role);
                    dial.TryGetValue("content", out content);
                    if (role == "user")
                    {
                        Bubble newbubble = new Bubble(settings.UserName, content ,true,null);
                        CreatBubble(newbubble);
                    }
                    else if(role == "assistant")
                    {
                        Bubble newbubble = new Bubble(settings.AIName, content, false, settings.Headsprite);
                        CreatBubble(newbubble);
                    }
                }
            }
        }
        else
        {
            for(int i = settings.tempDialogue.Count - 15; i < settings.tempDialogue.Count - 1; i++)
            {
                if (tempDialogue[i] != null)
                {
                    string role, content;
                    tempDialogue[i].TryGetValue("role", out role);
                    tempDialogue[i].TryGetValue("content", out content);
                    if (role == "user")
                    {
                        Bubble newbubble = new Bubble(settings.UserName, content, true, null);
                        CreatBubble(newbubble);
                    }
                    else if (role == "assistant")
                    {
                        Bubble newbubble = new Bubble(settings.AIName, content, false, settings.Headsprite);
                        CreatBubble(newbubble);
                    }
                }
            }
        }
    }

    
}

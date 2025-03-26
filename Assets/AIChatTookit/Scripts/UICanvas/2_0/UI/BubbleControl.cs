using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BubbleControl : MonoBehaviour
{
    public GameObject AvaterBubble;
    public GameObject PlayerBubble;

    private float timer; //计时器
    private float BubbleStayTime = 10f;
    private void Start()
    {
        SetBubble(true, "想和我说些什么？");
    }
    public void SetBubble(bool isAvater,string text)
    {
        if (isAvater)
        {
            AvaterBubble.SetActive(true);
            PlayerBubble.SetActive(false);
            //AvaterBubble.GetComponentInChildren<Text>().text = text;
            //开始逐字打印
            StartTypeWords(text);
        }
        else
        {
            //AvaterBubble.SetActive(false);
            //PlayerBubble.SetActive(true);
            //PlayerBubble.GetComponentInChildren<Text>().text = text;
        }
    }

    //逐字显示的时间间隔
    [SerializeField] private float m_WordWaitTime = 0.1f;
    //是否显示完成
    [SerializeField] private bool m_WriteState = false;
    private void StartTypeWords(string msg)
    {
        if (msg == "")
            return;
        m_WriteState = true;
        StartCoroutine(SetTextPerWord(msg));
    }

    private IEnumerator SetTextPerWord(string msg)
    {
        int currentPos = 0;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //更新显示的内容
            AvaterBubble.GetComponentInChildren<Text>().text = msg.Substring(0, currentPos);
            m_WriteState = currentPos < msg.Length;
        }
    }

    private void Update()
    {
        if(AvaterBubble.activeInHierarchy && !m_WriteState)
        {
            //AvaterBubble存在状态且不在打印中
            timer += Time.deltaTime;
            if(timer> BubbleStayTime)
            {
                AvaterBubble.SetActive(false);
                timer = 0;
                //开始说新的话：
                Debug.Log("开始说新的话");
            }
        }
    }
}

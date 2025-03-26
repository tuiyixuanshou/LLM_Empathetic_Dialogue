using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BubbleControl : MonoBehaviour
{
    public GameObject AvaterBubble;
    public GameObject PlayerBubble;

    private float timer; //��ʱ��
    private float BubbleStayTime = 10f;
    private void Start()
    {
        SetBubble(true, "�����˵Щʲô��");
    }
    public void SetBubble(bool isAvater,string text)
    {
        if (isAvater)
        {
            AvaterBubble.SetActive(true);
            PlayerBubble.SetActive(false);
            //AvaterBubble.GetComponentInChildren<Text>().text = text;
            //��ʼ���ִ�ӡ
            StartTypeWords(text);
        }
        else
        {
            //AvaterBubble.SetActive(false);
            //PlayerBubble.SetActive(true);
            //PlayerBubble.GetComponentInChildren<Text>().text = text;
        }
    }

    //������ʾ��ʱ����
    [SerializeField] private float m_WordWaitTime = 0.1f;
    //�Ƿ���ʾ���
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
            //������ʾ������
            AvaterBubble.GetComponentInChildren<Text>().text = msg.Substring(0, currentPos);
            m_WriteState = currentPos < msg.Length;
        }
    }

    private void Update()
    {
        if(AvaterBubble.activeInHierarchy && !m_WriteState)
        {
            //AvaterBubble����״̬�Ҳ��ڴ�ӡ��
            timer += Time.deltaTime;
            if(timer> BubbleStayTime)
            {
                AvaterBubble.SetActive(false);
                timer = 0;
                //��ʼ˵�µĻ���
                Debug.Log("��ʼ˵�µĻ�");
            }
        }
    }
}

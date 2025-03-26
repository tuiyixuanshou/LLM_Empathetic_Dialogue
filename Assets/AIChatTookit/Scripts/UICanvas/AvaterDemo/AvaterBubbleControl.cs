using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvaterBubbleControl : MonoBehaviour
{
    public GameObject AvatarBubble;
    public Text AvatarBubbleText;
    private float timer;
    public float CompeletBubbleStay = 60f;
    private void Start()
    {
        AvatarBubbleText = AvatarBubble.GetComponentInChildren<Text>();
        AvatarBubbleText.text = string.Empty;
        AvatarBubble.SetActive(false);
    }

    private void Update()
    {
        //����Bubble�Զ��ر�
        if (AvatarBubble.activeInHierarchy && !m_WriteState)
        {
            timer += Time.deltaTime;
            if(timer> CompeletBubbleStay)
            {
                AvatarBubble.SetActive(false);
                timer = 0;
            }
        }
        else
        {
            timer = 0;
        }
    }

    //�ⲿ�����������
    public void SetUpAvatarBubble(string msg)
    {
        AvatarBubbleText.text = string.Empty;
        if (!AvatarBubble.activeInHierarchy)
        {
            AvatarBubble.SetActive(true);
        }
        StartTypeWords(msg);
    }

    //�û����������ݣ�avatar����ֱ����ʧ
    public void UserSendInput()
    {
        AvatarBubble.SetActive(false);
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
            AvatarBubbleText.text = msg.Substring(0, currentPos);
            m_WriteState = currentPos < msg.Length;
        }
    }
}

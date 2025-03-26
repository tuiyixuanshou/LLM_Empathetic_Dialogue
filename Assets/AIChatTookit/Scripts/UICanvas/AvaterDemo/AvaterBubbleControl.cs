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
        //控制Bubble自动关闭
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

    //外部调用这个即可
    public void SetUpAvatarBubble(string msg)
    {
        AvatarBubbleText.text = string.Empty;
        if (!AvatarBubble.activeInHierarchy)
        {
            AvatarBubble.SetActive(true);
        }
        StartTypeWords(msg);
    }

    //用户输入了内容，avatar气泡直接消失
    public void UserSendInput()
    {
        AvatarBubble.SetActive(false);
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
            AvatarBubbleText.text = msg.Substring(0, currentPos);
            m_WriteState = currentPos < msg.Length;
        }
    }
}

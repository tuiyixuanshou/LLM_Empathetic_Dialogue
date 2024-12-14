using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UI.Debug;
using WebGLSupport;
public class myChatSample : MonoBehaviour
{
    /// <summary>
    /// ����ģ��
    /// </summary>
    [Header("������Ҫ���ز�ͬ��llm�ű�")]
    [SerializeField] public LLM m_ChatModel;

    /// <summary>
    /// ���ص���Ϣ
    /// </summary>
    [SerializeField] private Text m_TextBack;

    /// <summary>
    /// ������Ϣ��ť,����������ɵ�whisper����
    /// </summary>
    [SerializeField] private Button m_CommitMsgBtn;

    [Header("��ʱ��prompt���")]
    public string TempPrompt;


    public void OnButtonPressed()
    {
        SendData(TempPrompt);
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    public void SendData(string RecognizeText)
    {
        if (RecognizeText.Equals(""))
        {
            Debug.Log("return null");
            return;
        }
            

        //��������
        m_ChatModel.PostMsg(RecognizeText, CallBack);
        //m_ChatSettings.m_ChatModel.PostMsg(RecognizeText, CallBack);

        //��Ҫ�޸ģ�whisper�������Ϊ0 m_InputWord.text = "";
        m_TextBack.text = "����˼����...";

        //�л�˼��������è�䶯���л�
        //SetAnimator("state", 1);
    }

    private void CallBack(string _response)
    {
        //ȥ���ַ������˿հ��ַ�
        _response = _response.Trim();
        m_TextBack.text = "";


        Debug.Log("�յ�AI�ظ���" + _response);

        //��ʼ�����ʾ���ص��ı�
        StartTypeWords(_response);
    }


    #region AI�ظ��ı��������������ʾ
    //������ʾ��ʱ����
    [SerializeField] private float m_WordWaitTime = 0.2f;
    //�Ƿ���ʾ���
    [SerializeField] private bool m_WriteState = false;

    /// <summary>
    /// ��ʼ�����ӡ
    /// </summary>
    /// <param name="_msg"></param>
    private void StartTypeWords(string _msg)
    {
        if (_msg == "")
            return;

        m_WriteState = true;
        StartCoroutine(SetTextPerWord(_msg));
    }

    private IEnumerator SetTextPerWord(string _msg)
    {
        int currentPos = 0;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //������ʾ������
            m_TextBack.text = _msg.Substring(0, currentPos);

            m_WriteState = currentPos < _msg.Length;

        }

        //�л����ȴ�����
        //SetAnimator("state", 0);
    }

    #endregion

}

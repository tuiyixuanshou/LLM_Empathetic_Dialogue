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
    /// 聊天模型
    /// </summary>
    [Header("根据需要挂载不同的llm脚本")]
    [SerializeField] public LLM m_ChatModel;

    /// <summary>
    /// 返回的信息
    /// </summary>
    [SerializeField] private Text m_TextBack;

    /// <summary>
    /// 发送信息按钮,这里把他集成到whisper里面
    /// </summary>
    [SerializeField] private Button m_CommitMsgBtn;

    [Header("暂时的prompt语句")]
    public string TempPrompt;


    public void OnButtonPressed()
    {
        SendData(TempPrompt);
    }

    /// <summary>
    /// 发送信息
    /// </summary>
    public void SendData(string RecognizeText)
    {
        if (RecognizeText.Equals(""))
        {
            Debug.Log("return null");
            return;
        }
            

        //发送数据
        m_ChatModel.PostMsg(RecognizeText, CallBack);
        //m_ChatSettings.m_ChatModel.PostMsg(RecognizeText, CallBack);

        //需要修改，whisper里面这个为0 m_InputWord.text = "";
        m_TextBack.text = "正在思考中...";

        //切换思考动作，猫咪动作切换
        //SetAnimator("state", 1);
    }

    private void CallBack(string _response)
    {
        //去除字符串两端空白字符
        _response = _response.Trim();
        m_TextBack.text = "";


        Debug.Log("收到AI回复：" + _response);

        //开始逐个显示返回的文本
        StartTypeWords(_response);
    }


    #region AI回复文本框中文字逐个显示
    //逐字显示的时间间隔
    [SerializeField] private float m_WordWaitTime = 0.2f;
    //是否显示完成
    [SerializeField] private bool m_WriteState = false;

    /// <summary>
    /// 开始逐个打印
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
            //更新显示的内容
            m_TextBack.text = _msg.Substring(0, currentPos);

            m_WriteState = currentPos < _msg.Length;

        }

        //切换到等待动作
        //SetAnimator("state", 0);
    }

    #endregion

}

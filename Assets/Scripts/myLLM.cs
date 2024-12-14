using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using UnityEngine;
using static LLM;

public class myLLM : MonoBehaviour
{
    /// <summary>
    /// api地址
    /// </summary>
    [SerializeField] protected string url;

    /// <summary>
    /// 提示词，与消息一起发送
    /// </summary>
    [Header("发送的提示词设定")]
    [SerializeField] protected string m_Prompt = string.Empty;

    /// <summary>
    /// 语言
    /// </summary
    [Header("设置回复的语言")]
    [SerializeField] protected string lan = "中文";

    /// <summary>
    /// 计算方法调用的时间
    /// </summary>
    [SerializeField] protected Stopwatch stopwatch = new Stopwatch();

    /// <summary>
    /// 发送消息
    /// </summary>
    public virtual void PostMsg(string _msg, Action<string> _callback)
    {
        //提示词处理
        string message = "当前为角色的人物设定：" + m_Prompt +
            " 回答的语言：" + lan +
            " 接下来是我的提问：" + _msg;

        StartCoroutine(Request(message, _callback));
    }

    public virtual IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        yield return new WaitForEndOfFrame();

    }
}

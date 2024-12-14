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
    /// api��ַ
    /// </summary>
    [SerializeField] protected string url;

    /// <summary>
    /// ��ʾ�ʣ�����Ϣһ����
    /// </summary>
    [Header("���͵���ʾ���趨")]
    [SerializeField] protected string m_Prompt = string.Empty;

    /// <summary>
    /// ����
    /// </summary
    [Header("���ûظ�������")]
    [SerializeField] protected string lan = "����";

    /// <summary>
    /// ���㷽�����õ�ʱ��
    /// </summary>
    [SerializeField] protected Stopwatch stopwatch = new Stopwatch();

    /// <summary>
    /// ������Ϣ
    /// </summary>
    public virtual void PostMsg(string _msg, Action<string> _callback)
    {
        //��ʾ�ʴ���
        string message = "��ǰΪ��ɫ�������趨��" + m_Prompt +
            " �ش�����ԣ�" + lan +
            " ���������ҵ����ʣ�" + _msg;

        StartCoroutine(Request(message, _callback));
    }

    public virtual IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        yield return new WaitForEndOfFrame();

    }
}

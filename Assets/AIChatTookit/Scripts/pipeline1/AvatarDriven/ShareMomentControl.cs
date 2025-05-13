using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtilities;
using Newtonsoft.Json;

public class ShareMomentControl : MonoBehaviour
{
    public Settings settings;
    public SimpleFileReader reader;
    [Header("�����ߵ�ģ��")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;

    public int MomentIndex = 0;  //����ʱ����������
    public bool IS_Unexpect = false;

    public List<Event_Object> World_Objects = new();  //�˹�����¼�
    List<Dictionary<string, string>> ADeciSion_Dial = new();

    [Header("��������")]
    public AvatarMainStoryDemoV2 MainStory;
    private void Start()
    {
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
    }

    void Add_SystemPrompt(string prompt)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",prompt }
        };
        ADeciSion_Dial.Add(newmmessage);
    }


    public void ShareMomentStart()
    {
        MomentIndex++;
        MainStory.MomentIndex = MomentIndex;
        if (IS_Unexpect)
        {
            MainStory.AUnexpect_Event();
            StartCoroutine(WaitForUnexpectEvent(StartDecision));
        }
        else
        {
            StartDecision();
        }

    }

    private IEnumerator WaitForUnexpectEvent(Action<string> callback)
    {
        while (true)
        {
            var found = MainStory.Unexpect_Objects.Find(obj => obj.event_index == MomentIndex);
            if (found != null)
            {
                callback?.Invoke(found.Event);
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void StartDecision(string Unexpect = null)
    {
        //�������״������ʼ���о���
        string Plan = ListString.ListToString(MainStory.CurMon_Plan[MomentIndex - 1].Event);
        string World_Plan = null;
        var world_Event = World_Objects.Find(obj => obj.event_index == MomentIndex);
        if (world_Event != null)
        {
            World_Plan = world_Event.Event;
        }

        string prompt = $@"��������Ϊ����У԰ѧ�飬��ģ���ѧ������ķ�ʽ���������û���飬���л���ߡ�
����ԭ��1.����������û��¼������Ի�Ӧ�û��¼�Ϊ����2.���������ʵ�������ݣ��Ի�Ӧ��ʵ��������Ϊ����3.�ƻ���ͻ���¼�֮����Ҫ����Ȩ�⣬˼��ͻ���¼��Ƿ��Ӱ��ƻ����Լ��Ƿ���Ҫ��Ӧͻ���¼���
���߻�Ӧ���ȼ����û��¼�>��ʵ��������>�ƻ�=ͻ���¼������վ������ݷ��������ݣ��������
���ԡ�����ߣ�xxxx���ĸ�ʽ�����������40�֡�
�û��¼���{null}
��ʵ�������ݣ�{World_Plan}
�ƻ���{Plan}
ͻ���¼���{Unexpect}";

        Debug.Log(prompt);

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        ADeciSion_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = ADeciSion_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, StartDecision_Callback));
    }

    void StartDecision_Callback(string text)
    {
        Debug.Log("�¼�������" + MomentIndex + "  �����:" + text);
    }






    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }
}

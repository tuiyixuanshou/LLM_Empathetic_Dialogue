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
    public API_CentralControl api_CentralControl;
    [Header("�����ߵ�ģ��")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;
    [Header("chatģ������")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;


    public int MomentIndex = 0;  //����ʱ����������
    public bool IS_Unexpect = false;

    public List<Event_Object> World_Objects = new();  //��ʵ�������ݣ��˹�����
    public List<Event_Object> User_Objects = new(); //�û��������ݣ�ÿ�ζԻ�������Զ��ܽ�

    public ShareMomentDetail shareMomentDetail;

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
        string user_Plan = null;
        var user_Event = User_Objects.Find(obj => obj.event_index == MomentIndex);
        if (user_Event != null)
        {
            user_Plan = user_Event.Event;
        }

        shareMomentDetail.User_Event = user_Plan;
        shareMomentDetail.World_Plan = World_Plan;
        shareMomentDetail.Unexpect = Unexpect;
        shareMomentDetail.Plan = Plan;

        string prompt = $@"��������Ϊ����У԰ѧ�飬��ģ���ѧ������ķ�ʽ���������û���飬���л���ߡ�
����ԭ��1.����������û��¼������Ի�Ӧ�û��¼�Ϊ����2.���������ʵ�������ݣ��Ի�Ӧ��ʵ��������Ϊ����3.�������ٵ�ͻ���¼���ԭ�мƻ�����Ȩ�⣬˼��ͻ���¼��Ƿ��Ӱ��ƻ����Լ��Ƿ���Ҫ��Ӧͻ���¼���
���߻�Ӧ���ȼ����û��¼�>��ʵ��������>�ƻ�=ͻ���¼������վ������ݷ��������ݣ��������
���ԡ�����ߣ�xxxx���ĸ�ʽ�����������40�֡�
�û��¼���{user_Plan}
��ʵ�������ݣ�{World_Plan}
��ļƻ���{Plan}
�����ٵ�ͻ���¼���{Unexpect}";

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
        shareMomentDetail.Decision = text;
        SceneAndAction_Decision();
    }


    void SceneAndAction_Decision()
    {
        //��Ծ��ߣ���ʼ���г�����������

        string prompt = $@"�������˹��ļƻ���{shareMomentDetail.Plan}
�������˹����ٵ�ͻ���¼���{shareMomentDetail.Unexpect}
�������˹���Ե���ʵ����ʱ�䣺{shareMomentDetail.World_Plan}
�������˹���Ե����û��¼���{shareMomentDetail.User_Event}
�������˹���������Ļ���ߣ�{shareMomentDetail.Decision}
�ۺ�������Ϣ��˼�����˹������ڵ�һ���������򵥶����������������ݽ����ο����ԡ���xxxxx��ʲô������xxx��Ϊģ�巵�أ����ϴ�ѧ�������зḻ�ĳ����������ʵ����һЩ��������࣬����ͨ˳��������30��";

        Debug.Log(prompt);

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        List<Dictionary<string,string>> cur_MessageList = new();
        cur_MessageList.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = cur_MessageList,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, SceneAndAction_Decision_Callback));
    }

    void SceneAndAction_Decision_Callback(string text)
    {
        Debug.Log("�¼�������" + MomentIndex + "��������:" + text);
        shareMomentDetail.Scene_Decision = text;

        //����һЩ��ģ̬�ķ������ݣ��������첽����
        //����������ͬ����Ƶ�������ݡ�
        //��ͼ�������ɺ�̨�˹����

        StartCoroutine(ShareMoment_ImageCreat(text));


    }

    //�Ի���������е���
    public void AddUser_Objects(string text)
    {
        User_Objects.Add(new Event_Object
        {
            event_index = MomentIndex+1,
            Event = text
        });
    }


    IEnumerator ShareMoment_ImageCreat(string prompt)
    {
        yield return api_CentralControl.api_ImageCreat.SendPrompt(prompt);
        yield return api_CentralControl.api_ImageCreat.CheckImageStatusRepeatedly();
        api_CentralControl.api_Action.VideoPrompt();
        //�����жԻ�չʾ
        //api_CentralControl.api_Chat.Mchat_API_FreePrompt("", true, Mchat_Model, Mchat_url, Mchat_api);
    }


    [Serializable]
    public class ShareMomentDetail
    {
        public string User_Event;
        public string Plan;
        public string World_Plan;
        public string Unexpect;
        public string Decision;
        public string Scene_Decision;
        //public List<string> Action_Decision;
    }


    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }
}

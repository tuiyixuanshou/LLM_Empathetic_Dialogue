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

    public int timeIndex = 0;  //����ʱ����������


    public List<string> User_Important_Info = new(); //�û��ش��¼���¼

    public List<string> Scene_Desicribe = new();

    public ShareMomentDetail shareMomentDetail;

    List<Dictionary<string, string>> ADeciSion_Dial = new();

    [Header("��������")]
    public AvatarMainStoryDemoV2 MainStory;
    private void Start()
    {
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
        SceneAndAction_Decision_Callback("��У԰��������ѡ�˶����Ϻ��㽶������רע����");
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
        //MomentIndex++;
        //MainStory.MomentIndex = MomentIndex;
        //if (IS_Unexpect)
        //{
        //    MainStory.AUnexpect_Event();
        //    StartCoroutine(WaitForUnexpectEvent(StartDecision));
        //}
        //else
        //{
        //    StartDecision();
        //}

    }

    private IEnumerator WaitForUnexpectEvent(Action<string> callback)
    {
        //while (true)
        //{
        //    var found = MainStory.Unexpect_Objects.Find(obj => obj.event_index == MomentIndex);
        //    if (found != null)
        //    {
        //        callback?.Invoke(found.Event);
        //        yield break;
        //    }
        //    yield return new WaitForSeconds(0.5f);
        //}
        yield return null;
    }

    void StartDecision(string Unexpect = null)
    {

        string prompt = $@"��������Ϊ����У԰ѧ�飬��ģ���ѧ������ķ�ʽ���������û���飬���л���ߡ�
����ԭ��1.����������û��¼������Ի�Ӧ�û��¼�Ϊ����2.���������ʵ�������ݣ��Ի�Ӧ��ʵ��������Ϊ����3.�������ٵ�ͻ���¼���ԭ�мƻ�����Ȩ�⣬˼��ͻ���¼��Ƿ��Ӱ��ƻ����Լ��Ƿ���Ҫ��Ӧͻ���¼���
���߻�Ӧ���ȼ����û��¼�>��ʵ��������>�ƻ�=ͻ���¼������վ������ݷ��������ݣ��������
���ԡ�����ߣ�xxxx���ĸ�ʽ�����������40�֡�
�û��¼���
��ʵ�������ݣ�

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
        //Debug.Log("�¼�������" + MomentIndex + "  �����:" + text);
        //shareMomentDetail.Decision = text;
        //SceneAndAction_Decision();
    }


    public void Start_SceneAndAction()
    {
        if (timeIndex < 3)
        {
            SceneAndAction_Decision();
        }
        else
        {
            Debug.Log("ͬһ�������ͼƬ���������");
        }
    }

    void SceneAndAction_Decision()
    {
        //��Ի�ƻ������û��¼�����ʼ���г�����������
        //shareMomentDetail.Behavior_title = MainStory.concreteBehaviors[timeIndex].title;
        //shareMomentDetail.Behavior_description = MainStory.concreteBehaviors[timeIndex].description;

        string prompt = $@"���ǽ�ɫ���ܼƻ�������ƣ�{MainStory.concreteBehaviors[timeIndex].title}
��ļ�����{MainStory.concreteBehaviors[timeIndex].description}
��ص㣺{MainStory.concreteBehaviors[timeIndex].location}
�����û��ϴζԻ��е��ش��¼������ݣ�����
- ����ӽ�ɫ�ƻ�������ơ������͵ص��У���ȡ��Ŀǰ��ɫ���ڻ�������ɫ��̬
- ���Ǵ����û��Ի����û����ش��¼���˼���ܹ���Ӧ�û��Ľ�ɫ��������ɫ��̬
�ۺ��������㣬�������ɽ�ɫ�Ӿ�������prompt���ԡ���xxxxx��ʲô������xxx��Ϊģ�巵�أ����ϴ�ѧ�������зḻ�ĳ����������ʵ����һЩ��������࣬����ͨ˳��������30��";

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
        Debug.Log("��������������:" + text);
        Scene_Desicribe.Add(text); //��ӵ����������У�����Ѱ�Ҵ洢��ͼƬ����Ƶ����
        var scene = settings.Share_Scenes_List.Find(s => s.timeIndex == timeIndex);
        scene.Scene_Describe = text;


        //����һЩ��ģ̬�ķ������ݣ��������첽����
        //����������ͬ����Ƶ�������ݡ�
        //��ͼ�������ɺ�̨�˹����

        StartCoroutine(ShareMoment_ImageCreat(text));


    }

    //�Ի���������е���
    public void AddUser_Objects(string text)
    {
        User_Important_Info.Add(text);
    }


    IEnumerator ShareMoment_ImageCreat(string prompt)
    {
        yield return api_CentralControl.api_ImageCreat.SendPrompt(prompt);
        yield return api_CentralControl.api_ImageCreat.CheckImageStatusRepeatedly();
        api_CentralControl.api_Action.VideoPrompt(timeIndex);
        timeIndex++;
        //�����жԻ�չʾ
        //api_CentralControl.api_Chat.Mchat_API_FreePrompt("", true, Mchat_Model, Mchat_url, Mchat_api);
    }


    [Serializable]
    public class ShareMomentDetail
    {
        public string Behavior_title;
        public string Behavior_description;
        public string World_Plan;
        public string Scene_Decision;
    }


    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }
}

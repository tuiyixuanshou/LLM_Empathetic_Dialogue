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
    [Header("决策者的模型")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;

    public int MomentIndex = 0;  //分享时刻数字索引
    public bool IS_Unexpect = false;

    public List<Event_Object> World_Objects = new();  //人工添加事件
    List<Dictionary<string, string>> ADeciSion_Dial = new();

    [Header("其他部件")]
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
        //面对所有状况，开始进行决策
        string Plan = ListString.ListToString(MainStory.CurMon_Plan[MomentIndex - 1].Event);
        string World_Plan = null;
        var world_Event = World_Objects.Find(obj => obj.event_index == MomentIndex);
        if (world_Event != null)
        {
            World_Plan = world_Event.Event;
        }

        string prompt = $@"你现在做为治愈校园学伴，用模拟大学生生活的方式主动进行用户陪伴，进行活动决策。
决策原则：1.如果出现了用户事件，则以回应用户事件为主；2.如果出现真实世界内容，以回应真实世界内容为主；3.计划和突发事件之间需要做出权衡，思考突发事件是否会影响计划，以及是否需要回应突发事件。
决策回应优先级：用户事件>真实世界内容>计划=突发事件。最终决策内容符合你的身份，贴近生活。
请以“活动决策：xxxx”的格式输出，不超过40字。
用户事件：{null}
真实世界内容：{World_Plan}
计划：{Plan}
突发事件：{Unexpect}";

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
        Debug.Log("事件索引：" + MomentIndex + "  活动决策:" + text);
    }






    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }
}

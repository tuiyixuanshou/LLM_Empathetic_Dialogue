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
    [Header("决策者的模型")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;
    [Header("chat模型设置")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;


    public int MomentIndex = 0;  //分享时刻数字索引
    public bool IS_Unexpect = false;

    public List<Event_Object> World_Objects = new();  //真实世界内容，人工输入
    public List<Event_Object> User_Objects = new(); //用户反馈内容，每次对话后进行自动总结

    public ShareMomentDetail shareMomentDetail;

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

        string prompt = $@"你现在做为治愈校园学伴，用模拟大学生生活的方式主动进行用户陪伴，进行活动决策。
决策原则：1.如果出现了用户事件，则以回应用户事件为主；2.如果出现真实世界内容，以回应真实世界内容为主；3.对你面临的突发事件和原有计划进行权衡，思考突发事件是否会影响计划，以及是否需要回应突发事件。
决策回应优先级：用户事件>真实世界内容>计划=突发事件。最终决策内容符合你的身份，贴近生活。
请以“活动决策：xxxx”的格式输出，不超过40字。
用户事件：{user_Plan}
真实世界内容：{World_Plan}
你的计划：{Plan}
你面临的突发事件：{Unexpect}";

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
        shareMomentDetail.Decision = text;
        SceneAndAction_Decision();
    }


    void SceneAndAction_Decision()
    {
        //面对决策，开始进行场景内容生成

        string prompt = $@"这是主人公的计划：{shareMomentDetail.Plan}
这是主人公面临的突发事件：{shareMomentDetail.Unexpect}
这是主人公面对的真实世界时间：{shareMomentDetail.World_Plan}
这是主人公面对的其用户事件：{shareMomentDetail.User_Event}
这是主人公最后做出的活动决策：{shareMomentDetail.Decision}
综合上述信息，思考主人公所存在的一条场景及简单动作描述。分享内容仅做参考。以“在xxxxx做什么，表情xxx”为模板返回，符合大学生生活中丰富的场景，可以适当添加一些描述，简洁，内容通顺，不超过30字";

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
        Debug.Log("事件索引：" + MomentIndex + "场景描述:" + text);
        shareMomentDetail.Scene_Decision = text;

        //进行一些多模态的反馈内容，可以用异步操作
        //这里会包含不同的视频生成内容。
        //新图的生成由后台人工完成

        StartCoroutine(ShareMoment_ImageCreat(text));


    }

    //对话结束后进行调用
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
        //最后进行对话展示
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

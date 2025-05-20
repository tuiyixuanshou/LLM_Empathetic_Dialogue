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

    public int timeIndex = 0;  //分享时刻数字索引


    public List<string> User_Important_Info = new(); //用户重大事件记录

    public List<string> Scene_Desicribe = new();

    public ShareMomentDetail shareMomentDetail;

    List<Dictionary<string, string>> ADeciSion_Dial = new();

    [Header("其他部件")]
    public AvatarMainStoryDemoV2 MainStory;
    private void Start()
    {
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
        SceneAndAction_Decision_Callback("在校园便利店挑选运动饮料和香蕉，表情专注认真");
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

        string prompt = $@"你现在做为治愈校园学伴，用模拟大学生生活的方式主动进行用户陪伴，进行活动决策。
决策原则：1.如果出现了用户事件，则以回应用户事件为主；2.如果出现真实世界内容，以回应真实世界内容为主；3.对你面临的突发事件和原有计划进行权衡，思考突发事件是否会影响计划，以及是否需要回应突发事件。
决策回应优先级：用户事件>真实世界内容>计划=突发事件。最终决策内容符合你的身份，贴近生活。
请以“活动决策：xxxx”的格式输出，不超过40字。
用户事件：
真实世界内容：

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
        //Debug.Log("事件索引：" + MomentIndex + "  活动决策:" + text);
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
            Debug.Log("同一天的内容图片已生成完毕");
        }
    }

    void SceneAndAction_Decision()
    {
        //面对活动计划，和用户事件，开始进行场景内容生成
        //shareMomentDetail.Behavior_title = MainStory.concreteBehaviors[timeIndex].title;
        //shareMomentDetail.Behavior_description = MainStory.concreteBehaviors[timeIndex].description;

        string prompt = $@"这是角色本周计划活动的名称：{MainStory.concreteBehaviors[timeIndex].title}
活动的简述：{MainStory.concreteBehaviors[timeIndex].description}
活动地点：{MainStory.concreteBehaviors[timeIndex].location}
这是用户上次对话中的重大事件的内容：暂无
- 请你从角色计划活动的名称、简述和地点中，提取出目前角色所在环境、角色姿态
- 若是存在用户对话中用户的重大事件，思考能够回应用户的角色环境、角色姿态
综合以上两点，输入生成角色视觉场景的prompt。以“在xxxxx做什么，表情xxx”为模板返回，符合大学生生活中丰富的场景，可以适当添加一些描述，简洁，内容通顺，不超过30字";

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
        Debug.Log("场景和内容描述:" + text);
        Scene_Desicribe.Add(text); //添加到场景索引中，方便寻找存储的图片和视频内容
        var scene = settings.Share_Scenes_List.Find(s => s.timeIndex == timeIndex);
        scene.Scene_Describe = text;


        //进行一些多模态的反馈内容，可以用异步操作
        //这里会包含不同的视频生成内容。
        //新图的生成由后台人工完成

        StartCoroutine(ShareMoment_ImageCreat(text));


    }

    //对话结束后进行调用
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
        //最后进行对话展示
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

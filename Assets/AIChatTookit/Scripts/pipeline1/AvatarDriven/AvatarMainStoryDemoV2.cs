using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MyUtilities;
using static AvatarMainStoryDemo;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

public class AvatarMainStoryDemoV2 : MonoBehaviour
{
    public ShareMomentControl shareMomentControl;
    public Settings settings;
    public SimpleFileReader reader;
    public API_Chat api_chat;

    [Header("Agent上下文维护")]
    List<Dictionary<string, string>> APlan_Dial= new();
    List<Dictionary<string, string>> AEvent_Dial= new();  //外部事件生成、动态调整
    List<Dictionary<string, string>> AUnexpect_Dial = new();  //突发事件派发

    //这个直接加入Setting中的tempDialogue，tempDialogue有RAG可以使用
    List<Dictionary<string, string>> ARespond_Dial = new();

    [Header("生成模型")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;

    [Header("Avatar轨迹生成数据内容")]
    [TextArea(5,5)]
    public string Event_Pool;
    public int MomentIndex;
    public List<Event_Object> event_Objects ;
    public List<Event_Object> Unexpect_Objects;
    public List<Target_Object> Avatar_Target;
    public List<Target_WeightObject> Avatar_TargetWeight;

    public List<Week_Plan> CurMon_Plan;

    private void Start()
    {
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
    }

    /// <summary>
    /// 异步调用system prompt之后开始生成虚拟线计划
    /// </summary>
    /// <param name="prompt"></param>
    void Add_SystemPrompt(string prompt)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        AEvent_Dial.Add(newmmessage);
        AUnexpect_Dial.Add(newmmessage);
        APlan_Target();
        //AEvent_Generation();
    }

    public void UpdateControl()
    {
        //settings 的 monthindex和week index的控制，顺便进行计划更新内容。
        //开始时：总计划更新、总事件安排更新、权重更新、最小单位生成更新+周应对更新
        //月数字+1：最小单位生成更新+周应对更新
        //周数字+1：周应对更新
        if (settings.month_Index > 3) 
        { 
            Debug.LogError("停止！！！！");
        }
        if (settings.week_Index< 4)
        {
            settings.week_Index++;
            api_chat.Avatar_ProActive_Chat();
        }
        else
        {
            settings.month_Index++;
            settings.week_Index = 1;
            APlan_SpecifyPlan();
        }
    }

    public void APlan_Target()
    {
        Debug.Log("开始总目标生成");
        string prompt = $@"模仿一个大学生，生成大学生的多维度核心目标。其中目标维度分为：
自我规划类，我已经知道自己真正想要追求什么
志业追求类，我立志要在自己的专业或职业领域中做出突出成就
超我贡献类，我会从改善社会和人类福祉的角度考虑人生规划
这三类又被细分为四个导向：学业成就、职业准备、个人成长、社交关系。
请综合以上的信息，生成四个导向的具体目标追求。
注意：目标追求应该是一个具体、需长期努力、贴合大学生实际生活的事件。只需要生成一组目标即可，简洁明了。
以Json的格式回复：[
{{
""学业成就"":""目标内容""，
""职业准备"":""目标内容""，
""个人成长"":""目标内容""，
""社交关系"":""目标内容""
}}
]
请不要返回除Json数据以外的任何内容";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = APlan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, APlan_Target_CallBack));
    }

    void APlan_Target_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        APlan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("解析Json");
            Avatar_Target = JsonConvert.DeserializeObject<List<Target_Object>>(Json);
            //Debug.Log(Avatar_Target[0].职业准备导向);
            APlan_SpecifyPlan();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }
    }


    public void AEvent_Generation()
    {
        Debug.Log("开始外部事件生成");
        string prompt = $@"作为事件安排系统，安排7个客观事件，这些时间是System prompt中模仿的大学生可能会遇见的。
事件发生时间的起点以system prompt中提供的真实时间为准，7个事件按时间顺序排列。
可以包含：校内事件、好友事件、社会事件
校内事件：在大学中会遇到的事件，多半由学校各组织安排
好友事件：社交群体中会发生的事件，如会议、计划旅游等 
社会事件：社会外界事件 
不需要生成人物反应，只需要生成客观事件即可。
输出格式为Json格式，按顺序输出，如
[
    {{""event_index"":1,""Event"":""<输入事件内容>""}},
    {{""event_index"":2,""Event"":""<输入事件内容>""}},
]
请不要返回除Json数据以外的任何内容";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        AEvent_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = AEvent_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, AEvent_Generation_CallBack));
    }

    void AEvent_Generation_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        AEvent_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        event_Objects = JsonConvert.DeserializeObject<List<Event_Object>>(Json);

        //收集安排事件，用作Agent-Plan生成指导
        //APlan_TargetWeigh();
    }

    public void APlan_TargetWeigh()
    {
        //这里我会给你提供目前已知的每个月的事件安排：{ListString.ListToString<Event_Object>(expose_EventObjects)}
        Debug.Log("开始非最小事件单位目标权重生成");
        string prompt = $@"模仿一个大学生，生成三个月中的核心目标权重分布，每月四种目标的权重总和为1。
四个导向分别为：学业成就、职业准备、个人成长、社交关系。
综合每月的事件，生成每个月的月目标权重。
请你综合每月的事件，生成每个月的月目标权重。
请以Json格式进行回复：[
{{
""month_index"":1,
""学业成就"":0.1,
""职业准备"":0.1,
""个人成长"":0.1,
""社交关系"":0.1
}}
]
请不要返回除Json数据以外的任何内容";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = APlan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, APlan_TargetWeigh_CallBack));
    }

    void APlan_TargetWeigh_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        APlan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("解析Json");
            Avatar_TargetWeight = JsonConvert.DeserializeObject<List<Target_WeightObject>>(Json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }
        APlan_SpecifyPlan();
    }

    public void APlan_SpecifyPlan(Action CallBack = null)
    {
        Debug.Log("开始进行计划生成");
        //List<Target_WeightObject> newWeigh = new();
        //newWeigh.Add(Avatar_TargetWeight[settings.month_Index - 1]);
        //你的目标:{ListString.ListToString(Avatar_Target)}
        string prompt = $@"现在你模仿大学生，来生成接下来7个时间点的计划。计划会受到你的性格、环境、目标等多方面影响。
计划时间的起点以system prompt中提供的真实时间为准，7个事件按时间顺序排列。
你需要在事件池中，选择事件安排.事件池：{Event_Pool}.
目标导向会对事件选择产生影响，具体影响为：
学术成就：学习培训占比显著增加，自由支配活动被压缩，无酬劳动简化
职业准备：有酬劳动/技能学习优先级提升，自由支配活动偏向实用化，交通活动范围扩大
个人成长导向：自由支配活动高度结构化，生理必需活动优化，学习培训可能让步
社交关系导向：自由支配活动社交化，无酬劳动协作性增强，学习培训效率要求提高
由1或2个核心目标作为导向，作为该时间点下的计划事件。
请你选择每个时间点的计划事件，计划事件将链接成你的活动主线。回复内容请用Json格式
[
{{""Index"":1,
""Event"":[{{""type"":""<选取事件的类型，如<学习培训><自由支配活动>>"",""driven_type"":""<说明导向类型，如学术成就、职业准备等>""""specify_event"":""<描述事件>""}}，{{”type“:""<选取事件的类型，如<学习培训><自由支配活动>>"",""driven_type"":""<说明导向类型，如学术成就、职业准备等>""""specify_event"":""<描述事件>""}}]}},
]
请不要返回除Json数据以外的任何内容";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = APlan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, APlan_SpecifyPlan_CallBack));
    }

    void APlan_SpecifyPlan_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        APlan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("解析Json");
            CurMon_Plan = JsonConvert.DeserializeObject<List<Week_Plan>>(Json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }

        //直接让他调用主动对话
        //api_chat.Avatar_ProActive_Chat();
        Debug.Log("进行第一次share moment");
        shareMomentControl.ShareMomentStart();
    }

    public void AUnexpect_Event(Action CallBack = null)
    {
        Debug.Log("开始突发事件派送");
        reader.LoadFile("Unexpect_Pool.json", Add_Unexpect);
    }
    void Add_Unexpect(string prompt)
    {
        string full_prompt = $@"你是一个分配突发事件的agent，请你根据模仿画像，分配其该时间点可能遇见的突发事件。
我会给你提供一个突发事件池作为你的参考。仅返回一个突发事件内容即可。突发事件池："+ prompt;

        string simplePrompt = "继续分配下一时间点的突发事件内容。";

        string m_Prompt = AUnexpect_Dial.Count > 1 ? simplePrompt : full_prompt;

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",m_Prompt }
        };
        AUnexpect_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = AUnexpect_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, AUnexpect_Event_CallBack));
    }

    void AUnexpect_Event_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        AUnexpect_Dial.Add(newmmessage);
        Debug.Log("突发事件：" + text);
        Unexpect_Objects.Add(new Event_Object
        {
            event_index = MomentIndex, 
            Event = text
        });
    }
   


    string JsonPatch(string rawText)
    {
        try
        {
            // 先用平衡组提取最外层 [...]
            string pattern = @"\[(?:[^[\]]+|(?<open>\[)|(?<-open>\]))+(?(open)(?!))\]";
            Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);
            if (!match.Success) return null;
            Debug.Log("提纯Json"+match.Value);
            // 再解析提取后的 JSON
            return match.Value;
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析失败：{ex.Message}");
            return null;
        }
    }
    #region 解析目标
    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }

    [Serializable]
    public class Target_Object
    {
        public string 学业成就;
        public string 职业准备;
        public string 个人成长;
        public string 社交关系;
    }

    [Serializable]
    public class Target_WeightObject
    {
        public int month_index;
        public float 学业成就;
        public float 职业准备;
        public float 个人成长;
        public float 社交关系;
    }
    [Serializable]
    public class Week_Plan
    {
        public int Index;
        public List<Week_Event> Event;
    }
    [Serializable]
    public class Week_Event
    {
        public string type;
        public string driven_type;
        public string specify_event;
    }
    #endregion

}


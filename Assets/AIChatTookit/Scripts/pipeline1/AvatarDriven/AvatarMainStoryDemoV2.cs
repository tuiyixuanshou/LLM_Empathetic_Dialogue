using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MyUtilities;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using static AvatarMainStoryDemoV2;

public class AvatarMainStoryDemoV2 : MonoBehaviour
{
    public ShareMomentControl shareMomentControl;
    public Settings settings;
    public SimpleFileReader reader;
    public API_Chat api_chat;

    [Header("Agent上下文维护")]
    List<Dictionary<string, string>> A_Status_Dial= new();
    List<Dictionary<string, string>> A_Eval_Dial = new();
    List<Dictionary<string, string>> A_Plan_Dial= new();  //
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
    public int timeIndex = 0;
    public string World_Object;
    public List<Status_Object> Avatar_Status; //Avatar状态
    public List<Eval_WeightObject> Eval_Weight; //Avatar行为衡量标准
    public List<A_ConcreteBehavior> plan_Objects ;//抽象行为生成后的筛选行为
    public List<A_ConcreteBehavior> concreteBehaviors; //一日具体行为

    private List<AbstractBehavior> Abstract_behaviors; //抽象类事件池

    //public List<Event_Object> Unexpect_Objects;

    private void Awake()
    {
        StartSampleandConcreteBehavior();
    }
    private void Start()
    {
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
        reader.LoadFile("Abstract_Event_Pool.json", Abstract_EventPoolSet);
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
        A_Status_Dial.Add(newmmessage);
        A_Eval_Dial.Add(newmmessage);
        A_Plan_Dial.Add(newmmessage);

        AUnexpect_Dial.Add(newmmessage);
        //A_Status_t();
    }

    void Abstract_EventPoolSet(string text)
    {
        Abstract_behaviors = JsonConvert.DeserializeObject<List<AbstractBehavior>>(text);
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
            //A_Eval_t();
        }
    }

    public void A_Status_t()
    {
        Debug.Log("开始角色状态生成");
        string prompt = $@"你正在模拟一个虚拟大学生的日常生活系统。
请你根据当前的时间与情境，合理推测该虚拟人物在一天中上午、下午、晚上三个时间段的心理与生理状态。
请生成以下结构化信息：
1. 时间段（生成的三组内容分别代表上午、下午、晚上时间段）
2. 精力（0-100之间的整数，代表当前的体力与精神状态）
3. 心情（-5至5的整数，代表角色的情绪波动，负值为不开心，正值为开心）
4. 压力（0.0 到 1.0，代表心理压力水平）
5. 欲望（分别列出对以下方面的当前驱动力，数值范围为-1.0到1.0）：
   - 放松
   - 学习
   - 社交
   - 运动
   - 自我提升

请用以下 JSON 结构输出：
[
{{
  ""时间"": ""上午"",
  ""精力"": ,
  ""心情"": ,
  ""压力"": ,
  ""欲望"": {{
    ""放松"": ,
    ""学习"": ,
    ""社交"": ,
    ""运动"": ,
    ""自我提升"": 
  }}
}},
{{
  ""时间"": ""下午"",
  ""精力"": ,
  ""心情"": ,
  ""压力"": ,
  ""欲望"": {{
    ""放松"": ,
    ""学习"": ,
    ""社交"": ,
    ""运动"": ,
    ""自我提升"": 
  }}
}},
{{
  ""时间"": ""晚上"",
  ""精力"": ,
  ""心情"": ,
  ""压力"": ,
  ""欲望"": {{
    ""放松"": ,
    ""学习"": ,
    ""社交"": ,
    ""运动"": ,
    ""自我提升"": 
  }}
}}
]
请不要返回除Json数据以外的任何内容";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        A_Status_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = A_Status_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, A_Status_t_CallBack));
    }

    void A_Status_t_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        A_Status_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("解析Json");
            Avatar_Status = JsonConvert.DeserializeObject<List<Status_Object>>(Json);
            A_Eval_t(timeIndex);

        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }
    }

    public void A_Eval_t(int timeIndex)
    {
        Debug.Log("开始衡量标准权重生成");
        //List<Target_WeightObject> newWeigh = new();
        //newWeigh.Add(Avatar_TargetWeight[settings.month_Index - 1]);
        //你的目标:{ListString.ListToString(Avatar_Status)}
        string prompt = $@"你是一个虚拟行为权重生成器，任务是根据角色的性格、环境、心理与生理状态，以及外部发生的事件，为其生成当下的行为偏好分布。
【当前角色状态】
- 时间：{Avatar_Status[timeIndex].时间}
- 精力值：{Avatar_Status[timeIndex].精力}（0-100，越高代表越有体力）
- 心情值：{Avatar_Status[timeIndex].心情}（-5至5，负值代表情绪低落）
- 压力水平：{Avatar_Status[timeIndex].压力}（0.0-1.0，越高表示压力越大）
- 欲望值：
  - 放松：{Avatar_Status[timeIndex].欲望.放松}
  - 学习：{Avatar_Status[timeIndex].欲望.学习}
  - 社交：{Avatar_Status[timeIndex].欲望.社交}
  - 运动：{Avatar_Status[timeIndex].欲望.运动}
  - 自我提升：{Avatar_Status[timeIndex].欲望.自我提升}
【外部发生事件】
{World_Object}（请分析对于角色性格而言，该事件会对行为权重产生的影响）

请根据以上状态，合理生成一个行为打分权重，表示当前角色在五种行为上的倾向。注意：
- 如果角色精力较低、压力较大，通常更偏好“放松”
- 如果角色心情好、精力充足，自我提升或运动倾向可能更高
- 欲望值是角色内在驱动，需优先参考，但也要考虑精力与压力的调节作用
- 外部事件是当天真实世界中发生的事件，请综合角色性格、心理、状态，考虑外部事件对于角色活动行为权重的影响

请输出如下JSON 结构（值范围0.0~1.0）：
[
{{
  ""放松"": ,
  ""学习"": ,
  ""社交"": ,
  ""运动"": ,
  ""自我提升"": ,
  ""解释"":""""
}}
]";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        A_Eval_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = A_Eval_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, A_Eval_CallBack));
    }

    void A_Eval_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        A_Eval_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("解析Json");
            var Weight = JsonConvert.DeserializeObject<List<Eval_WeightObject>>(Json)[0];
            Eval_Weight.Add(Weight);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }

        //Debug.Log("进行第一次share moment");
        //shareMomentControl.ShareMomentStart();
        if (timeIndex < 2)
        {
            timeIndex++;
            A_Eval_t(timeIndex);
        }
        else
        {
            Debug.Log("Eval评估标准生成结束，进入下一步");
            timeIndex = 0;
            Debug.Log("开始对抽象行为类进行采样");
            StartSampleandConcreteBehavior();

        }
    }

    public void StartSampleandConcreteBehavior()
    {
        if (timeIndex < 3)
        {
            var cur_AbstractBehavior = SampleTopBehaviors(Eval_Weight[timeIndex], 1);
            Debug.Log("获得抽象行为：" + cur_AbstractBehavior[0].name);
            A_Plan_t(cur_AbstractBehavior, timeIndex);
            timeIndex++;
        }
        else
        {
            Debug.Log("同天内容已经生成结束。");
            for(int i = 0; i < 3; i++)
            {
                Scene_Recording newScene = new Scene_Recording
                {
                    timeIndex = i,
                    state_Object = Avatar_Status[i],
                    a_ConcreteBehavior = concreteBehaviors[i],
                    World_Plan = World_Object,
                };
                settings.Share_Scenes_List.Add(newScene);
            }
            Debug.Log("已全部加入Settings中");
        }
        
    }

    public List<AbstractBehavior> SampleTopBehaviors(Eval_WeightObject preference, int topN = 3)
    {
        return Abstract_behaviors
            .OrderByDescending(b =>
                preference.学习 * b.behavior_traits.学习 +
                preference.放松 * b.behavior_traits.放松 +
                preference.社交 * b.behavior_traits.社交 +
                preference.运动 * b.behavior_traits.运动 +
                preference.自我提升* b.behavior_traits.自我提升
            )
            .Take(topN)
            .ToList();
    }

    public void A_Plan_t(List<AbstractBehavior> preference,int timeIndex)
    {
        Debug.Log("开始细化事件生成");
        string prompt = $@"你将虚拟人物行为细化。
请根据给定的“抽象行为”“真实世界内容”与“当前角色状态”，生成多种可供选择的具体行为选项，每个选项应具有合理的内容、地点和当前偏好评分（用于后续采样）。
【抽象行为】：{ListString.ListToString(preference)}
【真实世界内容】：{World_Object}（请结合真实世界内容生成具体计划）
【角色状态】：
- 当前心情：{Avatar_Status[timeIndex].心情}（范围：-5至+5）
- 当前精力：{Avatar_Status[timeIndex].精力}（范围：0-100）
- 当前压力：{Avatar_Status[timeIndex].压力}(范围：0.0 到 1.0，代表心理压力水平)
- 欲望值：
  - 放松：{Avatar_Status[timeIndex].欲望.放松}
  - 学习：{Avatar_Status[timeIndex].欲望.学习}
  - 社交：{Avatar_Status[timeIndex].欲望.社交}
  - 运动：{Avatar_Status[timeIndex].欲望.运动}
  - 自我提升：{Avatar_Status[timeIndex].欲望.自我提升}

【输出要求】：
请生成 3～5 个 JSON 格式的候选行为选项，每个包含以下字段：
[
  {{
    ""title"": ""行为标题，例如：和朋友聚餐"",
    ""description"": ""一句话解释行为目的和感受"",
    ""location"": ""行为发生的场所"",
    ""weight"": 0.0～1.0，表示此刻角色对该行为的倾向""
  }}
]";
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        A_Plan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = A_Plan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, A_Plan_CallBack));
    }

    void A_Plan_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        A_Plan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        plan_Objects.Clear();
        plan_Objects = JsonConvert.DeserializeObject<List<A_ConcreteBehavior>>(Json);

        Debug.Log("获得行动内容，进入具体活动内容采样");
        var item = SampleConcreteBehavior(plan_Objects);
        concreteBehaviors.Add(item);
    }

    private A_ConcreteBehavior SampleConcreteBehavior(List<A_ConcreteBehavior> options) 
    {
        float totalchance = 0f;
        foreach(var item in options)
        {
            totalchance += item.weight;
        }
        float rand = UnityEngine.Random.Range(0, totalchance);
        float cur_chance = 0f;
        foreach(var item in options)
        {
            cur_chance += item.weight;
            if (rand <= cur_chance)
            {
                return item;
            }
        }
        return options[0];
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
        //Unexpect_Objects.Add(new Event_Object
        //{
        //    event_index = MomentIndex, 
        //    Event = text
        //});
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

    [System.Serializable]
    public class AbstractBehavior
    {
        public string id;
        public string name;
        public string[] tags;
        public string activation_hint;
        public string[] typical_locations;
        public A_S_Desire behavior_traits;
    }

    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }


    [Serializable]
    public class Eval_WeightObject
    {
        public float 放松;
        public float 学习;
        public float 社交;
        public float 运动;
        public float 自我提升;
        public string 解释;
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


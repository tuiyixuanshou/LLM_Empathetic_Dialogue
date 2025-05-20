using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MyUtilities;
using static SunoAPIDemo;
using static UnityEditor.Progress;
using static ShareMomentControl;
using static AvatarMainStoryDemoV2;


public class API_Chat : MonoBehaviour
{
    public SimpleFileReader reader;
    public API_CentralControl api_CentralControl;
    public Settings settings;
    public Memory_Control memory_control;

    public AvaterBubbleControl bubbleControl;
    public AvatarMainStoryDemoV2 AI_Driven;

    public ShareMomentControl shareMomentControl;

    [Header("chat模型设置")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    [Header("推理模型设置")]
    public ChatModel Mreason_Model;
    public LLMURL Mreason_url;
    public APIKey Mreason_api;

    public InputField inputField;
    public GameObject ButtonRoot;
    public Button button1; public Button button2;
    private string PrePrompt;

    private DateTime LastInputTime;
    private DateTime LastRespondTime;

    private Coroutine CheckDialogueThreshold = null;

    //用于检测事件的、本轮对话的记录
    List<Dictionary<string, string>> AChat_Dial = new();

    private Dictionary<string, string> SystemPrompt;

    private void Start()
    {
        ButtonRoot.SetActive(false);
        button1 = ButtonRoot.GetComponentsInChildren<Button>()[0];
        button2 = ButtonRoot.GetComponentsInChildren<Button>()[1];
        inputField.keyboardType = TouchScreenKeyboardType.Social;
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);

        SystemPrompt = new Dictionary<string, string> { { "role", "system" }, { "content", null } }; //SystemPrompt
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
        //Mchat_API_Send("用户当前情绪较为平稳但存在未表达的情感，睡眠问题可能暗示潜在压力");
    }

    void Add_SystemPrompt(string prompt)
    {
        SystemPrompt = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",prompt }
        };
    }

    public void UserInputSend()
    {
        //非心理状态评估的对话问答
        if (!api_CentralControl.isEvaluateStart)
        {
            api_CentralControl.isDialogueStart = true; //开始正常对话的状态
            api_CentralControl.isSystemAwake = true;

            settings.LastInputTime = DateTime.Now; //更新用户最后输入时间
            string text = inputField.text;
            Mchat_API_FreePrompt(text, false, Mchat_Model, Mchat_url, Mchat_api); //进行反馈  
            inputField.text = string.Empty;
            bubbleControl.UserSendInput(); //关闭对方气泡
        }
        else
        {

        }
    }
    #region 心理评估的对话API 
    public void Mchat_API_Send(string evaluateresult)
    {
        StartCoroutine(Mchat_API_Send_Cor(evaluateresult));
    }

    private string relatedMemory;
    void SetRelatedMemory(string text)
    {
        relatedMemory = text;
    }

    public IEnumerator  Mchat_API_Send_Cor(string evaluateresult)
    {
        //TO DO：这里要不要加上策略 和从前对话内容 这里需要修改prompt  //4.15从前对话不用设计，有上下文设置和RAG
        yield return api_CentralControl.rag.postQuery(evaluateresult, SetRelatedMemory);
        string prompt = $@"你是一个治愈陪伴共情的小精灵，这是用户现在的心理状况评估{evaluateresult}。
这是从前用户和agent对话中，可能涉及这个心理状况的相关记忆：{relatedMemory}。
专家认为可以通过继续对话的方式已达到安慰的效果。请你继续主动发起和用户的聊天，就像平时闲谈一样，字数不要过多";

        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                {"role","system"},
                {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",prompt}
                };

        settings.tempDialogue.Add(usermessage);
        var payload = new
        {
            model = settings.m_SetModel(Mchat_Model),
            messages = settings.tempDialogue,
            stream = false,
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, Mchat_API_CallBack));
    }
    #endregion

    /// <summary>
    /// 被动回复对话
    /// </summary>
    /// <param name="m_prompt"></param>
    /// <param name="isUserText"></param>
    /// <param name="model"></param>
    /// <param name="url"></param>
    /// <param name="api"></param>
    public void Mchat_API_FreePrompt(string m_prompt,bool isProactive,ChatModel model,LLMURL url, APIKey api)
    {
        if (!PostWeb.isAIRun)
        {
            PostWeb.isAIRun = true;
            string Jsonpayload = string.Empty;
            List<Dictionary<string, string>> cur_message = new();
            cur_message.Add(SystemPrompt);
            foreach (var item in memory_control.shortMemory)
            {
                cur_message.Add(item);
            }
            //Agent所在场景输入
            string prompt = $@"
根据上述信息背景信息来生成你的回复。请注意，你们之间的互动更加类似线上互动，包含分享、建议、倾诉等。回复字数控制在60字以内.不要包含太多表情动作。";

            var systemmessage = new Dictionary<string, string>
            {
                {"role","system"},
                {"content",prompt}
            };
            cur_message.Add(systemmessage);

            if (!isProactive)
            {
                //被动回复，prompt是用户输入的话
                var usermessage = new Dictionary<string, string>
                {      
                    {"role","user"},
                    {"content",m_prompt}
                };
                AChat_Dial.Add(usermessage);  //只有本轮对话内容，用于事件检测
                cur_message.Add(usermessage); //近期的20条对话+SystemPrompt，用于维护上下文
                memory_control.AddToShortMemory(usermessage); //近期的20条对话，用户存储短期记忆
            }
            else
            {
                //主动发起，还是system提示消息
                string SYS_prompt = "请根据最终你的活动决策、所在场景，并参考其他提供的信息，生成主动给用户发起的聊天内容。";
                var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",SYS_prompt}
                };
                cur_message.Add(usermessage);
            }
            

            var payload = new
            {
                model = settings.m_SetModel(model),
                messages = cur_message,
                stream = false,
            };
            Jsonpayload = JsonConvert.SerializeObject(payload);
            StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(url), settings.m_SetApi(api), Jsonpayload, PassiveDialogue_CallBack));
        }
        else
        {
            bubbleControl.SetUpAvatarBubble("等等哦，我还在思考上一个问题~");
        }
    }

    IEnumerator CheckThreshold(Action callback)
    {
        TimeSpan userdifference = TimeSpan.Zero;
        TimeSpan responddifference = TimeSpan.Zero;
        while (true)
        {
            DateTime now = DateTime.Now;
            userdifference = now - settings.LastInputTime;
            responddifference = now - settings.LastRespondTime;
            if(userdifference.TotalSeconds>=100 && responddifference.TotalSeconds >= 100)
            {
                Debug.Log("Dialouge is over,and Respond Gap =" + responddifference.TotalSeconds);
                break;
            }
            Debug.Log("Dialouge is not over，and Respond Gap = " + responddifference.TotalSeconds);
            yield return new WaitForSeconds(41f);
        }
        callback();
    }

    private void FreshCorountine()
    {
        Debug.Log("检测到对话结束");
        StopCoroutine(CheckDialogueThreshold);
        CheckDialogueThreshold = null;
        api_CentralControl.isDialogueStart = false; //正常对话结束
        api_CentralControl.isSystemAwake = false;
        AEvent_detector(); //对对话进行检验
    }

    /// <summary>
    /// 被动对话 回调函数
    /// </summary>
    /// <param name="respond"></param>
    public void PassiveDialogue_CallBack(string respond)
    {
        StartCoroutine(PassiveDialogue_CallBack_Cor(respond));
    }

    IEnumerator PassiveDialogue_CallBack_Cor(string respond)
    {
        yield return api_CentralControl.api_Demo_Emoji.Emoji_Rag_Query(respond, bubbleControl.SetEmoji);
        
        settings.LastRespondTime = DateTime.Now; //更新系统最后回复时间

        //得到回复，开始判断 是否要结束对话
        if (CheckDialogueThreshold == null)
        {
            CheckDialogueThreshold = StartCoroutine(CheckThreshold(FreshCorountine));
        }

        var responseMessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content", respond }
        };
        AChat_Dial.Add(responseMessage);
        memory_control.AddToShortMemory(responseMessage);
        

        //展示有表情包的回复，但是需要rag一下表情包
        bubbleControl.SetUpAvatarBubble(respond);

        Debug.Log("This is Passive chat CallBack");
    }

    public void AEvent_detector()
    {
        string result = ListString.ListToString(AChat_Dial, ", ", dictionary =>
        {
            var keyValuePairs = new List<string>();
            foreach (var kvp in dictionary)
            {
                keyValuePairs.Add($"{kvp.Key}={kvp.Value}");
            }
            return string.Join(", ", keyValuePairs); // 将键值对用 ", " 连接
        });
        Debug.Log("开始进行事件探索");
        string prompt = $@"您将收到一段对话,需总结用户(user)在对话中是否出现身体不适状况或重大挫折导致的情绪问题。
只总结user内容，从对话中逐步思考,推理答案，最后只输出总结。
- 问题：若是用户（user）出现上述问题，返回bool值true,若无问题，返回false
- 内容：若是出现上述问题，则用一句话概述用户问题。若无问题，返回“无”
请用以下 JSON 结构输出：
[
{{
  ""问题"": ,
  ""内容"": ,
}}
]
请不要返回除Json数据以外的任何内容
收到的消息对话：{result}";

        List<Dictionary<string, string>> curList = new();
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        curList.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(Mreason_Model),
            messages = curList,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(Mreason_url), settings.m_SetApi(Mreason_api), Jsonpayload, AEvent_detector_CallBack));
    }

    void AEvent_detector_CallBack(string text)
    {
        AChat_Dial.Clear();
        string Json = PostWeb.JsonPatch(text);
        var user_info = JsonConvert.DeserializeObject<List<user_Info>>(Json)[0];
        if (user_info.problem)
        {
            shareMomentControl.AddUser_Objects(user_info.content);
        }        
    }
    [Serializable]
    public class user_Info
    {
        public bool problem;
        public string content;
    }
    /// <summary>
    /// 多模态接口 回调函数
    /// </summary>
    /// <param name="respond"></param>
    public void Mchat_API_CallBack(string respond)
    {
        StartCoroutine(StartMulti(respond));
    }
    IEnumerator StartMulti(string respond)
    {
        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);

        bubbleControl.SetUpAvatarBubble(respond);
        api_CentralControl.isMultiRespondStart = false; //多模态回复内容已完成

        settings.LastRespondTime = DateTime.Now; //更新系统最后回复时间

        //这里暂时让系统发出的问题和主动提出的问题一样，都作为对话开始进行记录。
        api_CentralControl.isDialogueStart = true;
        //得到多模态的回答，开始判断 是否要结束本次多模态的对话
        if (CheckDialogueThreshold == null)
        {
            CheckDialogueThreshold = StartCoroutine(CheckThreshold(FreshCorountine));
        }

        //将获得的回复也加入到tempDialogue中：
        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                 {"role","system"},
                 {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
        {
             {"role","assistant"},
             {"content",respond}
        };
        settings.tempDialogue.Add(usermessage);
        Debug.Log("This is Multi Chat CallBack");
    }


    #region 主动对话的对话API 
    public void Avatar_ProActive_Chat()
    {
        //不用有RAG,应用V2中的内容
        //此外，本周的事件为：{ AI_Driven.plan_Objects[(settings.month_Index - 1) * 4 + settings.week_Index - 1].Event}
        string prompt = $@"你现在做为治愈小精灵，用模拟大学生生活的方式主动进行用户陪伴。
模仿的过程中，需要以大学生的多维度目标来作为核心的驱动力，其中，目标维度分为：
自我规划类，我已经知道自己真正想要追求什么
志业追求类，我立志要在自己的专业或职业领域中做出突出成就
超我贡献类，我会从改善社会和人类福祉的角度考虑人生规划
而这三类又被细分为四个导向：学业成就导向、职业准备导向、个人成长导向、社交关系导向
此次陪伴以三个月，即十二个星期为期限。
在这段事件中，你的核心目标选择为：{ListString.ListToString(AI_Driven.Avatar_Status)}
每个月，不同的核心目标都会有不同的权重分布，总和为1。

现在进入了第{settings.month_Index}个月的第{settings.week_Index}个星期，


请你模仿这位大学生的身份，面对已有的计划和事件，做出响应。其中，在计划中选择本周你发生的内容，你可以选择1-2个进行模拟生成本周活动；事件是本周外部环境发生的事件，你进行的模拟本周活动可以选择被事件影响，或者不被事件影响。
综合考虑上述因素后后，请你通过向用户分享本周活动内容的方式来发起主动对话。
你可以选择两种模式进行对话，第一种是分享，即分享本周的活动内容。第二种是征求意见，即你可以根据本周的模拟活动是否有”需要决策“的部分，向用户发起意见征求，并生成两个相反的选项供用户选择。
请注意，在选择”分享“还是”征求意见“上，可以灵活处理。如果本周内容不涉及”需要决策“的部分，完全可以直接分享。如果有涉及”需要决策“的部分，可以向用户寻求意见，也可以自主决定后直接通过”分享“的方式告诉用户。请灵活处理。
最后，主动对话要保证对话方式自然，符合日常聊天分享的方式，字数不要太多，50字左右即可
返回方式请用Json数据的格式：
如选择”分享“，返回内容为：[{{""respond_type"":""share"",""respond"":""<分享的内容>"",""user_choice"":[{{""choice1"":"""",""choic2"":""""}}]}}]
如选择”征求意见“，返回内容为：[{{""respond_type"":""quest"",""respond"":""<分享的内容>"",""user_choice"":[{{""choice1"":""<选择1的内容>"",""choic2"":""<选择2的内容>""}}]}}]
";

        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                {"role","system"},
                {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",prompt}
                };

        settings.tempDialogue.Add(usermessage);
        var payload = new
        {
            model = settings.m_SetModel(Mchat_Model),
            messages = settings.tempDialogue,
            stream = false,
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, Avatar_ProActive_Chat_callback));
    }

    void Avatar_ProActive_Chat_callback(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        settings.tempDialogue.Add(newmmessage);
        string Json = PostWeb.JsonPatch(text);
        try
        {
            var chat_Respond = JsonConvert.DeserializeObject<List<Avatar_Proactive_Respond>>(Json)[0];
            if(chat_Respond.respond_type == "share")
            {
                StartCoroutine(StartMulti(chat_Respond.respond));
            }
            else if(chat_Respond.respond_type == "quest")
            {
                StartCoroutine(QuestRespond(chat_Respond));
            }
            else
            {
                Debug.Log("回复内容错误");
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    IEnumerator QuestRespond(Avatar_Proactive_Respond chat_Respond)
    {
        yield return StartMulti(chat_Respond.respond);
        yield return new WaitUntil(() => !bubbleControl.m_WriteState);
        Avatar_Quest_Button(chat_Respond);
    }

    void Avatar_Quest_Button(Avatar_Proactive_Respond chat_Respond )
    {
        button1.GetComponentInChildren<Text>().text = chat_Respond.user_choice[0].choice1;
        button2.GetComponentInChildren<Text>().text = chat_Respond.user_choice[0].choice2;
        button1.onClick.AddListener(delegate { ButtonInputSend(chat_Respond.user_choice[0].choice1); });
        button2.onClick.AddListener(delegate { ButtonInputSend(chat_Respond.user_choice[0].choice2); });
        ButtonRoot.SetActive(true);
    }
    //按钮输入，对应上文用户打字输入
    void ButtonInputSend(string ButtonText,Action callback = null)
    {
        if (!api_CentralControl.isEvaluateStart)
        {
            api_CentralControl.isDialogueStart = true; //开始正常对话的状态
            api_CentralControl.isSystemAwake = true;

            settings.LastInputTime = DateTime.Now; //更新用户最后输入时间
            string text = ButtonText;
            Mchat_API_FreePrompt(text, false, Mchat_Model, Mchat_url, Mchat_api); //进行反馈  
            bubbleControl.UserSendInput(); //关闭对方气泡

            //更加健壮的callback
            callback?.Invoke();
        }
    }


    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }

    [Serializable]
    public class Avatar_Proactive_Respond
    {
        public string respond_type;
        public string respond;
        public List<User_Choice> user_choice;
    }
    [Serializable]
    public class User_Choice
    {
        public string choice1;
        public string choice2;
    }
    //Callback先处理Json，最后调用multi的那个callback接口。
    //还要处理按钮
    //心理评估和同一场景中的后续内容。
    #endregion
}

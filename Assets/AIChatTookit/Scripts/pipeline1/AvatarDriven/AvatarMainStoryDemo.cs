using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static AvatarMainStoryDemo;
using System.Reflection;
using Unity.VisualScripting;

public class AvatarMainStoryDemo : MonoBehaviour
{
    public Settings settings;
    public KLingAPIImage API_Image;
    public KLingAPIDemo API_Video;
    public API_Chat api_chat;
    public quadVideo quadvideo;
    public AvaterBubbleControl bubbleControl;
    [Header("生成主线故事内容的模型")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;

    [Header("生成主线故事内容的模型")]
    public ChatModel MainLineChat_Model;
    public LLMURL MainLineChat_url;
    public APIKey MainLineChat_api;

    public string AvatarBackGround = "大学生";  //可以考虑是否移交给Settings，，不过说实在的setting其实也不顶用，最后还是需要本地存储。
    public string MainTarget="去看一场演唱会";
    [TextArea(5,5)]
    public string EventPool;
    public List<MonthTargetRespond> monthTargetResponds = new();
    public List<WeekTargetRespond> weekTargetResponds = new();
    public List<WeekEvent> weekEvents = new();
    public List<WeekEvent> choseWeekEvents = new();

    [TextArea(5, 5)]
    public string newImagePrompt;

    public string CurDialogue;
    public string ImageURL;

    public int month = 1;
    public int week = 0;

    List<Dictionary<string, string>> tempDialogue = new();
    List<Dictionary<string, string>> tempChatDialogue = new();

    private void Start()
    {
        //StartGenerateImage();
        //string Json = "[\r\n{\"month_index\":1,\"month_target\":\"适应新学期课程节奏，规划课余时间；了解演唱会具体时间与场地信息，制定初步购票预算；参加校内社团活动，拓展社交圈；保持日常跑步锻炼，为长途出行储备体力\"},\r\n{\"month_index\":2,\"month_target\":\"完成期中考试复习；与同学组队预订往返交通票；通过兼职/节省生活开支积累演唱会基金；参加校园音乐社的歌唱练习活动；调研演唱会周边美食攻略\"},\r\n{\"month_index\":3,\"month_target\":\"协调期末作业与出行时间；购置便携式充电宝等旅行用品；参与寝室夜谈会制定应援方案；维持规律作息调整生物钟；关注当地天气准备合适服饰\"}\r\n]";
        //string m_Json = JsonPatch(Json);
        //monthTargetResponds = JsonConvert.DeserializeObject<List<MonthTargetRespond>>(m_Json);
        //Debug.Log(monthTargetResponds);
        //monthTargetResponds = JsonConvert.DeserializeObject<List<MonthTargetRespond>>("[\r\n{\"month_index\":1,\"month_target\":\"合理安排课余时间熟悉演唱会相关信息，逐步调整新学期课程节奏；用两周时间研究演唱会场地交通及周边设施情况，利用周末参与同学社交活动培养出行默契；保持每周三次的校园慢跑适应体力需求\"},\r\n{\"month_index\":2,\"month_target\":\"通过家教兼职建立观演资金储备账户，每天节省15元餐饮开支作为应急基金；加入校园音乐社团扩展歌单鉴赏能力，参加两次城市探索活动熟悉车站枢纽路线；月末完成校园体测保持体能基准\"},\r\n{\"month_index\":3,\"month_target\":\"确认最终交通住宿方案并与同好协商分工，购置必要应援装备不超过预算30%；优化课程作业排期确保出行周无考核冲突，维持夜跑习惯提升持续站立耐力；参与三次歌迷线下聚会演练现场互动流程\"}\r\n]");
        //weekTargetResponds = JsonConvert.DeserializeObject<List<WeekTargetRespond>>("[\r\n{\"month_Index\":1,\"week_Index\":1,\"week_target\":\"熟悉演唱会主题曲目与歌手信息，建立课程时间表框架\",\"eventType\":\"1-上课,12-写作业,7-筛选演唱会歌单\"},\r\n{\"month_Index\":1,\"week_Index\":2,\"week_target\":\"利用地图软件研究路线，参加同学聚餐协商结伴\",\"eventType\":\"9-短途探访交通站点,8-班级联络会,4-听音乐放松\"},\r\n{\"month_Index\":1,\"week_Index\":3,\"week_target\":\"维持校园体力适应计划，完成专业课预习\",\"eventType\":\"5-校园夜跑打卡,2-图书馆自习,6-制作追星手账\"},\r\n{\"month_Index\":1,\"week_Index\":4,\"week_target\":\"参与辩论社破冰活动，整理周边餐饮攻略\",\"eventType\":\"8-社团招新茶话会,7-探店直播观看,5-羽毛球练习\"},\r\n\r\n{\"month_Index\":2,\"week_Index\":1,\"week_target\":\"启动家教兼职并记账，制定节俭饮食方案\",\"eventType\":\"3-中学数学辅导,4-自制便当计划,5-晨间瑜伽\"},\r\n{\"month_Index\":2,\"week_Index\":2,\"week_target\":\"参与音乐鉴赏工作坊，测试换乘路线可行性\",\"eventType\":\"8-社团音乐沙龙,9-地铁线路彩排,12-论文框架搭建\"},\r\n{\"month_Index\":2,\"week_Index\":3,\"week_target\":\"整理应急资金收支表，完成期中体测准备\",\"eventType\":\"3-线上英语陪练,5-800米强化训练,6-拼装模型减压\"},\r\n{\"month_Index\":2,\"week_Index\":4,\"week_target\":\"复盘交通预案漏洞，参与同城歌迷交流\",\"eventType\":\"8-歌迷QQ群讨论,1-公共选修课,7-电影院观影\"},\r\n\r\n{\"month_Index\":3,\"week_Index\":1,\"week_target\":\"预订高铁票并比对住宿，采购荧光棒等道具\",\"eventType\":\"7-网络比价购物,9-民宿实地考察,12-课程汇报赶工\"},\r\n{\"month_Index\":3,\"week_Index\":2,\"week_target\":\"协商现场应援分工，保持体能峰值状态\",\"eventType\":\"8-应援团职责分配会,5-阶梯耐力训练,4-追剧补充睡眠\"},\r\n{\"month_Index\":3,\"week_Index\":3,\"week_target\":\"完成期末作业前置，模拟演唱会站立测试\",\"eventType\":\"12-实验报告撰写,5-单曲循环跟唱练习,6-手幅DIY制作\"},\r\n{\"month_Index\":3,\"week_Index\":4,\"week_target\":\"最终行程核对，参加强化版应援口号演练\",\"eventType\":\"8-线下应援集训营,3-临时促销兼职,4-冥想情绪调节\"}\r\n]");
    }
    public void MonthTarget()
    {
        Debug.Log("开始每月目标生成");
        string prompt = $@"你现在需要模仿一个{AvatarBackGround}，三个月后你将{MainTarget}，该目标会在第三个月的末尾发生。
请将总目标拆解成三个月内，每个月需要完成的小目标。请根据时间的维度，合理分配每个月的活动内容。
注意：生成的每月活动目标不要过于目的导向，要符合人类的日常生活规划，除了实现目标之外需要考虑符合人物背景设定的日常生活中的活动。
请以Json格式返回内容，格式参考如下：[
{{""month_index"":1,""month_target"":""第一个月目标""}},
{{""month_index"":2,""month_target"":""第二个月目标""}},
{{""month_index"":3,""month_target"":""第三个月目标""}},
]
请不要返回除Json数据以外的任何内容";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = tempDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, MonthTarget_CallBack));
    }

    void MonthTarget_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        tempDialogue.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            monthTargetResponds = JsonConvert.DeserializeObject<List<MonthTargetRespond>>(Json);
            Debug.Log("monthTarget 生成成功");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }
    }

    public void WeekTargetAndBenchMark()
    {
        Debug.Log("开始每周目标拆解");
        string prompt = $@"请根据上述生成的每个月目标，将月目标继续拆解成每周的活动目标和标准事件类型。假设每个月有四周。
这个标准事件类型是指能够符合本周活动目标的事件类型，需要在事件池中进行选择。事件池：{EventPool}。
请以Json格式返回内容，例如：[
{{""month_Index"":1,""week_Index"":1,""week_target"":""周目标"",""eventType"":""标准事件类型,展示格式为标号+具体事件""}}，
{{""month_Index"":1,""week_Index"":2,""week_target"":""周目标"",""eventType"":""标准事件类型,展示格式为标号+具体事件""}}，
{{""month_Index"":1,""week_Index"":3,""week_target"":""周目标"",""eventType"":""标准事件类型,展示格式为标号+具体事件""}}，
{{""month_Index"":1,""week_Index"":4,""week_target"":""周目标"",""eventType"":""标准事件类型,展示格式为标号+具体事件""}}，
]请不要返回除Json数据以外的任何内容";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = tempDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, WeekTargetAndBenchMark_CallBack));
    }

    void WeekTargetAndBenchMark_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        tempDialogue.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            //weekTargetResponds = JsonUtility.FromJson<List<WeekTargetRespond>>(Json);
            weekTargetResponds = JsonConvert.DeserializeObject<List<WeekTargetRespond>>(Json);
            Debug.Log("weekTarget 生成成功");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }
    }

    public void WeekEventGenerate()
    {
        Debug.Log("每周事件生成");
        UpdateMonthAndWeek();
        string LastWeekEvent = CheckLastWeek();
        string prompt = $@"现在进入了第{month}个月第{week}周，按照先前生成的主线内容，你的总目标是{MainTarget}，月目标是{monthTargetResponds[month-1].month_target},
本周目标是：{weekTargetResponds[week - 1].week_target},本周标准事件是：{weekTargetResponds[week-1].eventType}.
上周选择的活动内容：{LastWeekEvent}。
但是为了模仿人的真实状态，人随时都能够改变已有的主线规划，或遇到日常生活中的突发事件。
为了还原这种不确定型，可以在事件池中随机选择3-4种事件类型，并细化具体事件内容。
细化后，先判断此事件和标准事件的相关性，如果和标准事件非常相关，也与本周、本月的目标强相关，那么这件事发生的概率就高；
如果该事件完全违背了标准事件、违背了本周、本月目标，那么这件事发生概率就低。
请生成事件和本周目标以及标准事件类型的相关性，完全相关为1，完全不相干为0
请再根据相关性，生成该事件可能发生的概率，保证这些概率的总和是100%，高相关性的内容概率可以和低相关性内容概率产生的差值较大。
请以Json格式返回内容，例如：[
{{""event_Index"":1,""event_Type"":""事件类型"",""eventDetails"":""细化事件内容"",""correlation"":0.5，""occurance"":0.5}}，
{{""event_Index"":2,""event_Type"":""事件类型"",""eventDetails"":""细化事件内容"",""correlation"":0.5，""occurance"":0.5}}，
]请不要返回除Json数据以外的任何内容";
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = tempDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, WeekEventGenerate_CallBack));

    }

    void WeekEventGenerate_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        tempDialogue.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            //weekTargetResponds = JsonUtility.FromJson<List<WeekTargetRespond>>(Json);
            //weekEvents = JsonUtility.FromJson<List<WeekEvent>>(Json);
            weekEvents = JsonConvert.DeserializeObject<List<WeekEvent>>(Json);
            Debug.Log("weekEvents 生成成功");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }
    }
    string CheckLastWeek()
    {
        if(choseWeekEvents.Count == 0)
        {
            return "暂无";
        }
        else
        {
            string text = $"{"事件类型：" + choseWeekEvents[choseWeekEvents.Count - 1].event_Type + " 事件细节：" + choseWeekEvents[choseWeekEvents.Count - 1].eventDetails + " 事件和主线相关度：" + choseWeekEvents[choseWeekEvents.Count - 1].correlation + " 事件发生概率：" + choseWeekEvents[choseWeekEvents.Count - 1].occurance}";
            return text;
        }
    }

    void UpdateMonthAndWeek()
    {
        if(month > 3)
        {
            Debug.LogError("停止！！！！");
        }
        if (week<4)
        {
            week++;
        }
        else
        {
            month++;
            week = 1;
        }
    }

    public void ChooseWeekEvent(int index)
    {
        choseWeekEvents.Add(weekEvents[index]);
    }

    public void SceneImagePromptGenerate()
    {
        Debug.Log("图片prompt生成");
        string prompt = $@"你现在是一个经验丰富的AIGCprompt的撰写者,你也有丰富的心理学经验。
现在，你需要根据已经选择的所选择的本周事件内容，生成一段用于生成相关图片的prompt。
本周所选择内容是：{"事件类型："+choseWeekEvents[choseWeekEvents.Count - 1].event_Type+" 事件细节："+choseWeekEvents[choseWeekEvents.Count-1].eventDetails+ " 事件和主线相关度："+choseWeekEvents[choseWeekEvents.Count - 1].correlation+" 事件发生概率："+ choseWeekEvents[choseWeekEvents.Count - 1].occurance};
你需要判断一个符合本周事件发生的场景，生成该场景的位置名称，并生成该场景中的细节内容，使生成的内容可以作为生成图片的prompt。
请注意，现在已经有了一个主人公的形象，需要将主人公放入背景中，你需要生成主人公在场景中的简单姿势描述和背景描述。不要对主人公的外貌、具体样子有形容，统一使用“主人公”代替。
仅返回简单姿势描述和背景描述，不要返回其他任何内容。";
        Debug.Log(prompt);
        List<Dictionary<string, string>> temp = new();
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        temp.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = temp,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, SceneImagePrompt_CallBack));
    }

    void SceneImagePrompt_CallBack(string text)
    {
        newImagePrompt = text;
    }

    public void StartGenerateImage()
    {
        Debug.Log("图片生成");
        StartCoroutine(GenerateImage());
    }

    public IEnumerator GenerateImage()
    {
        API_Image.prompt = newImagePrompt+"生成的场景需要可爱软萌温馨治愈。";
        Debug.Log(API_Image.prompt);
        yield return API_Image.GenerateImage(GenerateImage_CallBack);
    }

    void GenerateImage_CallBack(string text)
    {
        ImageURL = text;
    }

    public void ChatGenerate()
    {
        Debug.Log("对话内容生成");
        string prompt = $@"你现在是一个陪伴治愈小精灵，你现在需要模仿一个{AvatarBackGround}，三个月后你将{MainTarget}，
现在进入了第{month}个月第{week}周，按照先前生成的主线内容，你的总目标是{MainTarget}，月目标是{monthTargetResponds[month - 1].month_target},
本周目标是：{weekTargetResponds[(month-1)*4+ week - 1].week_target},本周所选择内容是：{"事件类型：" + choseWeekEvents[choseWeekEvents.Count - 1].event_Type + " 事件细节：" + choseWeekEvents[choseWeekEvents.Count - 1].eventDetails + " 事件和主线相关度：" + choseWeekEvents[choseWeekEvents.Count - 1].correlation + " 事件发生概率：" + choseWeekEvents[choseWeekEvents.Count - 1].occurance}，
现在的场景和你的姿势是{newImagePrompt}。
你需要根据本周的情况，结合相关情况，主动发起的和用户的对话，对话内容简单可爱，不超过15个字，符合日常对话内容。";
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempChatDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLineChat_Model),
            messages = tempChatDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLineChat_url), settings.m_SetApi(MainLineChat_api), Jsonpayload, ChatGenerate_CallBack));
    }

    void ChatGenerate_CallBack(string text)
    {
        CurDialogue = text;
    }


    public void StartGenerateVideo()
    {
        StartCoroutine(GenerateVideo());
    }

    IEnumerator GenerateVideo()
    {
        Debug.Log("视频生成"); 
        string prompt = $@"你现在是一个经验丰富的AIGCprompt的撰写者,你也有丰富的心理学经验。
现在，你需要根据所选择的本周事件内容，和已经生成的场景图片，生成一段相关视频生成的prompt。
本周所选择内容是：{"事件类型：" + choseWeekEvents[choseWeekEvents.Count - 1].event_Type + " 事件细节：" + choseWeekEvents[choseWeekEvents.Count - 1].eventDetails + " 事件和主线相关度：" + choseWeekEvents[choseWeekEvents.Count - 1].correlation + " 事件发生概率：" + choseWeekEvents[choseWeekEvents.Count - 1].occurance}，已经生成的背景场景是{newImagePrompt}。
你需要生成主人公在此场景下的简单、合理的动作。
请注意，此动作需要符合当下场景和主人公目前的姿势，动作合理简单。
仅返回简单的动作描述，不要返回其他任何内容。";
        List<Dictionary<string, string>> temp = new();
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        temp.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = temp,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        yield return postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, VideoPrompt_CallBack);
        yield return API_Video.GenerateVideo(API_Video.prompt, tempShowVideo);
    }

    void VideoPrompt_CallBack(string text)
    {
        API_Video.prompt = $@"请在结合图片主体原有姿势以及背景的前提下，自动生成一段合理的{text},动作和表情需要可爱软萌";
        API_Video.ImageBase64 = ImageURL;
    }

    //视频播放，没有字典定义，先这样。
    void tempShowVideo(string url)
    {
        quadvideo.RespondToM_Action(url);
        api_chat.PassiveDialogue_CallBack(CurDialogue);
    }

    IEnumerator postRequest(string url, string api, string json, Action<string> callback)
    {
        using(var uwr = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Authorization", "Bearer " + api);

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error While Sending: " + uwr.error);
                Debug.Log("Full respond:" + uwr.downloadHandler.text);
            }
            else
            {
                Debug.Log("Received: " + uwr.downloadHandler.text);
                string response = uwr.downloadHandler.text;
                ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
                string responseJson = apiResponse.choices[0].message.content;
                Debug.Log(responseJson);
                callback(responseJson);
            }
        }
        
    }

    string JsonPatch(string rawText)
    {
        string pattern = @"\[.*?\]";
        Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string extractedJson = match.Value;
            Debug.Log("提取的 JSON 内容：" + extractedJson);
            return extractedJson;
        }
        else
        {
            Debug.Log("没有找到中括号内的内容，Json数据返回完全失败！");
            return null;
        }
    }

    #region 解析目标
    [Serializable]
    public class MonthTargetRespond
    {
        public int month_index;
        public string month_target;
    }

    [Serializable]
    public class WeekTargetRespond
    {
        public int month_Index;
        public int week_Index;
        public string week_target;
        public string eventType;
    }

    [Serializable]
    public class WeekEvent
    {
        public int event_Index;
        public string event_Type;
        public string eventDetails;
        public float correlation;
        public float occurance;
    }
    #endregion
}

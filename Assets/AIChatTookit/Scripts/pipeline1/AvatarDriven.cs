using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static SunoAPIDemo;

public class AvatarDriven : MonoBehaviour
{
    public Settings settings;
    public API_CentralControl api_CentralControl;
    public quadVideo quadvideo;
    //开始进行avatar平行世界故事内容生成
    //①生成故事
    //②生成多模态内容
    //③内容记录反馈
    [Header("生成故事内容的模型")]
    public ChatModel Story_Model;
    public LLMURL Story_url;
    public APIKey Story_api;

    string PrePrompt;
    public List<string> preStory;

    //private List<ApiResponse> responses = new();
    private APIRespond respond;

    private void Start()
    {
        PrePrompt = $@"你现在正在扮演一个疗愈小精灵，你的名字叫做{settings.AIName}，你的是性格是{settings.AICharacter}。";
        //StoryGeneration();
    }

    public void StoryGeneration()
    {
        //场景判断
        string PreContend = JsonConvert.SerializeObject(preStory);
        string prompt = $@"你现在所在的场景是：{settings.Scene_Discribtion}
在这个场景中，avatar会做出一些自主的活动。
在相同的场景中，avatar之前自主活动内容是：{PreContend}。请你生成avatar的新的活动，可以是有关：
1.经历分享（在此场景中做了什么、此场景中发生了什么）；2.寻求帮助（在场景中遇到了某些问题，需要需求用户意见等）。
请只选择其中一种方面的内容，生成小动物的活动内容,例如：点了一份他尝试过的咖啡，他尝了尝发现完全不好喝，有些泄气，并决定重新点一杯咖啡。
只返回avatar故事内容。";
        Debug.Log(prompt);

        List<Dictionary<string, string>> temp = new();
        var Systemmessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",PrePrompt }
        };
        temp.Add(Systemmessage);
        var newMessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content", prompt}
        };
        temp.Add(newMessage);

        var payload = new
        {
            model = settings.m_SetModel(Story_Model),
            messages = temp,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(Story_url), settings.m_SetApi(Story_api), Jsonpayload, StoryGenerateCallback));
    }

    public void StoryGenerateCallback(string text)
    {
        //this is callback, generate mulit respond prompt
        MultiPromptGeneration(text);
    }

    public void MultiPromptGeneration(string text)
    {
        string prompt = $@"你现在是一个经验丰富的AIGCprompt的撰写者，你也有丰富的心理学经验。现在，你需要根据提供的故事内容，生成多模态回复的prompt。
故事如下：{text}。
你需要生成文字、动作两方面的回复。文字回复的要求：符合这个故事内容背景下，AI小动物会说的话，需要符合日常对话的形式。
动作回复要求：思考符合这个故事内容下，AI Agent会做出的动作，动作需要合理、且幅度不大，生成可以使AIGC准确生成这段动作的prompt指令。
请以Json的形式回复，除了Json内容之外什么都不要回复，参考格式如下：[
{{""Chat"":""刚刚点的这杯咖啡真的好难喝啊~"",""Action"":""沮丧地摇了摇头，并摆了摆手，表情有些沮丧和无奈。""}}
]";
        Debug.Log(prompt);

        List<Dictionary<string, string>> temp = new();
        var Systemmessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",prompt }
        };
        temp.Add(Systemmessage);
        var payload = new
        {
            model = settings.m_SetModel(Story_Model),
            messages = temp,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(Story_url), settings.m_SetApi(Story_api), Jsonpayload, MultiPromptCallBack));
    }

    public void MultiPromptCallBack(string text)
    {
        Debug.Log("安全解析Json数据");
        List<APIRespond> apiResponds = ParseJsonSafely(text);
        Debug.Log("调用API接口");
        respond = apiResponds[0];

        StartCoroutine(ShowStoryMulit(respond));
        //这里的chat需要在动作视频生成结束之后调用
        //api_CentralControl.api_Chat.Mchat_API_CallBack(apiResponds[0].Chat);

    }

    IEnumerator ShowStoryMulit(APIRespond responed)
    {
        string prompt = $@"请在结合图片主体原有姿势以及背景的前提下，自动生成一段合理的{responed.Action},动作和表情需要可爱软萌";

        //这里的视频不在系统发起中 吗 对 不在系统发起中，是定时的
        yield return api_CentralControl.api_Action.GenerateVideo(prompt, WaitForVedioWaitForVedio);
    }

    public void WaitForVedioWaitForVedio(string url)
    {
        StartCoroutine(func(url,respond));
    }
    IEnumerator func(string url, APIRespond responed)
    {
        yield return new WaitUntil(() => !api_CentralControl.isSystemAwake);
        api_CentralControl.isSystemAwake = true;
        quadvideo.RespondToM_Action(url);
        api_CentralControl.api_Chat.Mchat_API_CallBack(responed.Chat);
    }
    public List<APIRespond> ParseJsonSafely(string text)
    {
        string newJson = JsonPatch(text);
        try
        {
            return JsonConvert.DeserializeObject<List<APIRespond>>(text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
            return new List<APIRespond>(); // 返回空列表，防止崩溃
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

    IEnumerator postRequest(string url, string api, string json, Action<string> callback)
    {
        var uwr = new UnityWebRequest(url, "POST");
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

    #region API指导实例
    [System.Serializable]
    public class APIRespond
    {
        public string Chat;
        public string Action;
    }
    #endregion
}

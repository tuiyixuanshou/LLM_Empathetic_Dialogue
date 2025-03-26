using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;


public class EventShare : MonoBehaviour
{
    public Settings settings;
    //生成一个事件分享的内容
    [Header("朋友圈分享模型设置")]
    //public string Silicon_url = "https://api.siliconflow.cn/v1/chat/completions";
    //private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;

    private string PrePrompt;
    private string SpecifyPrompt;

    [Header("Event UI")]
    public Text text;

    private void Start()
    {
        Debug.Log(settings.AIName);
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);
        PrePrompt = $@"你现在正在扮演一个疗愈小精灵，
                        你的名字叫做{settings.AIName}，
                        你的是性格是{settings.AICharacter},
                        这是你们之间对话中，发生的能够让用户感到快乐、悲伤或其他鲜明情绪的大事{DialogueEventjson}";
        SpecifyPrompt = $@"请问你有什么事情想和用户分享呢？可以和用户分享你最近经历的一件事，类似于告诉你的朋友
                          你最近的情况，字数不超过50。以下是两个例子：“今天外出爬山，发现山上居然养了一些猴子，他们真可爱，我用小零食喂了他们。”
                         “出门时下雨了，我没有带伞，跑进一家咖啡店，结果发现了非常美味的咖啡！”
                         ";
        //暂时作为流程的开头
        SendEvent_Silicon(SpecifyPrompt, PrePrompt);
    }

    private void SendEvent_Silicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;

        List<Dictionary<string, string>> tempData = new();
        var SystemMessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content", pre }
        };
        var UserMessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content", prompt }
        };
        tempData.Add(SystemMessage);
        tempData.Add(UserMessage);
        var payload = new
        {
            //model = "meta-llama/Llama-3.3-70B-Instruct",
            model = settings.m_SetModel(chatModel),
            messages = tempData,
            stream = false,
        };
        string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
        StartCoroutine(postEventRequest_Silicon(settings.m_SetUrl(url), jsonPayload));
    }

    IEnumerator postEventRequest_Silicon(string url, string json)
    {
        Debug.Log("now Start to Generate Silicon Daily Routine");
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + settings.m_SetApi(api));

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Silicon Daily Routine: " + uwr.downloadHandler.text);
            //retrieve response from the JSON
            string response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            text.text = responseJson;
        }
    }

}

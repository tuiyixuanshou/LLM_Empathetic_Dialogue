using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using static sendData;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LLMChat : MonoBehaviour
{
    //聊一聊事件分享的内容
    public Settings settings;
    public BubbleControl bubbleControl;
    [Header("User Input设置")]
    public InputField UserInputField;
    private string userInputText;
    [Header("聊天模型设置")]
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;
    //[Header("Silicon-LLM设置")]
    //public string Silicon_url = "https://api.siliconflow.cn/v1/chat/completions";
    //private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";

    //[Header("DeepSeek-LLM设置")]
    //public string DeepSeek_url = "https://aihub.cuc.edu.cn/console/v1/chat/completions";
    //private string DSapiKey = "sk-84b722fdcc9a41ea93917034c53457c7";



    private string PrePrompt;
    private string SpecifyPrompt;

    private string ShareEvent = "A few days ago in the town's park, I helped a lost kitten find its owner, and everyone smiled happily!";
    //public static List<Dictionary<string, string>> tempDialoguePos = new();
    private bool isAIRun;

    private void Start()
    {
        UserInputField.keyboardType = TouchScreenKeyboardType.Social;
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);
        PrePrompt = $@"你现在正在扮演一个疗愈小精灵，
                        你的名字叫做{settings.AIName}，
                        你的是性格是{settings.AICharacter},
                        这是你们之间对话中，发生的能够让用户感到快乐、悲伤或其他鲜明情绪的大事{DialogueEventjson}，
                        此外，你会给用户分享你可能经历的一些事件。
                        回复不要过长，符合日常聊天的模式。";

        SpecifyPrompt = $@"用户对于你分享给他的事件“{ShareEvent}”非常感兴趣，
                           请你和他聊聊这件事吧,闲谈其中的一小部分内容即可，不超过100字。";
        //StartChat(SpecifyPrompt, PrePrompt);
    }

    //用户输入回车键之后调用
    public void GetTextAndClear(string text)
    {
        Debug.Log("GetText");
        if (UserInputField.gameObject.activeSelf)
        {
            userInputText = UserInputField.text;
        }
        else
        {
            //语音输入得到的内容
            userInputText = text;
        }
        Debug.Log(userInputText);

        if (userInputText != "总结" && userInputText != null)
        {
            UserInputField.text = string.Empty;
            PlayerBubble(userInputText);
            RespondChat(userInputText, PrePrompt);
        }
        else if (userInputText == "总结")
        {
            //StartGenerilize();
        }

    }
    public void StartChat(string eventContent)
    {
        if (!isAIRun)
        {
            string prompt = $@"用户对于你分享给他的事件“{eventContent}”非常感兴趣，
                           请你和他聊聊这件事吧,闲谈其中的一小部分内容即可，符合日常聊天的模式,不超过100字。";;
            //添加system预设
            if (settings.tempDialogue.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", PrePrompt }
                };
                settings.tempDialogue.Add(preMessage);
                Debug.Log("start temp1:" + settings.tempDialogue.Count);
            }
            // 构造消息内容
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            settings.tempDialogue.Add(newmessage);
            Debug.Log("start temp2:" + settings.tempDialogue.Count);

            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = settings.tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            settings.tempDialogue.RemoveAt(settings.tempDialogue.Count - 1);
            StartCoroutine(postRequestSiliconChat(settings.m_SetUrl(url), jsonPayload, AvaterBubble));
        }
        else
        {
            Debug.Log("wait");
            //CreatBubble(CurAIName, "请稍等，我还在回复上一条信息~", false, AIHeadImage);
        }
    }

    private void RespondChat(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //添加system预设
            if (settings.tempDialogue.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", pre }
                };
                settings.tempDialogue.Add(preMessage);
            }
            // 构造消息内容
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            settings.tempDialogue.Add(newmessage);

            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = settings.tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            //StartCoroutine(postRequestSiliconChat(Silicon_url, jsonPayload, AvaterBubble));
            StartCoroutine(postRequestSiliconChat(settings.m_SetUrl(url), jsonPayload, AvaterBubble));
        }
        else
        {
            Debug.Log("wait respond");
        }
    }

    private void AvaterBubble(string text)
    {
        bubbleControl.SetBubble(true, text);
    }

    private void PlayerBubble(string text)
    {
        bubbleControl.SetBubble(false, text);
    }


    IEnumerator postRequestSiliconChat(string url, string json,Action<string> callback)
    {
        isAIRun = true;
        //HeadName.text = "对方正在输入……";
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        //uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);
        uwr.SetRequestHeader("Authorization", "Bearer " + settings.m_SetApi(api));

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            string response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            //将获得的回复也加入到tempDialogue中：
            var responseMessage = new Dictionary<string, string>
            {
                {"role","assistant" },
                {"content", responseJson }
            };
            settings.tempDialogue.Add(responseMessage);
            //CreatBubble(CurAIName, responseJson, false, AIHeadImage);

            //if (isSpeech)
            //{
            //    //speech.baiduTTS.Speak(responseJson, speech.PlayAudio);
            //    //speech.openAITTS.Speak(responseJson, speech.PlayAudio);
            //    speech.SpeakFunction(tts, responseJson);
            //}
            callback(responseJson);

        }
        isAIRun = false;
        //HeadName.text = CurAIName;
    }
}

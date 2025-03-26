using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using static SunoAPIDemo;

public class API_Chat : MonoBehaviour
{
    public API_CentralControl api_CentralControl;
    public Settings settings;
    public AvaterBubbleControl bubbleControl;
    [Header("chat模型设置")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    public InputField inputField;

    private bool isAIRun;  //一问一答
    private string PrePrompt;

    private DateTime LastInputTime;
    private DateTime LastRespondTime;

    private Coroutine CheckDialogueThreshold = null;
    


    private void Start()
    {
        inputField.keyboardType = TouchScreenKeyboardType.Social;
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);
        PrePrompt = $@"你现在正在扮演一个疗愈小精灵，
                        你的名字叫做{settings.AIName}，
                        你的是性格是{settings.AICharacter},
                        这是你们之间对话中，发生的能够让用户感到快乐、悲伤或其他鲜明情绪的大事{DialogueEventjson}，
                        回复不要过长，符合日常聊天的模式。";
        Mchat_API_Send("用户当前情绪较为平稳但存在未表达的情感，睡眠问题可能暗示潜在压力");
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
            Mchat_API_FreePrompt(text, true, Mchat_Model, Mchat_url, Mchat_api); //进行反馈
            inputField.text = string.Empty;
            bubbleControl.UserSendInput(); //关闭对方气泡
        }
        else
        {

        }
    }

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

        //TO DO：这里要不要加上策略 和从前对话内容 这里需要修改prompt
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
        StartCoroutine(postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, Mchat_API_CallBack));
    }

    public void Mchat_API_FreePrompt(string m_prompt,bool isUserText,ChatModel model,LLMURL url, APIKey api)
    {
        if (!isAIRun)
        {
            isAIRun = true;string Jsonpayload = string.Empty;
            //其他地方需要调用对话模型
            if (!isUserText)
            {
                List<Dictionary<string, string>> temp = new();
                var message = new Dictionary<string, string>
                {
                    {"role","system"},
                    {"content",m_prompt}
                };
                temp.Add(message);
                var payload = new
                {
                    model = settings.m_SetModel(model),
                    messages = temp,
                    stream = false,
                };
                Jsonpayload = JsonConvert.SerializeObject(payload);
            }
            else
            {
                if(settings.tempDialogue.Count  == 0)
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
                    {"content",m_prompt}
                };

                settings.tempDialogue.Add(usermessage);
                var payload = new
                {
                    model = settings.m_SetModel(model),
                    messages = settings.tempDialogue,
                    stream = false,
                };
                Jsonpayload = JsonConvert.SerializeObject(payload);
            }
            StartCoroutine(postRequest(settings.m_SetUrl(url), settings.m_SetApi(api), Jsonpayload, PassiveDialogue_CallBack));
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
            if(userdifference.TotalSeconds>=120 && responddifference.TotalSeconds >= 120)
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
    }

    /// <summary>
    /// 被动对话 回调函数
    /// </summary>
    /// <param name="respond"></param>
    void PassiveDialogue_CallBack(string respond)
    {
        settings.LastRespondTime = DateTime.Now; //更新系统最后回复时间

        //得到回复，开始判断 是否要结束对话
        if (CheckDialogueThreshold == null)
        {
            CheckDialogueThreshold = StartCoroutine(CheckThreshold(FreshCorountine));
        }

        //将获得的回复也加入到tempDialogue中：
        var responseMessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content", respond }
        };
        settings.tempDialogue.Add(responseMessage);
        bubbleControl.SetUpAvatarBubble(respond);

        Debug.Log("This is Passive chat CallBack");
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

    IEnumerator postRequest(string url, string api, string json, Action<string> callback)
    {
        isAIRun = true;
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
            FreshCorountine();
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
        isAIRun = false;
    }
}

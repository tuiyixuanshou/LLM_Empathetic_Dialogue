using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using WebGLSupport;
using System.Net.Security;

using System.Text.RegularExpressions;
using UnityEngine.Windows;

public class evaluateTest : MonoBehaviour
{
    public Settings settings;
    public API_CentralControl api_CentralControl;
    public AvaterBubbleControl avaterBubble;

    [Header("测量评估用户状态模型设置")]
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;

    public InputField inputField;

    private bool isAIRun;
    private List<Dictionary<string, string>> tempEvaluate = new();

    //用户是否回答了单个问题
    private bool isUserAnswer = false;

    //心理评估是否已经开始
    private bool isStartEvaluate = false;

    private Coroutine askEvaluateCoroutine; // 存储协程句柄

    private List<Dictionary<string, string>> TempevaluateContext = new();

    private void Start()
    {
        //TO DO:API Central Control进行控制
        //GenerateEvaluate();
    }

    public void GenerateEvaluate()
    {
        if (!isAIRun) //isAIRun判定是必要的，评估问题一次只生成一次。
        {
            isAIRun = true;
            Debug.Log("评估问题生成");
            string prompt = $@"请向用户询问3-4个问题，用于评估用户的情绪，返回的数据全部使用JSON数据的格式，如：
[
    {{""Question"": ""你现在好吗""}},
    {{""Question"": ""最近有没有什么事情让你感到特别开心或不开心？""}},
    {{""Question"": ""你最近有没有感到压力或焦虑？如果有，是什么事情让你感到压力？""}}
]
回复中，请不要生成任何除了JSON数据以外的东西，包括多余的数字、文字、符号等等。并保证回复格式正确，谢谢。";

            var newmessage = new Dictionary<string, string>
            {
                {"role","system" },
                {"content", prompt }
            };
            tempEvaluate.Add(newmessage);
            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = tempEvaluate,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            StartCoroutine(postRequest(settings.m_SetUrl(url), settings.m_SetApi(api), jsonPayload, CallBackEvaluate));
        }
        else
        {
            Debug.Log("Wait Evaluate");
        }
    }

    void CallBackEvaluate(string text)
    {
        //text解析
        List<QuestionData> questions = ParseJsonSafely(text);
        if(questions.Count != 0)
        {
            //StartCoroutine(askEvaluate(questions));
            askEvaluateCoroutine = StartCoroutine(askEvaluate(questions));
        }
    }
    public List<QuestionData> ParseJsonSafely(string text)
    {
        string newJson = JsonPatch(text);
        try
        {
            return JsonConvert.DeserializeObject<List<QuestionData>>(text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
            return new List<QuestionData>(); // 返回空列表，防止崩溃
        }
    }

    IEnumerator askEvaluate(List<QuestionData> questions)
    {
        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);
        isStartEvaluate = true;
        api_CentralControl.isEvaluateStart = true;

        Debug.Log(api_CentralControl.isEvaluateStart);
        TempevaluateContext.Clear();
        foreach (var i in questions)
        {
            var newitem = new Dictionary<string, string>
            {
                {"Question",i.Question },
                {"Answer", ""}
            };
            TempevaluateContext.Add(newitem);
            avaterBubble.SetUpAvatarBubble(i.Question);
            settings.LastRespondTime = DateTime.Now;
            //TO DO这里做一个简单的回答，不然显得过于干巴
            yield return new WaitUntil(() => isUserAnswer);
            isUserAnswer = false;
            yield return new WaitForSeconds(1f);

        }
        avaterBubble.UserSendInput();
        askEvaluateCoroutine = null;
        isStartEvaluate = false;
        api_CentralControl.isEvaluateStart = false;
        Debug.Log("进行评估");
        EvaluateAndFirstSelect();
    }

    //inputField end 调用
    public void UserInputFunc()
    {
        //此处没有涉及到isDialogue start
        //avaterBubble.UserSendInput();
        if (TempevaluateContext.Count != 0 && isStartEvaluate)
        {
            Debug.Log("用户回答数据存储，用于评估");
            var last = TempevaluateContext[TempevaluateContext.Count - 1];
            if (last.ContainsKey("Answer")) // 确保键存在
            {
                last["Answer"] = inputField.text;
            }
            else
            {
                Debug.Log("数据存储出现问题");
                //last.Add("Answer", inputField.text);
            }
            settings.LastInputTime = DateTime.Now; 
            inputField.text = string.Empty;
            isUserAnswer = true;
        }
        
    }

    void EvaluateAndFirstSelect()
    {
        string evalutateContext = JsonConvert.SerializeObject(TempevaluateContext);

        //这里prompt不一定是evaluate Context，我觉得tempDialogue也可以啊对吧  

        string prompt = $@"你是一个优秀的心理安慰师，以下是你提供给用户一些用于评估他/她心理状态的问题和用户的回答:{evalutateContext},
请你根据这段问答，评估出用户此时的心理状态，并选择一种安慰他的策略，策略分为四大类，分别是：
chat对话，继续用语言来安慰用户；action动作，做出一些动作来安慰用户；sound声音，通过声音来安慰用户，scene场景，通过场景变化来安慰用户。
返回的数据全部使用JSON数，并按照以下格式返回：
[
    {{""Evaluate"": ""用户现在的心情还不错，并愿意进行交流"",""MMChoose"":""chat""}}
],
错误格式例子：
```json
[
    {{""Evaluate"": ""用户当前情绪看似稳定但缺乏日常动力，可能存在潜在压力或孤独感"",""MMChoose"":""chat""}}
]
``` 
上述错误例子中，出现了除json数据外的其他内容，其中包括最开头的```json和结尾处的```，这些都是我绝对不要的。
请注意，Evaluate代表你根据问答对用户做出的心理评估，MMChoose代表你选择的安慰策略，安慰策略只返回chat、action、sound、scene四种中的一种。
回复中，请不要生成任何除了JSON数据以外的东西，包括多余的数字、文字、符号等等。并保证回复格式正确，谢谢。
";
        var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
        List<Dictionary<string, string>> tempEvaluate = new();
        tempEvaluate.Add(newmessage);
        var payload = new
        {
            model = settings.m_SetModel(chatModel),
            messages = tempEvaluate,
            stream = false,
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(url),settings.m_SetApi(api), Jsonpayload, api_CentralControl.EvaluateResultAccept));
    }

    private void Update()
    {
        //这里需要重新考虑 添加一段system提示词在tempdialogue中。
        if (isStartEvaluate)
        {
            if (!avaterBubble.AvatarBubble.activeInHierarchy)  //if看下能不能和isDialogueStart挂钩。
            {
                //结束携程
                if (askEvaluateCoroutine != null)
                {
                    StopCoroutine(askEvaluateCoroutine);
                    askEvaluateCoroutine = null;
                    isStartEvaluate = false;
                    Debug.Log("ask Evaluate 协程已停止");
                }
            }
        }
    }

    IEnumerator postRequest(string url, string api,string json, Action<string> callback)
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

    string JsonPatch(string rawText)
    {
        string pattern = @"\[.*?\]";
        Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string extractedJson = match.Value;
            Debug.Log("提取的 JSON 内容："+ extractedJson);
            return extractedJson;
        }
        else
        {
            //Console.WriteLine("没有找到中括号内的内容，Json数据返回完全失败！");
            Debug.Log("没有找到中括号内的内容，Json数据返回完全失败！");
            return null;
        }
    }



    #region 问题实例
    [System.Serializable]
    public class QuestionData
    {
        public string Question;
    }

    [System.Serializable]
    public class QuestionList
    {
        public QuestionData[] questions;
    }
    #endregion
}

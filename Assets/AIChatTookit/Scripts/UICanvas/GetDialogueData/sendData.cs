using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebGLSupport;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System;


public class sendData : MonoBehaviour
{
    [Header("Settings 设置")]
    public Settings settings;
    [Header("底部输入")]
    public InputField UserInputField;

    [Header("对话气泡和顶框设置")]
    public string CurUserName;
    public string CurAIName;
    public Sprite UserHead;
    public Text HeadName;
    //public Sprite HeadImage;
    public Sprite AIHeadImage;
    [Header("声音播放")]
    public bool isSpeech;
    public sendSpeech speech;
    [Header("对话模型")]
    public AIModel aiModel;


    //LLM
    [Header("本地Ollama")]
    public string urlOllama;
    public string modelName;

    [Header("Silicon 设置")]
    public string url = "https://api.siliconflow.cn/v1/chat/completions";
    public List<Dictionary<string, string>> tempDialogue = new(); //存储所有对话
    private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
    private string SystemPre;

    //玩家输入的内容
    private string userInputText;
    //AI回复内容承接
    private string _response;

    //AI生成中
    private bool isAIRun = false;

    //日常行为生成路径
    private string savePath;

    private string saveDialogueEvent;

    [Header("生成日常行为路径生成")]
    public bool isDailyRoutine;
    [TextArea(10,5)]
    public string dailyPrompt;
    [TextArea(10, 5)]
    public string dailyPreprompt = "你扮演的角色是一个治愈形象松鼠小精灵，可以拟人化，但是需要基于松鼠的基本特效来生成下面的日程内容";

    [TextArea(10, 5)]
    [Header("事件总结Prompt")]
    public string GeneralizePrompt = @"接下来请总结一下上面的对话内容，根据对话的内容总结以下几点：
                                        1.让用户感到快乐、悲伤或其他鲜明情绪的大事。
                                        2.用户可能的性格特点。
                                        3.评估用户和你的关系，如果0分是完全不熟悉，10分是灵魂伴侣，请给你们直接的关系评分
                                        另外，你必须注意的是，回复的内容必须用Json的格式输出，例如：
     {""Dialogueevent"":""用户即将面临一场政治考试，他感到焦虑"",
      ""tempcharacter"":""有些自卑"",
      ""bearing"":""3""}";

    public void CreatBubble(string name, string content, bool isRight, Sprite head)
    {
        Bubble mybubble = new Bubble(name, content, isRight, head);
        BubbleSlider.Instance.bubbles.Add(mybubble);
        BubbleSlider.Instance.CreatBubble(mybubble);
    }

    private void Start()
    {
        UserInputField.keyboardType = TouchScreenKeyboardType.Social;

        //名字、图片设置
        HeadName.text = Settings.Instance.AIName;
        CurAIName = Settings.Instance.AIName;
        CurUserName = Settings.Instance.UserName;

        if(Settings.Instance.Headsprite != null)
        {
            UserHead = Settings.Instance.Headsprite;
        }

        if(Settings.Instance.tex != null)
        {
            AIHeadImage = settings.ConvertToSprite(Settings.Instance.tex);
        }

        //初始问候
        CreatBubble(CurAIName, "你好，我是"+ CurAIName + ",有什么想要分享的吗？", false, AIHeadImage);

        //日常路径初始化
        savePath = Application.dataPath + "/response.json";
        saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";

        //读取DialogueEvent中的共同记忆
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);

        //AI性格预设：
        SystemPre = $@"你现在正在扮演一个疗愈小精灵，
                        你的名字叫做{CurAIName}，
                        你的是性格是{settings.AICharacter},
                        这是你们之间对话中，发生的能够让用户感到快乐、悲伤或其他鲜明情绪的大事{DialogueEventjson}
                        请基于上述的姓名、性格特质和共同记忆，回复下面用户的话：";
        //选择生成初始路径模型
        //SendDaily(dailyPrompt, dailyPreprompt);
        //SendDailySilicon(dailyPrompt, dailyPreprompt);
        if (isDailyRoutine)
        {
            switch (aiModel)
            {
                case AIModel.Ollama_Local_Llama3_1:
                    SendDaily(dailyPrompt, dailyPreprompt);
                    break;
                case AIModel.Silicon_Llama_3_3_70B:
                    SendDailySilicon(dailyPrompt, dailyPreprompt);
                    break;
                default:
                    SendDailySilicon(dailyPrompt, dailyPreprompt);
                    break;
            }
        }
        

        //BubbleSlider.Instance.ReadsavePath();
    }

    void SaveData()
    {
        string jsonData = "{\"userName\":\"JohnDoe\",\"highScore\":100}";
        File.WriteAllText(savePath, jsonData);
        Debug.Log($"数据已保存到: {savePath}");
    }
    public void testfunc()
    {
        Debug.Log("调用On End");
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
            userInputText = text;
        }
        Debug.Log(userInputText);
        if (userInputText != "总结" && userInputText != null)
        {
            UserInputField.text = string.Empty;
            CreatBubble(CurUserName, userInputText, true, UserHead);
            //模型选择
            switch (aiModel)
            {
                case AIModel.Ollama_Local_Llama3_1:
                    SendToChat(userInputText, SystemPre);
                    break;
                case AIModel.Silicon_Llama_3_3_70B:
                    SendToChatSilicon(userInputText, SystemPre);
                    break;
                default:
                    SendToChatSilicon(userInputText, SystemPre);
                    break;
            }
            //SendToChat(userInputText,SystemPre);
            //SendToChat(dailyPrompt);
            //SendDaily(dailyPrompt);
        }
        else if(userInputText == "总结")
        {
            //UserInputField.text = string.Empty;
            //CreatBubble(CurUserName, userInputText, true, UserHead);
            //Debug.Log(GeneralizePrompt);
            //SendToGeneralizeSilicon(GeneralizePrompt);
            StartGenerilize();
        }

    }

    public void StartGenerilize()
    {
        UserInputField.text = string.Empty;
        CreatBubble(CurUserName, userInputText, true, UserHead);
        Debug.Log(GeneralizePrompt);
        SendToGeneralizeSilicon(GeneralizePrompt);
    }

    /// <summary>
    /// 给本地Ollama发送对话信息
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendToChat(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //直接构造json数据
            string json = $@"{{
                ""model"": ""{modelName}"",
                ""system"": ""{pre}"",
                ""prompt"": ""{prompt}"",
                ""stream"": false
                }}";
            //StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
            StartCoroutine(postRequestChat(urlOllama + "api/generate", json));
        }
        else
        {
            CreatBubble(CurAIName, "请稍等，我还在回复上一条信息~", false, null);
        }

    }

    IEnumerator postRequestChat(string url, string json)
    {
        isAIRun = true;
        HeadName.text = "对方正在输入……";
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            _response = uwr.downloadHandler.text;
            //retrieve response from the JSON
            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(_response);
            string responseJson = apiResponse.response;
            Debug.Log(responseJson);
            CreatBubble(CurAIName, responseJson, false, AIHeadImage);
            //CreatBubble(CurAIName, responseJson, false, null);
        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    /// <summary>
    /// 本地Ollama生成daliy routine
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendDaily(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            StartCoroutine(postDailyRequest(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
        }
        else
        {
            CreatBubble(CurAIName, "请稍等，我还在回复上一条信息~", false, AIHeadImage);
        }
    }

    IEnumerator postDailyRequest(string url, string json)
    {
        Debug.Log("now Start to Generate Daily Routine");
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Daily Routine: " + uwr.downloadHandler.text);
            //retrieve response from the JSON 牛逼 直接解析了
            _response = uwr.downloadHandler.text;
            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(_response);
            string responseJson = apiResponse.response;
            JArray formattedJson = JArray.Parse(responseJson);
            string formattedJsonString = formattedJson.ToString(Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(savePath, formattedJsonString);

            Debug.Log(responseJson);
            BubbleSlider.Instance.ReadsavePath();
        }
    }
 
    /// <summary>
    /// Silicon Llama3.3 70B对话
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendToChatSilicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //添加system预设
           if(tempDialogue.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", pre }
                };
                tempDialogue.Add(preMessage);
            }
            // 构造消息内容
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            //var messages = new List<Dictionary<string, string>>
            //{
            //        new Dictionary<string, string>
            //    {
            //        { "role", "user" },
            //        { "content", pre+prompt }
            //    }
            //};
            tempDialogue.Add(newmessage);

            var payload = new
            {
                model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            //StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
            StartCoroutine(postRequestSiliconChat(url, jsonPayload));
        }
        else
        {
            CreatBubble(CurAIName, "请稍等，我还在回复上一条信息~", false, AIHeadImage);
        }

    }

    IEnumerator postRequestSiliconChat(string url, string json)
    {
        isAIRun = true;
        HeadName.text = "对方正在输入……";
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            _response = uwr.downloadHandler.text;
            //retrieve response from the JSON
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            //Debug.Log(apiResponse.choices[0].message.content);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            //将获得的回复也加入到tempDialogue中：
            var responseMessage = new Dictionary<string, string>
            {
                {"role","assistant" },
                {"content", responseJson }
            };
            tempDialogue.Add(responseMessage);
            CreatBubble(CurAIName, responseJson, false, AIHeadImage);

            if (isSpeech)
            {
                speech.baiduTTS.Speak(responseJson, speech.PlayAudio);
            }

        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    /// <summary>
    /// silicon 生成Daily routine路径
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendDailySilicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
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
                model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = tempData,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            StartCoroutine(postDailySiliconRequest(url, jsonPayload));
        }
        else
        {
            CreatBubble(CurAIName, "请稍等，我还在回复上一条信息~", false, AIHeadImage);
        }
    }

    IEnumerator postDailySiliconRequest(string url, string json)
    {
        Debug.Log("now Start to Generate Silicon Daily Routine");
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Silicon Daily Routine: " + uwr.downloadHandler.text);
            //retrieve response from the JSON 牛逼 直接解析了
            _response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            string responseJson = apiResponse.choices[0].message.content;
            JArray formattedJson = JArray.Parse(responseJson);
            string formattedJsonString = formattedJson.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(savePath, formattedJsonString);
            Debug.Log(responseJson);
            BubbleSlider.Instance.ReadsavePath();
        }
    }

    /// <summary>
    /// Silicon总结对话内容
    /// </summary>
    /// <param name="prompt"></param>
    private void SendToGeneralizeSilicon(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            // 构造消息内容
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt}
            };

            tempDialogue.Add(newmessage);

            var payload = new
            {
                model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            //StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
            StartCoroutine(postRequestSiliconGeneralize(url, jsonPayload));
        }
        else
        {
            CreatBubble(CurAIName, "请稍等，我还在回复上一条信息~", false, AIHeadImage);
        }

    }

    IEnumerator postRequestSiliconGeneralize(string url, string json)
    {
        isAIRun = true;
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            _response = uwr.downloadHandler.text;
            //retrieve response from the JSON
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);

            GeneralizeAPI generalizeAPI = JsonUtility.FromJson<GeneralizeAPI>(responseJson);
            Debug.Log($"DialogueEvent:{generalizeAPI.Dialogueevent}");
            Debug.Log($"tempcharacter:{generalizeAPI.tempcharacter}");
            Debug.Log($"bearing:{generalizeAPI.bearing}");

            DateTime now = DateTime.Now;
            string formattedDate = now.ToString("yyyy-MM-dd HH:mm:ss");

            var payload = new
            {
                time = formattedDate,
                DialogueEvent = generalizeAPI.Dialogueevent+","
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            File.AppendAllText(saveDialogueEvent, jsonPayload);
            //CreatBubble(CurAIName, responseJson, false, null);
        }
        isAIRun = false;
    }

    public void SendToSiliconPicture(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            var content = new List<Content>
            {
                new Content
                {
                    type = "image_url",
                    image_url = new ImageUrl
                    {
                        url = "https://sf-maas-uat-prod.oss-cn-shanghai.aliyuncs.com/dog.png",
                        detail = "auto"
                    }
                }
            };

            Payload payload = new Payload
            {
                model = "Qwen/QVQ-72B-Preview",
                
                //model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = new List<ImageMessage>
            {
                new ImageMessage
                {
                    role = "user",
                    content = new List<Content>
                    {
                        new Content
                        {
                            type = "image_url",
                            image_url = new ImageUrl
                            {
                                url = "https://sf-maas-uat-prod.oss-cn-shanghai.aliyuncs.com/dog.png",
                                detail = "auto"
                            }
                        }
                    }
                }
            },
                stream = false
            };

            // 将payload对象序列化为JSON字符串
            string json = JsonUtility.ToJson(payload, true);
            //string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            StartCoroutine(postRequestSiliconImage(url, json));
        }
    }
    IEnumerator postRequestSiliconImage(string url, string json)
    {
        isAIRun = true;
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            _response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
        }
        isAIRun = false;
    }

    #region Ollama回复解析实例
    public class ApiResponse
    {
        public string response;
    }
    #endregion

    #region Silicon回复解析实例
    [System.Serializable]
    public class ApiSilicion
    {
        public List<Choice> choices;
    }
    [System.Serializable]
    public class Choice
    {
        public Message message;
    }
    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
    #endregion

    #region 总结解析实例
    [System.Serializable]
    public class GeneralizeAPI
    {
        public string Dialogueevent;
        public string tempcharacter;
        public string bearing;
    }
    #endregion
    // 定义JSON对象的类
    // 定义JSON对象的类
    [System.Serializable]
    public class Payload
    {
        public string model;
        public List<ImageMessage> messages;
        public bool stream;
    }

    [System.Serializable]
    public class ImageMessage
    {
        public string role;
        public List<Content> content;
    }

    [System.Serializable]
    public class Content
    {
        public string type;
        public ImageUrl image_url;
    }

    [System.Serializable]
    public class ImageUrl
    {
        public string url;
        public string detail;
    }
}

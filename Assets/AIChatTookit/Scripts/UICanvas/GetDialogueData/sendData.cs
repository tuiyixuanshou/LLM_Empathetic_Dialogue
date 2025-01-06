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


public class sendData : MonoBehaviour
{
    [Header("底部输入")]
    public InputField UserInputField;

    [Header("对话气泡和顶框设置")]
    public string CurUserName;
    public string CurAIName;
    public Sprite UserHead;

    public Text HeadName;
    public Sprite HeadImage;
    public Sprite AIHeadImage;

    //LLM
    [Header("本地Ollama")]
    public string urlOllama;
    public string modelName;


    private string userInputText;
    private string _response;
    private string preprompt = "你扮演的角色是一个治愈形象松鼠小精灵，可以拟人化，但是需要基于松鼠的基本特效来生成下面的日程内容";
    private string preprompt2 = "记住你的名字是小智,你的性格是乐观善良，但是经常有点脱线，你是一个治愈小精灵";


    private bool isAIRun = false;

    

    private string savePath;

    [Header("生成日常行为路径")]
    [TextArea(15,20)]
    public string dailyPrompt;

    public class ApiResponse
    {
        public string response;
    }

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
            AIHeadImage = Settings.Instance.ConvertToSprite(Settings.Instance.tex);
        }

        //初始问候
        CreatBubble(CurAIName, "你好，我是"+ CurAIName + ",有什么想要分享的吗？", false, AIHeadImage);

        //日常路径初始化
        savePath = Application.dataPath + "/response.json";

        //生成初始路径
        SendDaily(dailyPrompt, preprompt);

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
    public void GetTextAndClear()
    {
        Debug.Log("GetText");
        userInputText = UserInputField.text;
        Debug.Log(userInputText);
        if(userInputText != "总结")
        {
            UserInputField.text = string.Empty;
            CreatBubble(CurUserName, userInputText, true, UserHead);


            SendToChatSilicon(userInputText, SiliconPre);
            //SendToChat(userInputText,preprompt2);
            //SendToChat(dailyPrompt);
            //SendDaily(dailyPrompt);
        }
        else if(userInputText == "总结")
        {
            UserInputField.text = string.Empty;
            CreatBubble(CurUserName, userInputText, true, UserHead);
            string GeneralizePrompt = "接下来请总结一下上面的对话内容，根据对话的内容总结以下几点：1.让用户感到快乐、悲伤或其他鲜明情绪的大事。2.用户可能的性格特点。3.评估用户和你的关系，如果0分是完全不熟悉，10分是灵魂伴侣，请给你们直接的关系评分";
            SendToGeneralizeSilicon(GeneralizePrompt);
        }

    }

    /// <summary>
    /// 本地Ollama
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <returns></returns>
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

    private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
    private string SiliconPre = "你现在正在扮演一个疗愈小精灵，你的名字叫做小智，你的是性格是乐观善良，但是经常有点脱线。下面是用户发给你的内容：";
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
            CreatBubble(CurAIName, responseJson, false, AIHeadImage);
        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    private void SendToChat(string prompt,string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
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

    private string url = "https://api.siliconflow.cn/v1/chat/completions";

    private List<Dictionary<string,string>> tempDialogue = new();

    private void SendToChatSilicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //string userMessage = "你好啊"; // 用户消息
            //string assistantMessage = "嘿嘿!好啊!我就是小智！*挥手打招呼* 我很高兴见到你呢！你能告诉我今天是什么日子吗？是不是又要来一场盛大的雨天呢？*期待地眨眼*"; // 助手消息

            // 构造消息内容
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", pre+prompt }
            };
            //var messages = new List<Dictionary<string, string>>
            //{
            //        new Dictionary<string, string>
            //    {
            //        { "role", "user" },
            //        { "content", pre+prompt }
            //        //{ "system", pre }
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
    private void SendToGeneralizeSilicon(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            // 构造消息内容
            var newmessage = new Dictionary<string, string>
            {
                {"role","system" },
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
            //CreatBubble(CurAIName, responseJson, false, null);
        }
        isAIRun = false;
    }

    private void SendDaily(string prompt,string pre)
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

}

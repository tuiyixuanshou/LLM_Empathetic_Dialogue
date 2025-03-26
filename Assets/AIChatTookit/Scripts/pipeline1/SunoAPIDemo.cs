using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System;
using Unity.VisualScripting;
using static sendData;

public class SunoAPIDemo : MonoBehaviour
{
    public API_CentralControl api_CentralControl;
    public sendSpeech speech;
    //public AvaterBubbleControl bubbleControl;
    public Settings settings;

    [Header("voice内容")]
    public ChatModel Mvoice_Model;
    public LLMURL Mvoice_url;
    public APIKey Mvoice_api;


    public AudioSource audioSource;
    private string SunoAPIURL = "https://apibox.erweima.ai/api/v1/generate";

    private string SunoAPIKey = "d221dd1b31da8a9b3df854a9e266ad39";
    [TextArea(5,5)]
    public string SunoPrompt;
    private string Cur_taskID = string.Empty;
    private string Cur_status = "PENDING";
    private string Cur_Sound_URL = string.Empty;

    public List<string> Cur_Inventory;


    //evaluate 整个流程


    //图片生成

    //LLM生成其他模态的prompt测试

    //用户性格收集

    //流程打磨

    private string PrePrompt;
    private string Cur_text;


    private void Start()
    {
        //StartCoroutine(Auto_CheckCurTask("3ab1c9c9285516b6aaf48ec29283bfd3"));
        PrePrompt = $@"你现在正在扮演一个疗愈小精灵，你的名字叫做{settings.AIName}，你的是性格是{settings.AICharacter}。";
        //StartCoroutine(TTSTry("用户当前情绪较为平稳但存在未表达的情感，睡眠问题可能暗示潜在压力"));
    }

    public void MSound_API_Send(string evaluateResult)
    {
        //StartCoroutine(PostRequest());
        StartCoroutine(TTSTry(evaluateResult));
    }
    IEnumerator TTSTry(string evaluateResult)
    {
        string Json = JsonConvert.SerializeObject(Cur_Inventory);
        string tempDialogue = JsonConvert.SerializeObject(settings.tempDialogue);
        //请选择一个你觉得最合适的，如果都不合适，再生成一个。
        string prompt = $@"这是用户最近一次调查的心理状态评估结果：{evaluateResult}，这是近期内用户和大语言代理的对话内容:{tempDialogue}，
请你选择一种说话的方式和内容，在此时主动进行发起。
这是目前的说话内容语料库：{Json}，请你在当中选择一句此时最合适的内容，可以是安慰用户，也可以是打破用户此时的现状。
如果都不合适，你也可以参考内容库，重新生成一句话，注意内容需要软萌，符合QQ小动物会说的简短内容，不要过于复杂，仅返回说话内容即可。";
        Debug.Log(prompt);
        List<Dictionary<string, string>> temp = new();
        var preMessage = new Dictionary<string, string>
        {
            {"role", "system" },
            {"content",PrePrompt }
        };
        var newMessage = new Dictionary<string, string>
        {
            {"role", "user" },
            {"content",prompt }
        };
        temp.Add(preMessage);
        temp.Add(newMessage);
        var payload = new
        {
            model = settings.m_SetModel(Mvoice_Model),
            messages = temp,
            stream = false
        };
        string JsonPayload = JsonConvert.SerializeObject(payload);
        yield return postRequest(settings.m_SetUrl(Mvoice_url), settings.m_SetApi(Mvoice_api), JsonPayload, Voice_CallBack);
        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);
        speech.SpeakFunction(TTSs.AzureTTS, Cur_text);
        api_CentralControl.api_Chat.Mchat_API_CallBack(Cur_text);
    }
    //加入语料库
    void Voice_CallBack(string text)
    {
        Cur_text = text;
        if (!Cur_Inventory.Contains(text))
        {
            Cur_Inventory.Add(text);
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


    IEnumerator PostMusicRequest()
    {
        Debug.Log("Suno开始生成音乐");
        using(UnityWebRequest request = new UnityWebRequest(SunoAPIURL, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {SunoAPIKey}");

            var RequestBody = new
            {
                prompt = SunoPrompt,
                style = "Classical",
                customMode = false,
                instrumental = true,
                model = "V3_5",
                callBackUrl = "https://api.example.com/callback"
            };

            string JsonRequest = JsonConvert.SerializeObject(RequestBody);
            Debug.Log("Request Body: " + JsonRequest);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonRequest);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            // 处理响应
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Full Response: " + request.downloadHandler.text);
                Debug.LogError("Request Error: " + request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
                Cur_taskID = JsonConvert.DeserializeObject<SunoGenerate>(request.downloadHandler.text).data.taskId;
                Debug.Log(Cur_taskID);
                StartCoroutine(Auto_CheckCurTask(Cur_taskID));
            }

        }
    }

    IEnumerator Auto_CheckCurTask(string cur_ID)
    {
        string CurUrl = "https://apibox.erweima.ai/api/v1/generate/record-info?taskId=" + cur_ID;
        Debug.Log(CurUrl);
        Cur_status = "PENDING";
        string respond = string.Empty;
        while(Cur_status != "SUCCESS")
        {
            yield return new WaitForSeconds(10f);
            using (UnityWebRequest request = new UnityWebRequest(CurUrl, "GET"))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {SunoAPIKey}");
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Full Response: " + request.downloadHandler.text);
                    Debug.LogError("Request Error: " + request.error);
                    break;
                }
                else
                {
                    Debug.Log("Response: " + request.downloadHandler.text);
                    Cur_status = JsonConvert.DeserializeObject<SunoWait>(request.downloadHandler.text).data.status;
                    respond = request.downloadHandler.text;
                }
            }
        }
        if(Cur_status == "SUCCESS" && respond != string.Empty)
        {
            Cur_Sound_URL = JsonConvert.DeserializeObject<SunoRead>(respond).data.response.sunoData[0].audioUrl;
            Debug.Log(Cur_Sound_URL);
            StartCoroutine(DownloadAudio(Cur_Sound_URL));
        }

    }

    IEnumerator DownloadAudio(string Sound_URL)
    {
        Debug.Log("开始下载音乐");
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(Sound_URL, AudioType.MPEG);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Error downloading audio: " + webRequest.error);
        }
    }

    #region 读取Suno Json
    [Serializable]
    public class SunoData
    {
        public string audioUrl;
    }
    [Serializable]
    public class Response
    {
        public List<SunoData> sunoData;
    }
    [Serializable]
    public class DataRead
    {
        public Response response;
        public string status;
    }

    [Serializable]
    public class SunoRead
    {
        public string code;
        public DataRead data;
    }

    [Serializable]
    public class DataWait
    {
        public string status;
    }

    [Serializable]
    public class SunoWait
    {
        public string code;
        public DataWait data;
    }

    [Serializable]
    public class DataGenerate
    {
        public string taskId;
    }

    [Serializable]
    public class SunoGenerate
    {
        public string code;
        public DataGenerate data;
    }
    #endregion
}



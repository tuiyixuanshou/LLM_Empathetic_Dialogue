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

    [Header("voice����")]
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


    //evaluate ��������


    //ͼƬ����

    //LLM��������ģ̬��prompt����

    //�û��Ը��ռ�

    //���̴�ĥ

    private string PrePrompt;
    private string Cur_text;


    private void Start()
    {
        //StartCoroutine(Auto_CheckCurTask("3ab1c9c9285516b6aaf48ec29283bfd3"));
        PrePrompt = $@"���������ڰ���һ������С���飬������ֽ���{settings.AIName}��������Ը���{settings.AICharacter}��";
        //StartCoroutine(TTSTry("�û���ǰ������Ϊƽ�ȵ�����δ������У�˯��������ܰ�ʾǱ��ѹ��"));
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
        //��ѡ��һ�����������ʵģ�����������ʣ�������һ����
        string prompt = $@"�����û����һ�ε��������״̬���������{evaluateResult}�����ǽ������û��ʹ����Դ���ĶԻ�����:{tempDialogue}��
����ѡ��һ��˵���ķ�ʽ�����ݣ��ڴ�ʱ�������з���
����Ŀǰ��˵���������Ͽ⣺{Json}�������ڵ���ѡ��һ���ʱ����ʵ����ݣ������ǰ�ο�û���Ҳ�����Ǵ����û���ʱ����״��
����������ʣ���Ҳ���Բο����ݿ⣬��������һ�仰��ע��������Ҫ���ȣ�����QQС�����˵�ļ�����ݣ���Ҫ���ڸ��ӣ�������˵�����ݼ��ɡ�";
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
    //�������Ͽ�
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
        Debug.Log("Suno��ʼ��������");
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

            // ������Ӧ
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
        Debug.Log("��ʼ��������");
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

    #region ��ȡSuno Json
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



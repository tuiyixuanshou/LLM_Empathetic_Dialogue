using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;

//using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using static sendData;
using MyUtilities;
using static AvatarMainStoryDemoV2;

public class KLingAPIDemo : MonoBehaviour
{
    public quadVideo quadvideo;
    public AvaterBubbleControl bubbleControl;
    public Settings settings;
    public API_CentralControl api_CentralControl;
    public AvatarMainStoryDemoV2 MainStory;
    private string apiUrl = "https://api.klingai.com/v1/videos/text2video";
    private string apiImage2Vedio = "https://api.klingai.com/v1/videos/image2video";
    public string jwtToken; // JWT Token��ͨ��ǰ��Ĵ�������
    //public string prompt = $@"����С����������Լ�ͼƬ�������Զ�����һ�κ���ġ����Ȳ����С���ﶯ�������ֳ�С��������ƣ�������ɺ����ص���һ֡��״̬����β֡�������֡��ͬ��ͬʱ��ͷ�̶�����"; // ��Ƶ���ɵ��ı���ʾ

    public string secretKey;
    public string accessKey;

    private string default_imageUrl = "Assets\\AIChatTookit\\Video\\avatar\\С���ﾲ̬.png";
    public string ImageBase64;

    private string cur_Task_ID;
    private string cur_Vedio_URL;
    [Header("KLingģ������")]
    public string Model;
    public string prompt = $@"���ڽ��ͼƬ����ԭ�������Լ�������ǰ���£��Զ�����һ�κ���ġ����Ƚ�С��С������ֶ�����������ɺ����ص���һ֡��״̬����β֡�������֡��ͬ��ͬʱ��ͷ�̶�����";
    [Range(0,1)]
    public float Cfg_scale;
    public string MOD;

    [Header("��鶯����������ģ������")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    [Header("����prompt����ģ������")]
    public ChatModel Maction_Model;
    public LLMURL Maction_url;
    public APIKey Maction_api;

    public ShareMomentControl shareMomentControl;

    private SceneDetails sceneDetails;

    private string cur_scene;
    private string cur_Move = string.Empty;
    private string cur_Chat = string.Empty;
    //string PrePrompt;



    void Start()
    {
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);
        
        //StartCoroutine(checkTaskList());
    //TO DO:Ĭ����Ƭ��Ҫ�޸ģ��޸ĳɵ�������Ƭ����������Ҳ��Ҫ�洢���ı���
    //mageBase64 = ReadDefaultImage("C:\\Users\\TF\\Desktop\\InRoomStatic.txt");
        //prompt = "��ͷ����̶�������ͼ�е�С���ﲦŪ�����ϵĵ����ǡ�";
        //StartCoroutine(GenerateVideo("���к�����", AutoBroadCallBack));
        //StartCoroutine(checkSingleTask());
        //StartCoroutine(checkTaskList());
    }
    public void ButtonTestCheck()
    {
        //StartCoroutine(checkSingleTask());
    }

    IEnumerator checkSingleTask(string url)
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        yield return new WaitForSeconds(1f); // ��ֹ˲��������TokenʧЧ
        //using (UnityWebRequest request = new UnityWebRequest("https://api.klingai.com/v1/videos/text2video/ChCUFWfGxxgAAAAAAEofZg", "GET"))
        using (UnityWebRequest request = new UnityWebRequest("url", "GET"))
        {
            // ��������ͷ
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            // ��������
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
                //HandleResponse(request.downloadHandler.text);
            }
        }

    }

    IEnumerator checkTaskList()
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        yield return new WaitForSeconds(1f); // ��ֹ˲��������TokenʧЧ
        using(UnityWebRequest request = new UnityWebRequest("https://api.klingai.com/v1/videos/image2video?pageNum=1&pageSize=30", "GET"))
        {
            // ��������ͷ
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            // ��������
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
                //HandleResponse(request.downloadHandler.text);
            }
        }

    }

    public void MAction_API_Send(string Evaluate)
    {
        StartCoroutine(ActionProcesse(Evaluate));
    }

    IEnumerator ActionProcesse(string Evaluate)
    {
        //���ﶼ��Ҫ���ǵ�ͼƬ�ı�������ʱû�м���
        string Aciton_prompt = $@"����һ����Ϊѧ������ѧר�ң��û����ڵ�����״̬�ǣ�{Evaluate}��
����˼���������Ķ����ܹ��𵽰�ο�û��������û������������������˼����������������ͼ�����磺
���б�������Ķ�������ӵ��������ȣ����н�����ͼ�Ķ��������Ү�����衢΢Ц�ȣ����н�����ͼ�Ķ��������Ү�����衢΢Ц�ȣ����й�����ͼ�Ķ�������������ͷ������ǰ�㡢��б�Դ��ȡ�
��������һ����ͼ��һ�ֶ��������̶���ƥ���û����ڵ�����״̬���ظ����ݾ����ܼ��׶����硰���ֶ�������ӵ���������������㵸��������";

        yield return ActionPromptGen(Aciton_prompt, Action_Prompt_Set);

        string Action_Chat = $@"����Ϊһ���������С���飬�㼴������{cur_Move}������˼���������������ʱ���ܻ���û�˵һЩʲô����
���⣬�����û����ڵ�����״̬�����Ը����ṩ�ο���{Evaluate}��ֻ�ظ����ɵĻ���������Ҫ�࣬������Ȼ���쳡����";

        StartCoroutine(ActionPromptGen(Action_Chat, chat_CallBack));

        yield return GenerateVideo(prompt, AutoBroadCallBack);
        yield return new WaitForSeconds(0.5f);
        //Mchat_API_CallBack(cur_Chat);
        api_CentralControl.api_Chat.Mchat_API_CallBack(cur_Chat);
        api_CentralControl.isMultiRespondStart = false; 
    }

    void Action_Prompt_Set(string text)
    {
        cur_Move = text;
        //prompt = $@"���ڽ��ͼƬ����ԭ�������Լ�������ǰ���£��Զ�����һ�κ���ġ����Ƚ�С��{text}��������ɺ����ص���һ֡��״̬����β֡�������֡��ͬ��ͬʱ��ͷ�̶�����";
        prompt = $@"���ڽ��ͼƬ����ԭ�������Լ�������ǰ���£��Զ�����һ�κ����{text},�����ͱ�����Ҫ�ɰ�����";
    }

    void chat_CallBack(string respond)
    {
        cur_Chat = respond;
    }

    IEnumerator ActionPromptGen(string prompt, Action<string> callback)
    {
        List<Dictionary<string, string>> temp = new();
        var message = new Dictionary<string, string>
        {
            {"role","system"},
            {"content",prompt}
        };
        temp.Add(message);
        var payload = new
        {
            model = settings.m_SetModel(Maction_Model),
            messages = temp,
            stream = false,
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        var uwr = new UnityWebRequest(settings.m_SetUrl(Maction_url), "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(Jsonpayload);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + settings.m_SetApi(Maction_api));
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

    public void VideoPrompt(int timeIndex)
    {
        Debug.Log("��ʼ��Ƶprompt����+"+timeIndex);
        cur_scene = shareMomentControl.Scene_Desicribe[timeIndex];
        string prompt = $@"���ǽ�ɫ���ܼƻ�������ƣ�{MainStory.concreteBehaviors[timeIndex].title}
��ļ�����{MainStory.concreteBehaviors[timeIndex].description}
�������ɵ����ݣ�{cur_scene}
��˼���������ڴ˳����£��������˹��������һ���������Ƶ����Ƶ�е����嶯������Ӱ����Χ��ʲô���ģ���Ҫ����
�ظ���������Json��ʽ
[
    {{
    ""���嶯��"":""����"",
    ""��Ӱ"":""����"",
    ""��Χ"":""����""
    }}
]
�벻Ҫ���س�Json����������κ�����";
        Debug.Log(prompt);
        List<Dictionary<string, string>> curList = new();
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        curList.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(Maction_Model),
            messages = curList,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(Maction_url), settings.m_SetApi(Maction_api), Jsonpayload, VideoPrompt_CallBack));
    }

    void VideoPrompt_CallBack(string text)
    {
        string Json = PostWeb.JsonPatch(text);
        try
        {
            Debug.Log("����Json");
            sceneDetails = JsonConvert.DeserializeObject<List<SceneDetails>>(Json)[0];
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }

        string prompt = $@"
����:��ԭͼ�����еİ�ɫС����Ϊ���塣��Ҫ����������ɫ��
���嶯��:{sceneDetails.���嶯��}�������˶�����
����:����ԭͼ�����еĳ�������
��ͷ����:���־�ͷ�̶�����
��Ӱ:{sceneDetails.��Ӱ}
��Χ:{sceneDetails.��Χ}��������ܰ������";
        Debug.Log(prompt);

        StartCoroutine(GenerateVideo(prompt, AutoBroadCallBack));
    }

    public IEnumerator GenerateVideo(string prompt,Action<String> AutoCheckCallBack)
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        //Debug.Log("Generated JWT Token: " + jwtToken);
        yield return new WaitForSeconds(1f); // ��ֹ˲��������TokenʧЧ

        using (UnityWebRequest request = new UnityWebRequest(apiImage2Vedio, "POST"))
        {
            // ��������ͷ
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");

            //��ȡ��֡��Image��Base64����URL��
            //ImageBase64 = ReadImageBase64();
            var scene = settings.Share_Scenes_List.Find(s => s.Scene_Describe == cur_scene);
            string ImageBase64 = tools.LoadBase64FromPath(scene.First_Frame_Image);

            // ���� JSON ������
            string requestBody = JsonConvert.SerializeObject(new
            {
                model_name = "kling-v1-6",
                prompt = prompt,
                image = ImageBase64,
                //image = default_imageUrl,
                image_tail = ImageBase64,
                cfg_scale = 0.85,
                mode = "pro"
            });
            Debug.Log(prompt);

            Debug.Log("Request Body: " + requestBody);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // ��������
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
                cur_Task_ID = JsonConvert.DeserializeObject<KlingBackWait>(request.downloadHandler.text).data.task_id;
                //HandleResponse(request.downloadHandler.text);
               yield return Auto_CheckCurTask(cur_Task_ID, AutoCheckCallBack);
            }
        }
    }

    string ReadImageBase64()
    {
        //�Ҿ��Ż�����List�ȽϺã� cur_Scene���Բ��䣬��һ��Dictionary<Key,Value>���洢
        switch (quadvideo.cur_Scene)
        {
            case M_Scene.InDoor_Sofa:
                return ReadDefaultImage("C:\\Users\\TF\\Desktop\\InRoomStatic.txt");
            case M_Scene.cafe:
                return ReadDefaultImage("C:\\Users\\TF\\Desktop\\defualt_Image.txt");
            default:
                Debug.Log("�������⣡");
                return null;
        }
    }

    private string ReadDefaultImage(string filePath)
    {
        if (File.Exists(filePath))
        {
            // ʹ��File.ReadAllText������ȡ�ļ�����
            string fileContent = File.ReadAllText(filePath);
            return fileContent;
        }
        else
        {
            Debug.LogError("�ļ������ڣ�����·���Ƿ���ȷ��");
            return null;
        }
    }

    //�����Զ���ѯ
    IEnumerator Auto_CheckCurTask(string Take_ID,Action<string> callBack)
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        string new_URL = "https://api.klingai.com/v1/videos/image2video/" + Take_ID;
        Debug.Log(new_URL);
        string cur_task_status = "processing";
        string cur_respond = string.Empty;
        while(cur_task_status != "succeed")
        {
            yield return new WaitForSeconds(10f);             
            using (UnityWebRequest request = new UnityWebRequest(new_URL, "GET"))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Full Response: " + request.downloadHandler.text);
                    Debug.LogError("Request Error: " + request.error);
                    //TO DO:���￼��jwtʧЧ�����
                    break;
                }
                else
                {
                    Debug.Log("Response: " + request.downloadHandler.text);
                    cur_task_status = JsonConvert.DeserializeObject<KlingBackWait>(request.downloadHandler.text).data.task_status;
                    //HandleResponse(request.downloadHandler.text);
                    cur_respond = request.downloadHandler.text;
                }
            } 
        }

        if(cur_task_status == "succeed" && cur_respond != string.Empty)
        {
            cur_Vedio_URL = JsonConvert.DeserializeObject<KlingBackRead>(cur_respond).data.task_result.videos[0].url;
            Debug.Log(cur_Vedio_URL);
            yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);  //������֤���ڶԻ��С�
            callBack(cur_Vedio_URL);
        }
        else
        {
            Debug.Log("��Ƶ��ѯʱ�������⣡");
        }
    }

    public void AutoBroadCallBack(string url)
    {
        Debug.Log("��Ƶ���ݣ�" + url);
        var scene = settings.Share_Scenes_List.Find(s => s.Scene_Describe == cur_scene);
        scene.Video_Links.Add(url);
        //��Ƶ�������
        //settings.CurSceneName = shareMomentControl.shareMomentDetail.Scene_Decision;
        //if (!quadvideo.isStartPlayVideo) quadvideo.isStartPlayVideo = true;
        //quadvideo.RespondToM_Action(url);
        //api_CentralControl.api_Chat.Mchat_API_FreePrompt("", true, Mchat_Model, Mchat_url, Mchat_api);
        //settings.Scenes_Dict[cur_scene].Video_Links.Add(url);
        //if (settings.Scenes_Dict[settings.CurSceneName].Video_Links.Count < 1)
        //{
        //    VideoPrompt();
        //}
    }

    public static string EncodeJwtToken(string accessKey, string secretKey)
    {
        var header = new Dictionary<string, object>
        {
            { "alg", "HS256" },
            { "typ", "JWT" }
        };

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = new Dictionary<string, object>
        {
            { "iss", accessKey },
            { "exp", currentTime + 1800 }, // 30���Ӻ����
            { "nbf", currentTime - 5 }     // 5��ǰ��Ч
        };

        string headerBase64 = Base64UrlEncode(JsonConvert.SerializeObject(header));
        string payloadBase64 = Base64UrlEncode(JsonConvert.SerializeObject(payload));

        string signatureInput = $"{headerBase64}.{payloadBase64}";
        byte[] signatureBytes = HmacSha256(signatureInput, secretKey);
        string signatureBase64 = Base64UrlEncode(signatureBytes);

        return $"{signatureInput}.{signatureBase64}";
    }

    private static string Base64UrlEncode(string input)
    {
        return Base64UrlEncode(Encoding.UTF8.GetBytes(input));
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] HmacSha256(string data, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }

    #region ��ȡkling Json
    [System.Serializable]
    public class SceneDetails
    {
        public string ���嶯��;
        public string ��Ӱ;
        public string ��Χ;
    }

    [Serializable]
    public class Data
    {
        public string task_id;
        public string task_status;
    }
    [Serializable]
    public class KlingBackWait
    {
        public int code;
        public string message;
        public Data data;
    }

    [Serializable]
    public class Videos
    {
        public string url;
    }

    [Serializable]
    public class TaskResult
    {
        public List<Videos> videos;
    }

    [Serializable]
    public class DataRead
    {
        public string task_id;
        public string task_status;
        public TaskResult task_result;
    }

    [Serializable]
    public class KlingBackRead
    {
        public int code;
        public string message;
        public DataRead data;
    }
    #endregion
}

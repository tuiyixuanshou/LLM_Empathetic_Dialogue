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
    public string jwtToken; // JWT Token，通过前面的代码生成
    //public string prompt = $@"请结合小动物的坐姿以及图片背景，自动生成一段合理的、幅度不大的小动物动作，体现出小动物的友善，动作完成后必须回到第一帧的状态，即尾帧必须和首帧相同。同时镜头固定不动"; // 视频生成的文本提示

    public string secretKey;
    public string accessKey;

    private string default_imageUrl = "Assets\\AIChatTookit\\Video\\avatar\\小动物静态.png";
    public string ImageBase64;

    private string cur_Task_ID;
    private string cur_Vedio_URL;
    [Header("KLing模型设置")]
    public string Model;
    public string prompt = $@"请在结合图片主体原有姿势以及背景的前提下，自动生成一段合理的、幅度较小的小动物挥手动作，动作完成后必须回到第一帧的状态，即尾帧必须和首帧相同。同时镜头固定不动";
    [Range(0,1)]
    public float Cfg_scale;
    public string MOD;

    [Header("陪伴动作语言生成模型设置")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    [Header("动作prompt生成模型设置")]
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
    //TO DO:默认照片需要修改，修改成当场的照片，其他内容也需要存储到文本中
    //mageBase64 = ReadDefaultImage("C:\\Users\\TF\\Desktop\\InRoomStatic.txt");
        //prompt = "镜头必须固定不动，图中的小动物拨弄架子上的地球仪。";
        //StartCoroutine(GenerateVideo("打招呼动作", AutoBroadCallBack));
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
        yield return new WaitForSeconds(1f); // 防止瞬间请求导致Token失效
        //using (UnityWebRequest request = new UnityWebRequest("https://api.klingai.com/v1/videos/text2video/ChCUFWfGxxgAAAAAAEofZg", "GET"))
        using (UnityWebRequest request = new UnityWebRequest("url", "GET"))
        {
            // 设置请求头
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            // 发送请求
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
                //HandleResponse(request.downloadHandler.text);
            }
        }

    }

    IEnumerator checkTaskList()
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        yield return new WaitForSeconds(1f); // 防止瞬间请求导致Token失效
        using(UnityWebRequest request = new UnityWebRequest("https://api.klingai.com/v1/videos/image2video?pageNum=1&pageSize=30", "GET"))
        {
            // 设置请求头
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            // 发送请求
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
        //这里都需要考虑到图片的背景，暂时没有加入
        string Aciton_prompt = $@"你是一个行为学、心理学专家，用户现在的心理状态是：{Evaluate}。
请你思考：怎样的动作能够起到安慰用户、给予用户陪伴的能力。你可以先思考动作有怎样的意图，例如：
具有保护意义的动作，如拥抱、邀请等；具有交流意图的动作，如比耶、跳舞、微笑等；具有交流意图的动作，如比耶、跳舞、微笑等；具有共情意图的动作如聆听、点头、身体前倾、倾斜脑袋等。
请你生成一种意图的一种动作，最大程度上匹配用户现在的心理状态。回复内容尽可能简单易懂，如“挥手动作”或“拥抱动作”或“手舞足蹈动作”等";

        yield return ActionPromptGen(Aciton_prompt, Action_Prompt_Set);

        string Action_Chat = $@"你做为一个陪伴疗愈小精灵，你即将做出{cur_Move}。请你思考在做出这个动作时可能会和用户说一些什么话。
此外，这是用户现在的心理状态，可以给你提供参考：{Evaluate}。只回复生成的话，字数不要多，符合自然聊天场景。";

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
        //prompt = $@"请在结合图片主体原有姿势以及背景的前提下，自动生成一段合理的、幅度较小的{text}，动作完成后必须回到第一帧的状态，即尾帧必须和首帧相同。同时镜头固定不动";
        prompt = $@"请在结合图片主体原有姿势以及背景的前提下，自动生成一段合理的{text},动作和表情需要可爱软萌";
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
        Debug.Log("开始视频prompt生成+"+timeIndex);
        cur_scene = shareMomentControl.Scene_Desicribe[timeIndex];
        string prompt = $@"这是角色本周计划活动的名称：{MainStory.concreteBehaviors[timeIndex].title}
活动的简述：{MainStory.concreteBehaviors[timeIndex].description}
场景生成的内容：{cur_scene}
请思考，若是在此场景下，根据主人公活动来生成一个五秒的视频，视频中的主体动作、光影、氛围是什么样的？简要描述
回复内容请用Json格式
[
    {{
    ""主体动作"":""内容"",
    ""光影"":""内容"",
    ""氛围"":""内容""
    }}
]
请不要返回除Json数据以外的任何内容";
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
            Debug.Log("解析Json");
            sceneDetails = JsonConvert.DeserializeObject<List<SceneDetails>>(Json)[0];
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 解析失败：" + ex.Message);
        }

        string prompt = $@"
主体:已原图中已有的白色小动物为主体。不要出现其他角色。
主体动作:{sceneDetails.主体动作}。符合运动规律
场景:保持原图中已有的场景不变
镜头语言:保持镜头固定不变
光影:{sceneDetails.光影}
氛围:{sceneDetails.氛围}。环境温馨治愈。";
        Debug.Log(prompt);

        StartCoroutine(GenerateVideo(prompt, AutoBroadCallBack));
    }

    public IEnumerator GenerateVideo(string prompt,Action<String> AutoCheckCallBack)
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        //Debug.Log("Generated JWT Token: " + jwtToken);
        yield return new WaitForSeconds(1f); // 防止瞬间请求导致Token失效

        using (UnityWebRequest request = new UnityWebRequest(apiImage2Vedio, "POST"))
        {
            // 设置请求头
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");

            //读取首帧的Image（Base64或者URL）
            //ImageBase64 = ReadImageBase64();
            var scene = settings.Share_Scenes_List.Find(s => s.Scene_Describe == cur_scene);
            string ImageBase64 = tools.LoadBase64FromPath(scene.First_Frame_Image);

            // 生成 JSON 请求体
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

            // 发送请求
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
                cur_Task_ID = JsonConvert.DeserializeObject<KlingBackWait>(request.downloadHandler.text).data.task_id;
                //HandleResponse(request.downloadHandler.text);
               yield return Auto_CheckCurTask(cur_Task_ID, AutoCheckCallBack);
            }
        }
    }

    string ReadImageBase64()
    {
        //我觉着还是用List比较好？ cur_Scene可以不变，用一个Dictionary<Key,Value>来存储
        switch (quadvideo.cur_Scene)
        {
            case M_Scene.InDoor_Sofa:
                return ReadDefaultImage("C:\\Users\\TF\\Desktop\\InRoomStatic.txt");
            case M_Scene.cafe:
                return ReadDefaultImage("C:\\Users\\TF\\Desktop\\defualt_Image.txt");
            default:
                Debug.Log("出现问题！");
                return null;
        }
    }

    private string ReadDefaultImage(string filePath)
    {
        if (File.Exists(filePath))
        {
            // 使用File.ReadAllText方法读取文件内容
            string fileContent = File.ReadAllText(filePath);
            return fileContent;
        }
        else
        {
            Debug.LogError("文件不存在，请检查路径是否正确！");
            return null;
        }
    }

    //进行自动查询
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
                    //TO DO:这里考虑jwt失效的情况
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
            yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);  //基础保证不在对话中。
            callBack(cur_Vedio_URL);
        }
        else
        {
            Debug.Log("视频查询时遇到问题！");
        }
    }

    public void AutoBroadCallBack(string url)
    {
        Debug.Log("视频内容：" + url);
        var scene = settings.Share_Scenes_List.Find(s => s.Scene_Describe == cur_scene);
        scene.Video_Links.Add(url);
        //视频生成完毕
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
            { "exp", currentTime + 1800 }, // 30分钟后过期
            { "nbf", currentTime - 5 }     // 5秒前生效
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

    #region 读取kling Json
    [System.Serializable]
    public class SceneDetails
    {
        public string 主体动作;
        public string 光影;
        public string 氛围;
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

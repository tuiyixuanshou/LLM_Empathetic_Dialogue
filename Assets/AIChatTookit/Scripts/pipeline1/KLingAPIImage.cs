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

public class KLingAPIImage : MonoBehaviour
{
    public quadVideo quadvideo;
    public AvaterBubbleControl bubbleControl;
    public Settings settings;
    private string apiImage = "https://api.klingai.com/v1/images/generations";
    public string jwtToken; // JWT Token，通过前面的代码生成

    public string secretKey;
    public string accessKey;

    private string avatar_imageUrl = "Assets\\AIChatTookit\\Video\\avatar\\小动物静态.png";
    private string ImageBase64;

    private string cur_Task_ID;
    private string cur_Image_URL;
    [Header("KLing模型设置")]
    [TextArea(10,5)]
    public string prompt = $@"请在结合图片主体原有姿势以及背景的前提下，
自动生成一段合理的、幅度较小的小动物挥手动作，动作完成后必须回到第一帧的状态，即尾帧必须和首帧相同。同时镜头固定不动";

    [Header("陪伴动作语言生成模型设置")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    [Header("动作prompt生成模型设置")]
    public ChatModel Maction_Model;
    public LLMURL Maction_url;
    public APIKey Maction_api;



    private string cur_Move = string.Empty;
    private string cur_Chat = string.Empty;

    void Start()
    {
        ImageBase64 = ReadDefaultImage("C:\\Users\\TF\\Desktop\\avatar.txt");
        //StartCoroutine(GenerateImage());
        //StartCoroutine(checkSingleTask());
        //StartCoroutine(checkTaskList());
    }

    /// <summary>
    /// 读取默认文件
    /// </summary>
    /// <param name="filePath">文件地址</param>
    /// <returns>回复文件内容</returns>
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

    public void ButtonTestCheck()
    {
        //StartCoroutine(checkSingleTask());
    }

    IEnumerator checkTaskList()
    {
        yield return new WaitForSeconds(1f); // 防止瞬间请求导致Token失效
        using(UnityWebRequest request = new UnityWebRequest("https://api.klingai.com/v1/images/generations?pageNum=1&pageSize=30", "GET"))
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

    public void MScene_API_Send(string Evaluate)
    {
        //StartCoroutine(ActionProcesse(Evaluate));
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

        yield return GenerateImage();
        yield return new WaitForSeconds(0.5f);
        Mchat_API_CallBack(cur_Chat);
    }

    void Action_Prompt_Set(string text)
    {
        cur_Move = text;
        //prompt = $@"请在结合图片主体原有姿势以及背景的前提下，自动生成一段合理的、幅度较小的{text}，动作完成后必须回到第一帧的状态，即尾帧必须和首帧相同。同时镜头固定不动";
        prompt = $@"请在结合图片主体原有姿势以及背景的前提下，自动生成一段合理的、幅度较小的{text}";
    }

    void chat_CallBack(string respond)
    {
        cur_Chat = respond;
    }

    void Mchat_API_CallBack(string respond)
    {
        bubbleControl.SetUpAvatarBubble(respond);
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

    IEnumerator GenerateImage()
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        yield return new WaitForSeconds(1f); // 防止瞬间请求导致Token失效

        using (UnityWebRequest request = new UnityWebRequest(apiImage, "POST"))
        {
            // 设置请求头
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");

            // 生成 JSON 请求体
            string requestBody = JsonConvert.SerializeObject(new
            {
                model_name = "kling-v1-5",              
                //image = ImageBase64,
                prompt = prompt,
                n = 1,
                aspect_ratio = "1:1"
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
                yield return Auto_CheckCurTask(cur_Task_ID);
            }
        }
    }

    //进行自动查询
    IEnumerator Auto_CheckCurTask(string Take_ID)
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        string new_URL = "https://api.klingai.com/v1/images/generations/" + Take_ID;
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
                    cur_respond = request.downloadHandler.text;
                }
            } 
        }

        if(cur_task_status == "succeed" && cur_respond != string.Empty)
        {
            Debug.Log(cur_respond);
            cur_Image_URL = JsonConvert.DeserializeObject<KlingBackRead>(cur_respond).data.task_result.images[0].url;
            Debug.Log(cur_Image_URL);
            //quadvideo.RespondToM_Action(cur_Image_URL);
        }
        else
        {
            Debug.Log("图片查询时遇到问题！");
        }

       
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
    public class Images
    {
        public string url;
    }

    [Serializable]
    public class TaskResult
    {
        public List<Images> images;
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

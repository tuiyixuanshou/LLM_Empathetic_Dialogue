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
            Debug.Log("以加载图片");
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

  
    public IEnumerator GenerateImage(Action<string> callback)
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        yield return new WaitForSeconds(1f); // 防止瞬间请求导致Token失效

        using (UnityWebRequest request = new UnityWebRequest(apiImage, "POST"))
        {
            // 设置请求头
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            ImageBase64 = ReadDefaultImage("C:\\Users\\TF\\Desktop\\avatar.txt");
            // 生成 JSON 请求体
            string requestBody = JsonConvert.SerializeObject(new
            {
                model_name = "kling-v1-5",              
                //image = ImageBase64,
                prompt = prompt,
                negative_prompt = "模糊不清，抽象，恐怖谷，恐怖",
                image = ImageBase64,
                image_reference = "subject",
                image_fidelity = 0.9f,
                n = 1,
                aspect_ratio = "9:16"
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
                yield return Auto_CheckCurTask(cur_Task_ID,callback);
            }
        }
    }

    //进行自动查询
    IEnumerator Auto_CheckCurTask(string Take_ID,Action<string> callback)
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
            callback(cur_Image_URL);
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
            { "nbf", currentTime - 30 }     // 5秒前生效
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

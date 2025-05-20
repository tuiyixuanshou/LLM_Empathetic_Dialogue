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
    public string jwtToken; // JWT Token��ͨ��ǰ��Ĵ�������

    public string secretKey;
    public string accessKey;

    private string avatar_imageUrl = "Assets\\AIChatTookit\\Video\\avatar\\С���ﾲ̬.png";
    private string ImageBase64;

    private string cur_Task_ID;
    private string cur_Image_URL;
    [Header("KLingģ������")]
    [TextArea(10,5)]
    public string prompt = $@"���ڽ��ͼƬ����ԭ�������Լ�������ǰ���£�
�Զ�����һ�κ���ġ����Ƚ�С��С������ֶ�����������ɺ����ص���һ֡��״̬����β֡�������֡��ͬ��ͬʱ��ͷ�̶�����";

    [Header("��鶯����������ģ������")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    [Header("����prompt����ģ������")]
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
    /// ��ȡĬ���ļ�
    /// </summary>
    /// <param name="filePath">�ļ���ַ</param>
    /// <returns>�ظ��ļ�����</returns>
    private string ReadDefaultImage(string filePath)
    {
        if (File.Exists(filePath))
        {
            Debug.Log("�Լ���ͼƬ");
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

    public void ButtonTestCheck()
    {
        //StartCoroutine(checkSingleTask());
    }

    IEnumerator checkTaskList()
    {
        yield return new WaitForSeconds(1f); // ��ֹ˲��������TokenʧЧ
        using(UnityWebRequest request = new UnityWebRequest("https://api.klingai.com/v1/images/generations?pageNum=1&pageSize=30", "GET"))
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

    public void MScene_API_Send(string Evaluate)
    {
        //StartCoroutine(ActionProcesse(Evaluate));
    }

  
    public IEnumerator GenerateImage(Action<string> callback)
    {
        string jwtToken = EncodeJwtToken(accessKey, secretKey);
        yield return new WaitForSeconds(1f); // ��ֹ˲��������TokenʧЧ

        using (UnityWebRequest request = new UnityWebRequest(apiImage, "POST"))
        {
            // ��������ͷ
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            ImageBase64 = ReadDefaultImage("C:\\Users\\TF\\Desktop\\avatar.txt");
            // ���� JSON ������
            string requestBody = JsonConvert.SerializeObject(new
            {
                model_name = "kling-v1-5",              
                //image = ImageBase64,
                prompt = prompt,
                negative_prompt = "ģ�����壬���󣬿ֲ��ȣ��ֲ�",
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
                yield return Auto_CheckCurTask(cur_Task_ID,callback);
            }
        }
    }

    //�����Զ���ѯ
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
                    //TO DO:���￼��jwtʧЧ�����
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
            Debug.Log("ͼƬ��ѯʱ�������⣡");
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
            { "exp", currentTime + 1800 }, // 30���Ӻ����
            { "nbf", currentTime - 30 }     // 5��ǰ��Ч
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

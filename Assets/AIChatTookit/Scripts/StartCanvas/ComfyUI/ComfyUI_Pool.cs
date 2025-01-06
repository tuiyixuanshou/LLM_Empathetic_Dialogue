using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ComfyUI_Pool : MonoBehaviour
{
    [TextArea(10, 30)]
    public string tempJson;

    [TextArea(10, 30)]
    public string jsonHead;
    private string AllJson;

    public List<JsonData> jsonImageDatas;

    public List<JsonTextData> jsonTextDatas;

    private string url = "http://127.0.0.1:8188/prompt";

    private void Start()
    {
        //JsonConvert.SerializeObject(Data);
        //Debug.Log(ImageJson);
        //AllJson = jsonHead +
        //    "\"34\":" + JsonUtility.ToJson(jsonTextDatas[1]) + "," +  //34:negative Prompt
        //    "\"20\":" + JsonUtility.ToJson(jsonTextDatas[0]) + "," +  //20:positive Prompt
        //    "\"28\":" + JsonUtility.ToJson(jsonImageDatas[2]) + "," + //style image
        //    "\"29\":" + JsonUtility.ToJson(jsonImageDatas[1]) + "," + //style image
        //    "\"1\":" + JsonUtility.ToJson(jsonImageDatas[0]) +"}}";   //1: base image
        //Debug.Log(AllJson);
    }

    public void Delivery_task()
    {
        AllJson = jsonHead +
            "\"34\":" + JsonUtility.ToJson(jsonTextDatas[1]) + "," +  //34:negative Prompt
            "\"20\":" + JsonUtility.ToJson(jsonTextDatas[0]) + "," +  //20:positive Prompt
            "\"28\":" + JsonUtility.ToJson(jsonImageDatas[2]) + "," + //style image
            "\"29\":" + JsonUtility.ToJson(jsonImageDatas[1]) + "," + //style image
            "\"1\":" + JsonUtility.ToJson(jsonImageDatas[0]) + "}}";   //1: base image
        StartCoroutine(PostRequest(url, AllJson));
    }
    
    static IEnumerator PostRequest(string url,string json)
    {
        UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;
        www.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);

        Debug.Log("start ComfyUI");
        yield return www.SendWebRequest();

        if(www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("generate Success!");
        }
        else
        {
            Debug.LogError("Request failed:" + www.error);
        }
    }


    #region 上传图像API
    [Serializable]
    public class JsonData
    {
        public Inputs inputs;       // 对应 JSON 中的 "inputs" 对象
        public string class_type;   // 对应 JSON 中的 "class_type"
        public Meta _meta;          // 对应 JSON 中的 "_meta" 对象
    }
    [Serializable]
    public class Inputs
    {
        public string image;        // 对应 JSON 中的 "image" 字段
        public string upload;       // 对应 JSON 中的 "upload" 字段
    }
    [Serializable]
    public class Meta
    {
        public string title;        // 对应 JSON 中的 "title" 字段
    }
    #endregion

    #region 上传文本API
    [Serializable]
    public class JsonTextData
    {
        public TextInputs inputs;        // 对应 JSON 中的 "inputs"
        public string class_type;    // 对应 JSON 中的 "class_type"
        public TextMeta _meta;           // 对应 JSON 中的 "_meta"
    }

    [Serializable]
    public class TextInputs
    {
        public string text;          // 对应 JSON 中的 "text"
    }

    [Serializable]
    public class TextMeta
    {
        public string title;         // 对应 JSON 中的 "title"
    }
    #endregion
}

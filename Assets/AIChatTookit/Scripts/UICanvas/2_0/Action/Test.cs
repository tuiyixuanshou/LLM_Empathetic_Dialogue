//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

//public class Test : MonoBehaviour
//{
//    public string url;
//    private bool isAIRun;
//    private void SendToChat(string prompt, string pre)
//    {
//        if (string.IsNullOrEmpty(prompt))
//            return;
//        if (!isAIRun)
//        {
//            //直接构造json数据
//            string json = $@"{{
//                ""model"": ""{modelName}"",
//                ""system"": ""{pre}"",
//                ""prompt"": ""{prompt}"",
//                ""stream"": false
//                }}";
//            //StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
//            StartCoroutine(postRequestChat(url , json));
//        }
//    }

//    IEnumerator postRequestChat(string url, string json)
//    {
//        isAIRun = true;
//        var uwr = new UnityWebRequest(url, "POST");
//        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
//        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
//        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
//        uwr.SetRequestHeader("Content-Type", "application/json");

//        //Send the request then wait here until it returns
//        yield return uwr.SendWebRequest();

//        if (uwr.result != UnityWebRequest.Result.Success)
//        {
//            Debug.Log("Error While Sending: " + uwr.error);
//        }
//        else
//        {
//            Debug.Log("Received: " + uwr.downloadHandler.text);
//            _response = uwr.downloadHandler.text;
//            //retrieve response from the JSON
//            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(_response);
//            string responseJson = apiResponse.response;
//            Debug.Log(responseJson);
//        }
//        isAIRun = false;
//    }
//}

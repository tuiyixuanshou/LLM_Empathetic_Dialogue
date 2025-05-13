using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class RAG_TempDialogue : MonoBehaviour
{
    public Settings settings;

    public string embedURL = "http://127.0.0.1:5000/embed";

    public string queryURL = "http://127.0.0.1:5000/query";

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            if (settings.tempDialogue.Count > 20)
            {
                Debug.Log("��ʼ�洢�Ի�Ϊ���ڼ��䡭");
                int index = 0; string value;
                while (index < settings.tempDialogue.Count)
                {
                    var currentDict = settings.tempDialogue[index];
                    currentDict.TryGetValue("role", out value);
                    if (value == "system")
                    {
                        Debug.Log("ѡ��List�е���һ��Dictionary");
                        index++;

                    }
                    else
                    {
                        Debug.Log("���������Ի���");
                        HandleDialogueList(index, currentDict);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("���洢RAG");
            }
        }
    }
    private void HandleDialogueList(int index,Dictionary<string,string> currentDict)
    {
        settings.tempDialogue.RemoveAt(index);
        string Json = JsonConvert.SerializeObject(currentDict);
        //TO DO:�����û�ʱû�й�ϵ�����ж���û�ʱ����Ҫ����û�ʶ��ID���ڲ�����Vector��
        var payload = new
        {
            text = Json,
            name = "user"
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postEmbed(Jsonpayload));
    }

    IEnumerator postEmbed(string Json)
    {
        var Embedrequest = new UnityWebRequest(embedURL, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(Json);
        Embedrequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        Embedrequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        Embedrequest.SetRequestHeader("Content-Type", "application/json");
        yield return Embedrequest.SendWebRequest();
        if (Embedrequest.responseCode == 200)
        {
            Debug.Log("RAG�洢�ɹ���");
        }
        else
        {
            Debug.Log("RAG�洢ʧ�ܣ�");
        }
    }

    public IEnumerator postQuery(string question,Action<string> callback)
    {
        Debug.Log("��ʼ����embedding��ѯ��");
        using(var Queryrequest = new UnityWebRequest(queryURL, "POST"))
        {
            var payload = new
            {
                query = question,
                name = "user"
            };
            string Jsonpayload = JsonConvert.SerializeObject(payload);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(Jsonpayload);
            Queryrequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            Queryrequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            Queryrequest.SetRequestHeader("Content-Type", "application/json");
            yield return Queryrequest.SendWebRequest();
            if(Queryrequest.responseCode == 200)
            {
                callback(Queryrequest.downloadHandler.text);
            }
            else
            {
                Debug.Log("��ѯʧ�ܣ�");
                callback("null");
            }
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

        //Send the request then wait here until it returns
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

    [System.Serializable]
    public class EmbedResond
    {
        public string status;
    }
}

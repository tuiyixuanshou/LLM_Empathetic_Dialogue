using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class Emoji_Control : MonoBehaviour
{
    public string embedURL = "http://127.0.0.1:5000/embed";
    public string queryURL = "http://127.0.0.1:5000/query";
    public List<EmojiAndDesription> Emoji_List;
    public List<EmojiRAG> Emoji_rag;

    private void Start()
    {

    }

    private void HandleDialogueList()
    {
        for(int i = 1;i< Emoji_List.Count; i++)
        {
            string Json = JsonConvert.SerializeObject(Emoji_rag[i]);
            Debug.Log(Json);
            var payload = new
            {
                text = Json,
                name = "assistant"
            };
            string Jsonpayload = JsonConvert.SerializeObject(payload);
            Debug.Log(Jsonpayload);
            StartCoroutine(postEmbed(Jsonpayload));
        }
        
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
            Debug.Log("RAG存储成功！");
        }
        else
        {
            Debug.Log("RAG存储失败！");
        }
    }

    //表情包也可以下载下来，最后可以不用list查询，用名称+路径的方式查询。需要下载图片。
    public IEnumerator Emoji_Rag_Query(string text,Action<Sprite> callback)
    {
        Debug.Log("开始进行emoji查询：");
        using (var Queryrequest = new UnityWebRequest(queryURL, "POST"))
        {
            var payload = new
            {
                query = text,
                name = "assistant"
            };
            string Jsonpayload = JsonConvert.SerializeObject(payload);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(Jsonpayload);
            Queryrequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            Queryrequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            Queryrequest.SetRequestHeader("Content-Type", "application/json");
            yield return Queryrequest.SendWebRequest();
            if (Queryrequest.responseCode == 200)
            {
                EmojiRAG emojiRAG = JsonUtility.FromJson<EmojiRAG>(Queryrequest.downloadHandler.text);
                int index = int.Parse(emojiRAG.Emoji_Index);
                foreach(var item in Emoji_List)
                {
                    if(item.EmojiIndex == index)
                    {
                        callback(item.Emoji_Image);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("查询失败！");
            }
        }
    }

    [Serializable]
    public class EmojiAndDesription
    {
        public int EmojiIndex;
        public Sprite Emoji_Image;
        public string Emoji_description;
    }

    [Serializable]
    public class EmojiRAG
    {
        public string Emoji_Index;
        public string Emoji_description;
    }
}

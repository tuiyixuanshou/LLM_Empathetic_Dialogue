using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static sendData;
using System.IO;

public class EnvironmentSet : MonoBehaviour
{
    public Settings settings;
    public ChangeCanvas changeCanvas;
    public ComfyUI_Pool pool;
    public LLMChat llmChat;
    [Header("UI��ȡ")]
    public Image BackGround;
    public Material BackMaterial;
    public Button ChatEventButton;
    public Text EventContent;
    public string LocalBackGroundPath;

    [Header("comfyUI�ؼ�������ģ������")]
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;

    private string chosenContent;
    private string Pprompt;

    private Texture2D tex;
    private int count;

    private void Start()
    {
        ChatEventButton.onClick.AddListener( delegate { ChatButtonClick(); });
    }

    private void ChatButtonClick()
    {
        StartCoroutine(ChatEnvironmentSet());
    }

    IEnumerator ChatEnvironmentSet()
    {
        yield return changeCanvas.LoadFadeIn();
        chosenContent = EventContent.text;
        Debug.Log("�����ʾ��:"+ chosenContent);

        yield return Post_GetClipText_Silicon(settings.m_SetUrl(url));

        Debug.Log("��ʼ���ɱ���ͼƬ");
        string[] pathes = Directory.GetFiles(LocalBackGroundPath);
        count = pathes.Length;
        StartCoroutine(checkBackGround());
        yield return pool.PostRequest("http://127.0.0.1:8188/prompt", pool.EnvironmentSetBackGround(Pprompt, string.Empty));
        Debug.Log("ͼ��������ϣ���ʼ��������");

    }

    IEnumerator Post_GetClipText_Silicon(string url)
    {
        if (string.IsNullOrEmpty(chosenContent))
            yield break;
        string prompt = $@"����Ҫ��������comfyUI����һ�����컷����ͼƬ��
                           ���ű�����Ҫ�������û�����AI���۵������������ԣ�
                           �������£���{chosenContent}��
                           ������Ҫע����ǣ�����ͼƬ�в���Ҫ�����κ��ˡ������������������󣬽����ɻ������ɣ�
                           ��������һЩ��ʾ�ʣ�ֻ���ؾ�����ʾ�ʼ���,������Ӣ�Ļش�";
        List<Dictionary<string, string>> tempData = new();
        var UserMessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content", prompt }
        };
        tempData.Add(UserMessage);
        var payload = new
        {
            model = settings.m_SetModel(chatModel),
            messages = tempData,
            stream = false,
        };
        string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + settings.m_SetApi(api));

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            string response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
            Pprompt = apiResponse.choices[0].message.content;
            Debug.Log(Pprompt);

            //callback(responseJson);
        }
    }

    IEnumerator checkBackGround()
    {
        while (true)
        {
            string[] pathes = Directory.GetFiles(LocalBackGroundPath);
            if (pathes == null || pathes.Length == 0)
            {
                yield return new WaitForSeconds(2f);
          
            }
            else if (pathes.Length == count)
            {
                yield return new WaitForSeconds(2f);
            }
            else
            {
                byte[] data = File.ReadAllBytes(pathes[pathes.Length - 1]);
                tex = new Texture2D(2, 2);
                tex.LoadImage(data);
                //BackGround.sprite = settings.ConvertToSprite(tex);
                BackMaterial.SetTexture("_MainTex", tex);
                yield return changeCanvas.LoadFadeOut();
                llmChat.StartChat(chosenContent);
                break;
            }
            
        }

    }
}
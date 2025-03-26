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
    [Header("UI获取")]
    public Image BackGround;
    public Material BackMaterial;
    public Button ChatEventButton;
    public Text EventContent;
    public string LocalBackGroundPath;

    [Header("comfyUI关键词生成模型设置")]
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
        Debug.Log("获得提示词:"+ chosenContent);

        yield return Post_GetClipText_Silicon(settings.m_SetUrl(url));

        Debug.Log("开始生成背景图片");
        string[] pathes = Directory.GetFiles(LocalBackGroundPath);
        count = pathes.Length;
        StartCoroutine(checkBackGround());
        yield return pool.PostRequest("http://127.0.0.1:8188/prompt", pool.EnvironmentSetBackGround(Pprompt, string.Empty));
        Debug.Log("图像生成完毕，开始背景布置");

    }

    IEnumerator Post_GetClipText_Silicon(string url)
    {
        if (string.IsNullOrEmpty(chosenContent))
            yield break;
        string prompt = $@"我需要生成利用comfyUI生成一张聊天环境的图片，
                           这张背景需要和以下用户将和AI讨论的事情具有相关性，
                           事请如下：“{chosenContent}”
                           另外需要注意的是，这张图片中不需要包含任何人、动物或其他具体的形象，仅生成环境即可，
                           请你生成一些提示词，只返回具体提示词即可,并且用英文回答。";
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
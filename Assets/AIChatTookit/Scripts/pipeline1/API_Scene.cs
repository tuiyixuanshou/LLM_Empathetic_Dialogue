using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class API_Scene : MonoBehaviour
{
    public quadVideo quad;
    public ChangeCanvas changeCanvas;
    public API_CentralControl api_CentralControl;
    public Settings settings;
    //1.选择一个新场景【暂时不生成新的场景是因为api没有开放能力】
    public M_Scene cur_scene;
    public M_Scene target_scene;

    [Header("配文内容")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    private string PrePrompt;
    private string SceneChangeText;

    private void Start()
    {
        PrePrompt = $@"你现在正在扮演一个疗愈小精灵，你的名字叫做{settings.AIName}，你的是性格是{settings.AICharacter}。";
        //MScene_API_Send("用户当前情绪较为平稳但存在未表达的情感，睡眠问题可能暗示潜在压力");
    }

    public void MScene_API_Send(string evaluateResult)
    {
        StartCoroutine(SceneChangeProcess(evaluateResult));
    }

    IEnumerator SceneChangeProcess(string evaluateResult)
    {
        yield return null; //这一步是利用API选择场景  【后台生成新场景？】
        if (cur_scene == M_Scene.cafe)
        {
            target_scene = M_Scene.InDoor_Sofa;
        }
        else target_scene = M_Scene.cafe;  //这里只是暂时这样！！！！

        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);

        //2.黑屏，切换url视频
        //开始黑屏
        yield return changeCanvas.LoadFadeIn();
        quad.cur_Scene = target_scene;
        yield return GetText(evaluateResult);
        yield return quad.ChangePlayVideo2_URL(true);
        yield return changeCanvas.LoadFadeOut();
        cur_scene = target_scene;
        //展示说的话
        api_CentralControl.api_Chat.Mchat_API_CallBack(SceneChangeText);

        api_CentralControl.isMultiRespondStart = false;
    }


    IEnumerator GetText(string evaluateResult)
    {
        string prompt = $@"你现在已经切换了所在场景，从{cur_scene.ToString()}切换到了{target_scene.ToString()},
请参考用户目前的心理状态：{evaluateResult},生成符合场景切换后主动发起的第一句对话，简洁一些，符合日常对话习惯即可。";
        Debug.Log(prompt);
        List<Dictionary<string, string>> temp = new();
        var preMessage = new Dictionary<string, string>
        {
            {"role", "system" },
            {"content",PrePrompt }
        };
        var newMessage = new Dictionary<string, string>
        {
            {"role", "user" },
            {"content",prompt }
        };
        temp.Add(preMessage);
        temp.Add(newMessage);
        var payload = new
        {
            model = settings.m_SetModel(Mchat_Model),
            messages = temp,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        yield return postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, GetTextCallBack);
    }

    void GetTextCallBack(string text)
    {
        SceneChangeText = text;
    }



    //3.生成新的动作。

    IEnumerator postRequest(string url, string api, string json, Action<string> callback)
    {
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + api);
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
}

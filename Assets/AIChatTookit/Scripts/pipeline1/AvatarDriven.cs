using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static SunoAPIDemo;

public class AvatarDriven : MonoBehaviour
{
    public Settings settings;
    public API_CentralControl api_CentralControl;
    public quadVideo quadvideo;
    //��ʼ����avatarƽ�����������������
    //�����ɹ���
    //�����ɶ�ģ̬����
    //�����ݼ�¼����
    [Header("���ɹ������ݵ�ģ��")]
    public ChatModel Story_Model;
    public LLMURL Story_url;
    public APIKey Story_api;

    string PrePrompt;
    public List<string> preStory;

    //private List<ApiResponse> responses = new();
    private APIRespond respond;

    private void Start()
    {
        PrePrompt = $@"���������ڰ���һ������С���飬������ֽ���{settings.AIName}��������Ը���{settings.AICharacter}��";
        //StoryGeneration();
    }

    public void StoryGeneration()
    {
        //�����ж�
        string PreContend = JsonConvert.SerializeObject(preStory);
        string prompt = $@"���������ڵĳ����ǣ�{settings.Scene_Discribtion}
����������У�avatar������һЩ�����Ļ��
����ͬ�ĳ����У�avatar֮ǰ����������ǣ�{PreContend}����������avatar���µĻ���������йأ�
1.���������ڴ˳���������ʲô���˳����з�����ʲô����2.Ѱ��������ڳ�����������ĳЩ���⣬��Ҫ�����û�����ȣ���
��ֻѡ������һ�ַ�������ݣ�����С����Ļ����,���磺����һ�������Թ��Ŀ��ȣ������˳�������ȫ���úȣ���Щй�������������µ�һ�����ȡ�
ֻ����avatar�������ݡ�";
        Debug.Log(prompt);

        List<Dictionary<string, string>> temp = new();
        var Systemmessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",PrePrompt }
        };
        temp.Add(Systemmessage);
        var newMessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content", prompt}
        };
        temp.Add(newMessage);

        var payload = new
        {
            model = settings.m_SetModel(Story_Model),
            messages = temp,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(Story_url), settings.m_SetApi(Story_api), Jsonpayload, StoryGenerateCallback));
    }

    public void StoryGenerateCallback(string text)
    {
        //this is callback, generate mulit respond prompt
        MultiPromptGeneration(text);
    }

    public void MultiPromptGeneration(string text)
    {
        string prompt = $@"��������һ������ḻ��AIGCprompt��׫д�ߣ���Ҳ�зḻ������ѧ���顣���ڣ�����Ҫ�����ṩ�Ĺ������ݣ����ɶ�ģ̬�ظ���prompt��
�������£�{text}��
����Ҫ�������֡�����������Ļظ������ֻظ���Ҫ�󣺷�������������ݱ����£�AIС�����˵�Ļ�����Ҫ�����ճ��Ի�����ʽ��
�����ظ�Ҫ��˼������������������£�AI Agent�������Ķ�����������Ҫ�����ҷ��Ȳ������ɿ���ʹAIGC׼ȷ������ζ�����promptָ�
����Json����ʽ�ظ�������Json����֮��ʲô����Ҫ�ظ����ο���ʽ���£�[
{{""Chat"":""�ոյ���Ɑ������ĺ��ѺȰ�~"",""Action"":""��ɥ��ҡ��ҡͷ�������˰��֣�������Щ��ɥ�����Ρ�""}}
]";
        Debug.Log(prompt);

        List<Dictionary<string, string>> temp = new();
        var Systemmessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",prompt }
        };
        temp.Add(Systemmessage);
        var payload = new
        {
            model = settings.m_SetModel(Story_Model),
            messages = temp,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(Story_url), settings.m_SetApi(Story_api), Jsonpayload, MultiPromptCallBack));
    }

    public void MultiPromptCallBack(string text)
    {
        Debug.Log("��ȫ����Json����");
        List<APIRespond> apiResponds = ParseJsonSafely(text);
        Debug.Log("����API�ӿ�");
        respond = apiResponds[0];

        StartCoroutine(ShowStoryMulit(respond));
        //�����chat��Ҫ�ڶ�����Ƶ���ɽ���֮�����
        //api_CentralControl.api_Chat.Mchat_API_CallBack(apiResponds[0].Chat);

    }

    IEnumerator ShowStoryMulit(APIRespond responed)
    {
        string prompt = $@"���ڽ��ͼƬ����ԭ�������Լ�������ǰ���£��Զ�����һ�κ����{responed.Action},�����ͱ�����Ҫ�ɰ�����";

        //�������Ƶ����ϵͳ������ �� �� ����ϵͳ�����У��Ƕ�ʱ��
        yield return api_CentralControl.api_Action.GenerateVideo(prompt, WaitForVedioWaitForVedio);
    }

    public void WaitForVedioWaitForVedio(string url)
    {
        StartCoroutine(func(url,respond));
    }
    IEnumerator func(string url, APIRespond responed)
    {
        yield return new WaitUntil(() => !api_CentralControl.isSystemAwake);
        api_CentralControl.isSystemAwake = true;
        quadvideo.RespondToM_Action(url);
        api_CentralControl.api_Chat.Mchat_API_CallBack(responed.Chat);
    }
    public List<APIRespond> ParseJsonSafely(string text)
    {
        string newJson = JsonPatch(text);
        try
        {
            return JsonConvert.DeserializeObject<List<APIRespond>>(text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
            return new List<APIRespond>(); // ���ؿ��б���ֹ����
        }
    }

    string JsonPatch(string rawText)
    {
        string pattern = @"\[.*?\]";
        Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string extractedJson = match.Value;
            Debug.Log("��ȡ�� JSON ���ݣ�" + extractedJson);
            return extractedJson;
        }
        else
        {
            Debug.Log("û���ҵ��������ڵ����ݣ�Json���ݷ�����ȫʧ�ܣ�");
            return null;
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

    #region APIָ��ʵ��
    [System.Serializable]
    public class APIRespond
    {
        public string Chat;
        public string Action;
    }
    #endregion
}

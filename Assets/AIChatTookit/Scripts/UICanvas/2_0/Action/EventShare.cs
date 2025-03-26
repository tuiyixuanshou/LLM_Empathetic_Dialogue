using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;


public class EventShare : MonoBehaviour
{
    public Settings settings;
    //����һ���¼����������
    [Header("����Ȧ����ģ������")]
    //public string Silicon_url = "https://api.siliconflow.cn/v1/chat/completions";
    //private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;

    private string PrePrompt;
    private string SpecifyPrompt;

    [Header("Event UI")]
    public Text text;

    private void Start()
    {
        Debug.Log(settings.AIName);
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);
        PrePrompt = $@"���������ڰ���һ������С���飬
                        ������ֽ���{settings.AIName}��
                        ������Ը���{settings.AICharacter},
                        ��������֮��Ի��У��������ܹ����û��е����֡����˻��������������Ĵ���{DialogueEventjson}";
        SpecifyPrompt = $@"��������ʲô��������û������أ����Ժ��û����������������һ���£������ڸ����������
                          ����������������������50���������������ӣ������������ɽ������ɽ�Ͼ�Ȼ����һЩ���ӣ�������ɰ�������С��ʳι�����ǡ���
                         ������ʱ�����ˣ���û�д�ɡ���ܽ�һ�ҿ��ȵ꣬��������˷ǳ���ζ�Ŀ��ȣ���
                         ";
        //��ʱ��Ϊ���̵Ŀ�ͷ
        SendEvent_Silicon(SpecifyPrompt, PrePrompt);
    }

    private void SendEvent_Silicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;

        List<Dictionary<string, string>> tempData = new();
        var SystemMessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content", pre }
        };
        var UserMessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content", prompt }
        };
        tempData.Add(SystemMessage);
        tempData.Add(UserMessage);
        var payload = new
        {
            //model = "meta-llama/Llama-3.3-70B-Instruct",
            model = settings.m_SetModel(chatModel),
            messages = tempData,
            stream = false,
        };
        string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
        StartCoroutine(postEventRequest_Silicon(settings.m_SetUrl(url), jsonPayload));
    }

    IEnumerator postEventRequest_Silicon(string url, string json)
    {
        Debug.Log("now Start to Generate Silicon Daily Routine");
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
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
            Debug.Log("Silicon Daily Routine: " + uwr.downloadHandler.text);
            //retrieve response from the JSON
            string response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            text.text = responseJson;
        }
    }

}

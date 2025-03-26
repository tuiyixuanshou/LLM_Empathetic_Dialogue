using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using WebGLSupport;
using System.Net.Security;

using System.Text.RegularExpressions;
using UnityEngine.Windows;

public class evaluateTest : MonoBehaviour
{
    public Settings settings;
    public API_CentralControl api_CentralControl;
    public AvaterBubbleControl avaterBubble;

    [Header("���������û�״̬ģ������")]
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;

    public InputField inputField;

    private bool isAIRun;
    private List<Dictionary<string, string>> tempEvaluate = new();

    //�û��Ƿ�ش��˵�������
    private bool isUserAnswer = false;

    //���������Ƿ��Ѿ���ʼ
    private bool isStartEvaluate = false;

    private Coroutine askEvaluateCoroutine; // �洢Э�̾��

    private List<Dictionary<string, string>> TempevaluateContext = new();

    private void Start()
    {
        //TO DO:API Central Control���п���
        //GenerateEvaluate();
    }

    public void GenerateEvaluate()
    {
        if (!isAIRun) //isAIRun�ж��Ǳ�Ҫ�ģ���������һ��ֻ����һ�Ρ�
        {
            isAIRun = true;
            Debug.Log("������������");
            string prompt = $@"�����û�ѯ��3-4�����⣬���������û������������ص�����ȫ��ʹ��JSON���ݵĸ�ʽ���磺
[
    {{""Question"": ""�����ں���""}},
    {{""Question"": ""�����û��ʲô��������е��ر��Ļ򲻿��ģ�""}},
    {{""Question"": ""�������û�ие�ѹ�����ǣ�����У���ʲô��������е�ѹ����""}}
]
�ظ��У��벻Ҫ�����κγ���JSON��������Ķ�����������������֡����֡����ŵȵȡ�����֤�ظ���ʽ��ȷ��лл��";

            var newmessage = new Dictionary<string, string>
            {
                {"role","system" },
                {"content", prompt }
            };
            tempEvaluate.Add(newmessage);
            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = tempEvaluate,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            StartCoroutine(postRequest(settings.m_SetUrl(url), settings.m_SetApi(api), jsonPayload, CallBackEvaluate));
        }
        else
        {
            Debug.Log("Wait Evaluate");
        }
    }

    void CallBackEvaluate(string text)
    {
        //text����
        List<QuestionData> questions = ParseJsonSafely(text);
        if(questions.Count != 0)
        {
            //StartCoroutine(askEvaluate(questions));
            askEvaluateCoroutine = StartCoroutine(askEvaluate(questions));
        }
    }
    public List<QuestionData> ParseJsonSafely(string text)
    {
        string newJson = JsonPatch(text);
        try
        {
            return JsonConvert.DeserializeObject<List<QuestionData>>(text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
            return new List<QuestionData>(); // ���ؿ��б���ֹ����
        }
    }

    IEnumerator askEvaluate(List<QuestionData> questions)
    {
        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);
        isStartEvaluate = true;
        api_CentralControl.isEvaluateStart = true;

        Debug.Log(api_CentralControl.isEvaluateStart);
        TempevaluateContext.Clear();
        foreach (var i in questions)
        {
            var newitem = new Dictionary<string, string>
            {
                {"Question",i.Question },
                {"Answer", ""}
            };
            TempevaluateContext.Add(newitem);
            avaterBubble.SetUpAvatarBubble(i.Question);
            settings.LastRespondTime = DateTime.Now;
            //TO DO������һ���򵥵Ļش𣬲�Ȼ�Եù��ڸɰ�
            yield return new WaitUntil(() => isUserAnswer);
            isUserAnswer = false;
            yield return new WaitForSeconds(1f);

        }
        avaterBubble.UserSendInput();
        askEvaluateCoroutine = null;
        isStartEvaluate = false;
        api_CentralControl.isEvaluateStart = false;
        Debug.Log("��������");
        EvaluateAndFirstSelect();
    }

    //inputField end ����
    public void UserInputFunc()
    {
        //�˴�û���漰��isDialogue start
        //avaterBubble.UserSendInput();
        if (TempevaluateContext.Count != 0 && isStartEvaluate)
        {
            Debug.Log("�û��ش����ݴ洢����������");
            var last = TempevaluateContext[TempevaluateContext.Count - 1];
            if (last.ContainsKey("Answer")) // ȷ��������
            {
                last["Answer"] = inputField.text;
            }
            else
            {
                Debug.Log("���ݴ洢��������");
                //last.Add("Answer", inputField.text);
            }
            settings.LastInputTime = DateTime.Now; 
            inputField.text = string.Empty;
            isUserAnswer = true;
        }
        
    }

    void EvaluateAndFirstSelect()
    {
        string evalutateContext = JsonConvert.SerializeObject(TempevaluateContext);

        //����prompt��һ����evaluate Context���Ҿ���tempDialogueҲ���԰��԰�  

        string prompt = $@"����һ�����������οʦ�����������ṩ���û�һЩ����������/������״̬��������û��Ļش�:{evalutateContext},
�����������ʴ��������û���ʱ������״̬����ѡ��һ�ְ�ο���Ĳ��ԣ����Է�Ϊ�Ĵ��࣬�ֱ��ǣ�
chat�Ի�����������������ο�û���action����������һЩ��������ο�û���sound������ͨ����������ο�û���scene������ͨ�������仯����ο�û���
���ص�����ȫ��ʹ��JSON�������������¸�ʽ���أ�
[
    {{""Evaluate"": ""�û����ڵ����黹������Ը����н���"",""MMChoose"":""chat""}}
],
�����ʽ���ӣ�
```json
[
    {{""Evaluate"": ""�û���ǰ���������ȶ���ȱ���ճ����������ܴ���Ǳ��ѹ����¶���"",""MMChoose"":""chat""}}
]
``` 
�������������У������˳�json��������������ݣ����а����ͷ��```json�ͽ�β����```����Щ�����Ҿ��Բ�Ҫ�ġ�
��ע�⣬Evaluate����������ʴ���û�����������������MMChoose������ѡ��İ�ο���ԣ���ο����ֻ����chat��action��sound��scene�����е�һ�֡�
�ظ��У��벻Ҫ�����κγ���JSON��������Ķ�����������������֡����֡����ŵȵȡ�����֤�ظ���ʽ��ȷ��лл��
";
        var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
        List<Dictionary<string, string>> tempEvaluate = new();
        tempEvaluate.Add(newmessage);
        var payload = new
        {
            model = settings.m_SetModel(chatModel),
            messages = tempEvaluate,
            stream = false,
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(url),settings.m_SetApi(api), Jsonpayload, api_CentralControl.EvaluateResultAccept));
    }

    private void Update()
    {
        //������Ҫ���¿��� ���һ��system��ʾ����tempdialogue�С�
        if (isStartEvaluate)
        {
            if (!avaterBubble.AvatarBubble.activeInHierarchy)  //if�����ܲ��ܺ�isDialogueStart�ҹ���
            {
                //����Я��
                if (askEvaluateCoroutine != null)
                {
                    StopCoroutine(askEvaluateCoroutine);
                    askEvaluateCoroutine = null;
                    isStartEvaluate = false;
                    Debug.Log("ask Evaluate Э����ֹͣ");
                }
            }
        }
    }

    IEnumerator postRequest(string url, string api,string json, Action<string> callback)
    {
        isAIRun = true;
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
        isAIRun = false;
    }

    string JsonPatch(string rawText)
    {
        string pattern = @"\[.*?\]";
        Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string extractedJson = match.Value;
            Debug.Log("��ȡ�� JSON ���ݣ�"+ extractedJson);
            return extractedJson;
        }
        else
        {
            //Console.WriteLine("û���ҵ��������ڵ����ݣ�Json���ݷ�����ȫʧ�ܣ�");
            Debug.Log("û���ҵ��������ڵ����ݣ�Json���ݷ�����ȫʧ�ܣ�");
            return null;
        }
    }



    #region ����ʵ��
    [System.Serializable]
    public class QuestionData
    {
        public string Question;
    }

    [System.Serializable]
    public class QuestionList
    {
        public QuestionData[] questions;
    }
    #endregion
}

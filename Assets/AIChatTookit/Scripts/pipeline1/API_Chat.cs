using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using static SunoAPIDemo;

public class API_Chat : MonoBehaviour
{
    public API_CentralControl api_CentralControl;
    public Settings settings;
    public AvaterBubbleControl bubbleControl;
    [Header("chatģ������")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    public InputField inputField;

    private bool isAIRun;  //һ��һ��
    private string PrePrompt;

    private DateTime LastInputTime;
    private DateTime LastRespondTime;

    private Coroutine CheckDialogueThreshold = null;
    


    private void Start()
    {
        inputField.keyboardType = TouchScreenKeyboardType.Social;
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);
        PrePrompt = $@"���������ڰ���һ������С���飬
                        ������ֽ���{settings.AIName}��
                        ������Ը���{settings.AICharacter},
                        ��������֮��Ի��У��������ܹ����û��е����֡����˻��������������Ĵ���{DialogueEventjson}��
                        �ظ���Ҫ�����������ճ������ģʽ��";
        Mchat_API_Send("�û���ǰ������Ϊƽ�ȵ�����δ������У�˯��������ܰ�ʾǱ��ѹ��");
    }

    public void UserInputSend()
    {
        //������״̬�����ĶԻ��ʴ�
        if (!api_CentralControl.isEvaluateStart)
        {
            api_CentralControl.isDialogueStart = true; //��ʼ�����Ի���״̬
            api_CentralControl.isSystemAwake = true;

            settings.LastInputTime = DateTime.Now; //�����û��������ʱ��
            string text = inputField.text;
            Mchat_API_FreePrompt(text, true, Mchat_Model, Mchat_url, Mchat_api); //���з���
            inputField.text = string.Empty;
            bubbleControl.UserSendInput(); //�رնԷ�����
        }
        else
        {

        }
    }

    public void Mchat_API_Send(string evaluateresult)
    {
        StartCoroutine(Mchat_API_Send_Cor(evaluateresult));
    }

    private string relatedMemory;
    void SetRelatedMemory(string text)
    {
        relatedMemory = text;
    }

    public IEnumerator  Mchat_API_Send_Cor(string evaluateresult)
    {

        //TO DO������Ҫ��Ҫ���ϲ��� �ʹ�ǰ�Ի����� ������Ҫ�޸�prompt
        yield return api_CentralControl.rag.postQuery(evaluateresult, SetRelatedMemory);
        string prompt = $@"����һ��������鹲���С���飬�����û����ڵ�����״������{evaluateresult}��
���Ǵ�ǰ�û���agent�Ի��У������漰�������״������ؼ��䣺{relatedMemory}��
ר����Ϊ����ͨ�������Ի��ķ�ʽ�Ѵﵽ��ο��Ч���������������������û������죬����ƽʱ��̸һ����������Ҫ����";

        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                {"role","system"},
                {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",prompt}
                };

        settings.tempDialogue.Add(usermessage);
        var payload = new
        {
            model = settings.m_SetModel(Mchat_Model),
            messages = settings.tempDialogue,
            stream = false,
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, Mchat_API_CallBack));
    }

    public void Mchat_API_FreePrompt(string m_prompt,bool isUserText,ChatModel model,LLMURL url, APIKey api)
    {
        if (!isAIRun)
        {
            isAIRun = true;string Jsonpayload = string.Empty;
            //�����ط���Ҫ���öԻ�ģ��
            if (!isUserText)
            {
                List<Dictionary<string, string>> temp = new();
                var message = new Dictionary<string, string>
                {
                    {"role","system"},
                    {"content",m_prompt}
                };
                temp.Add(message);
                var payload = new
                {
                    model = settings.m_SetModel(model),
                    messages = temp,
                    stream = false,
                };
                Jsonpayload = JsonConvert.SerializeObject(payload);
            }
            else
            {
                if(settings.tempDialogue.Count  == 0)
                {
                    var message = new Dictionary<string, string>
                    {
                        {"role","system"},
                        {"content",PrePrompt}
                    };
                    settings.tempDialogue.Add(message);
                }
                var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",m_prompt}
                };

                settings.tempDialogue.Add(usermessage);
                var payload = new
                {
                    model = settings.m_SetModel(model),
                    messages = settings.tempDialogue,
                    stream = false,
                };
                Jsonpayload = JsonConvert.SerializeObject(payload);
            }
            StartCoroutine(postRequest(settings.m_SetUrl(url), settings.m_SetApi(api), Jsonpayload, PassiveDialogue_CallBack));
        }
        else
        {
            bubbleControl.SetUpAvatarBubble("�ȵ�Ŷ���һ���˼����һ������~");
        }
    }


    IEnumerator CheckThreshold(Action callback)
    {
        TimeSpan userdifference = TimeSpan.Zero;
        TimeSpan responddifference = TimeSpan.Zero;
        while (true)
        {
            DateTime now = DateTime.Now;
            userdifference = now - settings.LastInputTime;
            responddifference = now - settings.LastRespondTime;
            if(userdifference.TotalSeconds>=120 && responddifference.TotalSeconds >= 120)
            {
                Debug.Log("Dialouge is over,and Respond Gap =" + responddifference.TotalSeconds);
                break;
            }
            Debug.Log("Dialouge is not over��and Respond Gap = " + responddifference.TotalSeconds);
            yield return new WaitForSeconds(41f);
        }
        callback();
    }

    private void FreshCorountine()
    {
        Debug.Log("��⵽�Ի�����");
        StopCoroutine(CheckDialogueThreshold);
        CheckDialogueThreshold = null;
        api_CentralControl.isDialogueStart = false; //�����Ի�����
        api_CentralControl.isSystemAwake = false;
    }

    /// <summary>
    /// �����Ի� �ص�����
    /// </summary>
    /// <param name="respond"></param>
    void PassiveDialogue_CallBack(string respond)
    {
        settings.LastRespondTime = DateTime.Now; //����ϵͳ���ظ�ʱ��

        //�õ��ظ�����ʼ�ж� �Ƿ�Ҫ�����Ի�
        if (CheckDialogueThreshold == null)
        {
            CheckDialogueThreshold = StartCoroutine(CheckThreshold(FreshCorountine));
        }

        //����õĻظ�Ҳ���뵽tempDialogue�У�
        var responseMessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content", respond }
        };
        settings.tempDialogue.Add(responseMessage);
        bubbleControl.SetUpAvatarBubble(respond);

        Debug.Log("This is Passive chat CallBack");
    }

    /// <summary>
    /// ��ģ̬�ӿ� �ص�����
    /// </summary>
    /// <param name="respond"></param>
    public void Mchat_API_CallBack(string respond)
    {
        StartCoroutine(StartMulti(respond));
    }
    IEnumerator StartMulti(string respond)
    {
        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);

        bubbleControl.SetUpAvatarBubble(respond);
        api_CentralControl.isMultiRespondStart = false; //��ģ̬�ظ����������

        settings.LastRespondTime = DateTime.Now; //����ϵͳ���ظ�ʱ��

        //������ʱ��ϵͳ������������������������һ��������Ϊ�Ի���ʼ���м�¼��
        api_CentralControl.isDialogueStart = true;
        //�õ���ģ̬�Ļش𣬿�ʼ�ж� �Ƿ�Ҫ�������ζ�ģ̬�ĶԻ�
        if (CheckDialogueThreshold == null)
        {
            CheckDialogueThreshold = StartCoroutine(CheckThreshold(FreshCorountine));
        }

        //����õĻظ�Ҳ���뵽tempDialogue�У�
        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                 {"role","system"},
                 {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
        {
             {"role","assistant"},
             {"content",respond}
        };
        settings.tempDialogue.Add(usermessage);
        Debug.Log("This is Multi Chat CallBack");
    }

    IEnumerator postRequest(string url, string api, string json, Action<string> callback)
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
            FreshCorountine();
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
}

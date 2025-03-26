using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class AvaterAIDriven : MonoBehaviour
{
    public Settings settings;
    public SqurralInteract squrralInteract;
    public AvaterBubbleControl bubbleControl;
    [Header("λ������ģ������")]
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;

    private List<string> PositionMemory = new();
    private string PosMemory = string.Empty;
    private string CurrentPos = "Middle";

    private List<string> ActionMemory = new();
    private string ActMemory = string.Empty;

    private bool isAIRun;

    private string PrePrompt = $@"����һ�����ӳ����AI����������Ҫ���Ƹó������Ϊ��";

    private List<Dictionary<string, string>> tempDialoguePos = new();
    private List<Dictionary<string, string>> tempDialogueActionandContent = new();

    private float timer; //��ʱ��
    private float timer1;
    private float NextMoveGap = 50f; //��һ��������һ��random��������Ҫ���ǺͶ����Ľ��棬���������Կ���ͬʱ����
    private float NextActionGap = 20f;

    private void Update()
    {
        if (!isAIRun)
        {
            //AvaterBubble����״̬�Ҳ��ڴ�ӡ��
            timer += Time.deltaTime;
            timer1 += Time.deltaTime;
            if(timer1> NextActionGap)
            {
                timer1 = 0;
                Debug.Log("AI����������Ч");
                AIActionAndContentDriven();
            }
            else if (timer > NextMoveGap)
            {
                timer = 0;
                Debug.Log("AI������Ч");
                AIPosDriven();
            }
        }

    }


    public void AIPosDriven()
    {
        if (!isAIRun)
        {
            isAIRun = true;
            string prompt = $@"�����Ǹó�����ǰ���ƶ�λ�ã�{PosMemory}��
                               ���ڣ��������ѡ������ó�����ƶ�λ�ã�������ó���ȥ��ߣ���ظ�Left��������ó���ȥ�ұߣ���ظ�Right��������ó���ȥ�м䣬��ظ�Middle��
                               ����Left��Middle��Right��������ѡһ����ǧ��Ҫ���ض������ݻ��ߴ������ݣ�лл��";
            Debug.Log(prompt);
            //���systemԤ��
            if (tempDialoguePos.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", PrePrompt }
                };
                tempDialoguePos.Add(preMessage);
            }
            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            tempDialoguePos.Add(newmessage);

            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = tempDialoguePos,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload,Formatting.Indented);
            StartCoroutine(postRequest(settings.m_SetUrl(url), jsonPayload, CallBackSetPostion));
        }
        else
        {
            Debug.Log("wait");
        }
    }

    public void AIActionAndContentDriven()
    {
        if (!isAIRun)
        {
            isAIRun = true;
            string prompt = $@"�����Ǹó�����ǰ�Ķ�����{ActMemory}�����Ǹó��ﵱǰ��λ��{CurrentPos}��
                               ���ڣ��������ѡ������ó���Ķ�����
                               ������ó�����Զ����ظ�Jump��������ó�����к�����ظ�Greet��
                               �������ϵĻظ�����ѡһ������Ҫ�ظ������
                               ���ٸ���ѡ��Ķ�����ģ��һ�³�������ԣ��������Կ��Դ��������Գ�������������󣬢�������·����������ѡһ�����Խ��лش𣬵�Ҫ��֤����𸴺Ͷ�������һ����ƥ���ԡ�
                               �������������������Եľ��������
                               ���������Ҫ�ó���Ѱ���׽���������б����硰��ô��˵���ˡ��������������𣿡�
                               ������·�������ó������һ���£����硰�ոտ���һ����Ʈ��ȥ�ˡ�����������������
                               �������JSON��ʽ������������𰸣�
                               �ο���ʽ��{{\""Action\"":\""Jump\"",\""Event\"":\""�ոտ���һ����Ʈ��ȥ��\""}}������� JSON ���ݣ�����Ҫ�κζ�������";
            Debug.Log(prompt);
            //���systemԤ��
            if (tempDialogueActionandContent.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", PrePrompt }
                };
                tempDialogueActionandContent.Add(preMessage);
            }
            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            tempDialogueActionandContent.Add(newmessage);

            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = tempDialogueActionandContent,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            StartCoroutine(postRequest(settings.m_SetUrl(url), jsonPayload, CallBackSetAction));
        }
        else
        {
            Debug.Log("Action and Content Wait");
        }
    }

    void CallBackSetAction(string respond)
    {
        ActionContentDrivenResult DrivenResult = null;
        try
        {
            DrivenResult = JsonUtility.FromJson<ActionContentDrivenResult>(respond);
            ActMemory = m_ListAdd(ActionMemory, DrivenResult.Action);
            squrralInteract.SetAction(DrivenResult.Action);
            bubbleControl.SetUpAvatarBubble(DrivenResult.Event);
        }
        catch (JsonException)
        {
            Debug.Log("AI��������ʧ�ܣ������������");
        }
        
        //ActMemory = m_ListAdd(ActionMemory, respond);
        //squrralInteract.SetAction(respond);
    }

    void CallBackSetPostion(string respond)
    {
        CurrentPos = respond; 
        PosMemory = m_ListAdd(PositionMemory, respond);
        squrralInteract.MoveToSetUpPosition(respond);
    }

    IEnumerator postRequest(string url, string json, Action<string> callback)
    {
        isAIRun = true;
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + settings.m_SetApi(api));

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            string response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            //����õĻظ�Ҳ���뵽tempDialogue�У�
            //var responseMessage = new Dictionary<string, string>
            //{
            //    {"role","assistant" },
            //    {"content", responseJson }
            //};
            //tempDialoguePos.Add(responseMessage);
            callback(responseJson);
        }
        isAIRun = false ;
    }

    string m_ListAdd(List<string> m_List,string text)
    {
        if (m_List.Count >= 5)
        {
            m_List.RemoveAt(0);
        }
        m_List.Add(text);
        string m_OutSting = string.Empty;
        foreach (string s in m_List)
        {
            m_OutSting += s;
            m_OutSting += "->";
        }
        return m_OutSting; 
    }

    [Serializable]
    public class ActionContentDrivenResult
    {
        public string Action;
        public string Event;
    }
}



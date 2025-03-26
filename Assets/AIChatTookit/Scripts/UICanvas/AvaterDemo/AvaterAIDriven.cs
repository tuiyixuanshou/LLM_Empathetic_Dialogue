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
    [Header("位置驱动模型设置")]
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;

    private List<string> PositionMemory = new();
    private string PosMemory = string.Empty;
    private string CurrentPos = "Middle";

    private List<string> ActionMemory = new();
    private string ActMemory = string.Empty;

    private bool isAIRun;

    private string PrePrompt = $@"你是一个电子宠物的AI驱动，你需要控制该宠物的行为。";

    private List<Dictionary<string, string>> tempDialoguePos = new();
    private List<Dictionary<string, string>> tempDialogueActionandContent = new();

    private float timer; //计时器
    private float timer1;
    private float NextMoveGap = 50f; //下一步可以做一个random，但是需要考虑和动作的交叉，动作和语言可以同时发生
    private float NextActionGap = 20f;

    private void Update()
    {
        if (!isAIRun)
        {
            //AvaterBubble存在状态且不在打印中
            timer += Time.deltaTime;
            timer1 += Time.deltaTime;
            if(timer1> NextActionGap)
            {
                timer1 = 0;
                Debug.Log("AI驱动动作起效");
                AIActionAndContentDriven();
            }
            else if (timer > NextMoveGap)
            {
                timer = 0;
                Debug.Log("AI驱动起效");
                AIPosDriven();
            }
        }

    }


    public void AIPosDriven()
    {
        if (!isAIRun)
        {
            isAIRun = true;
            string prompt = $@"以下是该宠物先前的移动位置：{PosMemory}。
                               现在，请你随机选择这个该宠物的移动位置，如果想让宠物去左边，请回复Left。如果想让宠物去右边，请回复Right。如果想让宠物去中间，请回复Middle。
                               请在Left、Middle、Right三者中任选一个，千万不要返回多余内容或者错误内容，谢谢。";
            Debug.Log(prompt);
            //添加system预设
            if (tempDialoguePos.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", PrePrompt }
                };
                tempDialoguePos.Add(preMessage);
            }
            // 构造消息内容
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
            string prompt = $@"以下是该宠物先前的动作：{ActMemory}。这是该宠物当前的位置{CurrentPos}。
                               现在，请你随机选择这个该宠物的动作，
                               如果想让宠物跳远，请回复Jump。如果想让宠物打招呼，请回复Greet。
                               请在以上的回复中任选一个。不要回复错误或
                               请再根据选择的动作，模拟一下宠物的语言，宠物语言可以从两个策略出发，①情感索求，②自身故事分享。你可以任选一个策略进行回答，但要保证这个答复和动作具有一定的匹配性。
                               下面我来解释两个策略的具体情况：
                               情感索求需要让宠物寻求亲近，做出情感表达，例如“怎么不说话了”“可以理理我吗？”
                               自身故事分享可以让宠物分享一件事，例如“刚刚看到一朵云飘过去了”“我正在整理东西”
                               最后请以JSON格式输出以上两个答案，
                               参考格式：{{\""Action\"":\""Jump\"",\""Event\"":\""刚刚看到一朵云飘过去了\""}}。仅输出 JSON 数据，不需要任何额外描述";
            Debug.Log(prompt);
            //添加system预设
            if (tempDialogueActionandContent.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", PrePrompt }
                };
                tempDialogueActionandContent.Add(preMessage);
            }
            // 构造消息内容
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
            Debug.Log("AI驱动解析失败，忽略这次生成");
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
            //将获得的回复也加入到tempDialogue中：
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



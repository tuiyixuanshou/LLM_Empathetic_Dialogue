using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebGLSupport;
using System.IO;


public class sendData : MonoBehaviour
{
    [Header("底部输入")]
    public InputField UserInputField;

    [Header("对话气泡设置")]
    public string CurUserName;
    public string CurAIName;
    public Sprite UserHead;


    //LLM
    public string urlOllama;
    public string modelName;


    private string userInputText;
    private string _response;
    private string preprompt;

    private bool isAIRun = false;

    public Text HeadName;
    public Sprite HeadImage;

    private string savePath;

    private void Start()
    {
        UserInputField.keyboardType = TouchScreenKeyboardType.Social;
        HeadName.text = Settings.Instance.AIName;
        CurAIName = Settings.Instance.AIName;
        CurUserName = Settings.Instance.UserName;
        CreatBubble(CurAIName, "你好，我是"+ CurAIName + ",有什么想要分享的吗？", false, null);
    }
    public void testfunc()
    {
        Debug.Log("调用On End");
    }

    public void CreatBubble(string name,string content,bool isRight,Sprite head)
    {
        Bubble mybubble = new Bubble(name, content, isRight, head);
        BubbleSlider.Instance.bubbles.Add(mybubble);
        BubbleSlider.Instance.CreatBubble(mybubble);
    }
    public void GetTextAndClear()
    {
        userInputText = UserInputField.text;
        Debug.Log(userInputText);
        UserInputField.text = string.Empty;
        CreatBubble(CurUserName, userInputText, true, null);
        SendToChat(userInputText);
    }

    IEnumerator postRequest(string url, string json)
    {
        isAIRun = true;
        HeadName.text = "对方正在输入……";
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            _response = uwr.downloadHandler.text;
            //retrieve response from the JSON
            int pos = _response.IndexOf("response\":");
            //Debug.Log(pos);
            int endpos = _response.Substring(pos + 11).IndexOf("\"");
            //Debug.Log(endpos);
            _response = _response.Substring(pos + 11, endpos);

            Debug.Log(_response);
            CreatBubble(CurAIName, _response, false, null);
        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    private void SendToChat(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            StartCoroutine(postRequest(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + preprompt + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
        }
        else
        {
            CreatBubble(CurAIName, "请稍等，我还在回复上一条信息~", false, null);
        }
        
    }

}

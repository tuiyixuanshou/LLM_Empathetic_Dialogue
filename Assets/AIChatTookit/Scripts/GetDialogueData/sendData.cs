using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebGLSupport;
using System.IO;
using Newtonsoft.Json.Linq;


public class sendData : MonoBehaviour
{
    [Header("�ײ�����")]
    public InputField UserInputField;

    [Header("�Ի���������")]
    public string CurUserName;
    public string CurAIName;
    public Sprite UserHead;


    //LLM
    public string urlOllama;
    public string modelName;


    private string userInputText;
    private string _response;
    private string preprompt = "����ݵĽ�ɫ��һ��������������С���飬�������˻���������Ҫ��������Ļ�����Ч������������ճ�����";
    private string preprompt2 = "���������С��,����Ը����ֹ����������Ǿ����е����ߣ�����һ������С����";

    private bool isAIRun = false;

    public Text HeadName;
    public Sprite HeadImage;

    private string savePath;

    [Header("�����ճ���Ϊ·��")]
    [TextArea(15,20)]
    public string dailyPrompt;

    public class ApiResponse
    {
        public string response;
    }

    public void CreatBubble(string name, string content, bool isRight, Sprite head)
    {
        Bubble mybubble = new Bubble(name, content, isRight, head);
        BubbleSlider.Instance.bubbles.Add(mybubble);
        BubbleSlider.Instance.CreatBubble(mybubble);
    }

    private void Start()
    {
        UserInputField.keyboardType = TouchScreenKeyboardType.Social;
        HeadName.text = Settings.Instance.AIName;
        CurAIName = Settings.Instance.AIName;
        CurUserName = Settings.Instance.UserName;
        CreatBubble(CurAIName, "��ã�����"+ CurAIName + ",��ʲô��Ҫ�������", false, null);

        //�ճ�·����ʼ��
        savePath = Application.dataPath + "/response.json";
        //SendDaily(dailyPrompt, preprompt);
        BubbleSlider.Instance.ReadsavePath();
    }

    void SaveData()
    {
        string jsonData = "{\"userName\":\"JohnDoe\",\"highScore\":100}";
        File.WriteAllText(savePath, jsonData);
        Debug.Log($"�����ѱ��浽: {savePath}");
    }
    public void testfunc()
    {
        Debug.Log("����On End");
    }

    
    public void GetTextAndClear()
    {
        userInputText = UserInputField.text;
        Debug.Log(userInputText);
        UserInputField.text = string.Empty;
        CreatBubble(CurUserName, userInputText, true, null);
        SendToChat(userInputText,preprompt2);
        //SendToChat(dailyPrompt);
        //SendDaily(dailyPrompt);
    }

    IEnumerator postRequestChat(string url, string json)
    {
        isAIRun = true;
        HeadName.text = "�Է��������롭��";
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
            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(_response);
            string responseJson = apiResponse.response;
            Debug.Log(responseJson);
            CreatBubble(CurAIName, responseJson, false, null);
        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    IEnumerator postDailyRequest(string url, string json)
    {
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
            //retrieve response from the JSON ţ�� ֱ�ӽ�����
            _response = uwr.downloadHandler.text;
            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(_response);
            string responseJson = apiResponse.response;
            JArray formattedJson = JArray.Parse(responseJson);
            string formattedJsonString = formattedJson.ToString(Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(savePath, formattedJsonString);

            Debug.Log(responseJson);
            BubbleSlider.Instance.ReadsavePath();
        }
    }

    private void SendToChat(string prompt,string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
        }
        else
        {
            CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, null);
        }
        
    }

    private void SendDaily(string prompt,string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            StartCoroutine(postDailyRequest(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
        }
        else
        {
            CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, null);
        }

    }

}

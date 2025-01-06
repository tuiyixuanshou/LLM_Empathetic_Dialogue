using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebGLSupport;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Specialized;


public class sendData : MonoBehaviour
{
    [Header("�ײ�����")]
    public InputField UserInputField;

    [Header("�Ի����ݺͶ�������")]
    public string CurUserName;
    public string CurAIName;
    public Sprite UserHead;

    public Text HeadName;
    public Sprite HeadImage;
    public Sprite AIHeadImage;

    //LLM
    [Header("����Ollama")]
    public string urlOllama;
    public string modelName;


    private string userInputText;
    private string _response;
    private string preprompt = "����ݵĽ�ɫ��һ��������������С���飬�������˻���������Ҫ��������Ļ�����Ч������������ճ�����";
    private string preprompt2 = "��ס���������С��,����Ը����ֹ����������Ǿ����е����ߣ�����һ������С����";


    private bool isAIRun = false;

    

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

        //���֡�ͼƬ����
        HeadName.text = Settings.Instance.AIName;
        CurAIName = Settings.Instance.AIName;
        CurUserName = Settings.Instance.UserName;

        if(Settings.Instance.Headsprite != null)
        {
            UserHead = Settings.Instance.Headsprite;
        }

        if(Settings.Instance.tex != null)
        {
            AIHeadImage = Settings.Instance.ConvertToSprite(Settings.Instance.tex);
        }

        //��ʼ�ʺ�
        CreatBubble(CurAIName, "��ã�����"+ CurAIName + ",��ʲô��Ҫ�������", false, AIHeadImage);

        //�ճ�·����ʼ��
        savePath = Application.dataPath + "/response.json";

        //���ɳ�ʼ·��
        SendDaily(dailyPrompt, preprompt);

        //BubbleSlider.Instance.ReadsavePath();
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

    //�û�����س���֮�����
    public void GetTextAndClear()
    {
        Debug.Log("GetText");
        userInputText = UserInputField.text;
        Debug.Log(userInputText);
        if(userInputText != "�ܽ�")
        {
            UserInputField.text = string.Empty;
            CreatBubble(CurUserName, userInputText, true, UserHead);


            SendToChatSilicon(userInputText, SiliconPre);
            //SendToChat(userInputText,preprompt2);
            //SendToChat(dailyPrompt);
            //SendDaily(dailyPrompt);
        }
        else if(userInputText == "�ܽ�")
        {
            UserInputField.text = string.Empty;
            CreatBubble(CurUserName, userInputText, true, UserHead);
            string GeneralizePrompt = "���������ܽ�һ������ĶԻ����ݣ����ݶԻ��������ܽ����¼��㣺1.���û��е����֡����˻��������������Ĵ��¡�2.�û����ܵ��Ը��ص㡣3.�����û�����Ĺ�ϵ�����0������ȫ����Ϥ��10���������£��������ֱ�ӵĹ�ϵ����";
            SendToGeneralizeSilicon(GeneralizePrompt);
        }

    }

    /// <summary>
    /// ����Ollama
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <returns></returns>
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
            CreatBubble(CurAIName, responseJson, false, AIHeadImage);
            //CreatBubble(CurAIName, responseJson, false, null);
        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    IEnumerator postDailyRequest(string url, string json)
    {
        Debug.Log("now Start to Generate Daily Routine");
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
            Debug.Log("Daily Routine: " + uwr.downloadHandler.text);
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

    private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
    private string SiliconPre = "���������ڰ���һ������С���飬������ֽ���С�ǣ�������Ը����ֹ����������Ǿ����е����ߡ��������û�����������ݣ�";
    [System.Serializable]
    public class ApiSilicion
    {
        public List<Choice> choices;
    }
    [System.Serializable]
    public class Choice
    {
        public Message message;
    }
    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
    IEnumerator postRequestSiliconChat(string url, string json)
    {
        isAIRun = true;
        HeadName.text = "�Է��������롭��";
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);

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
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            //Debug.Log(apiResponse.choices[0].message.content);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            CreatBubble(CurAIName, responseJson, false, AIHeadImage);
        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    private void SendToChat(string prompt,string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            string json = $@"{{
                ""model"": ""{modelName}"",
                ""system"": ""{pre}"",
                ""prompt"": ""{prompt}"",
                ""stream"": false
                }}";
            //StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
            StartCoroutine(postRequestChat(urlOllama + "api/generate", json));
        }
        else
        {
            CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, null);
        }
        
    }

    private string url = "https://api.siliconflow.cn/v1/chat/completions";

    private List<Dictionary<string,string>> tempDialogue = new();

    private void SendToChatSilicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //string userMessage = "��ð�"; // �û���Ϣ
            //string assistantMessage = "�ٺ�!�ð�!�Ҿ���С�ǣ�*���ִ��к�* �Һܸ��˼������أ����ܸ����ҽ�����ʲô�������ǲ�����Ҫ��һ��ʢ��������أ�*�ڴ���գ��*"; // ������Ϣ

            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", pre+prompt }
            };
            //var messages = new List<Dictionary<string, string>>
            //{
            //        new Dictionary<string, string>
            //    {
            //        { "role", "user" },
            //        { "content", pre+prompt }
            //        //{ "system", pre }
            //    }
            //};
            tempDialogue.Add(newmessage);

            var payload = new
            {
                model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            //StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
            StartCoroutine(postRequestSiliconChat(url, jsonPayload));
        }
        else
        {
            CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, AIHeadImage);
        }

    }
    private void SendToGeneralizeSilicon(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","system" },
                {"content", prompt}
            };

            tempDialogue.Add(newmessage);

            var payload = new
            {
                model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            //StartCoroutine(postRequestChat(urlOllama + "api/generate", "{\"model\": \"" + modelName + "\",\"system\": \"" + pre + "\",\"prompt\": \"" + prompt + "\",\"stream\": false}"));
            StartCoroutine(postRequestSiliconGeneralize(url, jsonPayload));
        }
        else
        {
            CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, AIHeadImage);
        }

    }

    IEnumerator postRequestSiliconGeneralize(string url, string json)
    {
        isAIRun = true;
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);

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
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            //CreatBubble(CurAIName, responseJson, false, null);
        }
        isAIRun = false;
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
            CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, AIHeadImage);
        }

    }

}

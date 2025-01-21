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
using System;


public class sendData : MonoBehaviour
{
    [Header("Settings ����")]
    public Settings settings;
    [Header("�ײ�����")]
    public InputField UserInputField;

    [Header("�Ի����ݺͶ�������")]
    public string CurUserName;
    public string CurAIName;
    public Sprite UserHead;
    public Text HeadName;
    //public Sprite HeadImage;
    public Sprite AIHeadImage;
    [Header("��������")]
    public bool isSpeech;
    public sendSpeech speech;
    [Header("�Ի�ģ��")]
    public AIModel aiModel;


    //LLM
    [Header("����Ollama")]
    public string urlOllama;
    public string modelName;

    [Header("Silicon ����")]
    public string url = "https://api.siliconflow.cn/v1/chat/completions";
    public List<Dictionary<string, string>> tempDialogue = new(); //�洢���жԻ�
    private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
    private string SystemPre;

    //������������
    private string userInputText;
    //AI�ظ����ݳн�
    private string _response;

    //AI������
    private bool isAIRun = false;

    //�ճ���Ϊ����·��
    private string savePath;

    private string saveDialogueEvent;

    [Header("�����ճ���Ϊ·������")]
    public bool isDailyRoutine;
    [TextArea(10,5)]
    public string dailyPrompt;
    [TextArea(10, 5)]
    public string dailyPreprompt = "����ݵĽ�ɫ��һ��������������С���飬�������˻���������Ҫ��������Ļ�����Ч������������ճ�����";

    [TextArea(10, 5)]
    [Header("�¼��ܽ�Prompt")]
    public string GeneralizePrompt = @"���������ܽ�һ������ĶԻ����ݣ����ݶԻ��������ܽ����¼��㣺
                                        1.���û��е����֡����˻��������������Ĵ��¡�
                                        2.�û����ܵ��Ը��ص㡣
                                        3.�����û�����Ĺ�ϵ�����0������ȫ����Ϥ��10���������£��������ֱ�ӵĹ�ϵ����
                                        ���⣬�����ע����ǣ��ظ������ݱ�����Json�ĸ�ʽ��������磺
     {""Dialogueevent"":""�û���������һ�����ο��ԣ����е�����"",
      ""tempcharacter"":""��Щ�Ա�"",
      ""bearing"":""3""}";

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
            AIHeadImage = settings.ConvertToSprite(Settings.Instance.tex);
        }

        //��ʼ�ʺ�
        CreatBubble(CurAIName, "��ã�����"+ CurAIName + ",��ʲô��Ҫ�������", false, AIHeadImage);

        //�ճ�·����ʼ��
        savePath = Application.dataPath + "/response.json";
        saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";

        //��ȡDialogueEvent�еĹ�ͬ����
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);

        //AI�Ը�Ԥ�裺
        SystemPre = $@"���������ڰ���һ������С���飬
                        ������ֽ���{CurAIName}��
                        ������Ը���{settings.AICharacter},
                        ��������֮��Ի��У��������ܹ����û��е����֡����˻��������������Ĵ���{DialogueEventjson}
                        ������������������Ը����ʺ͹�ͬ���䣬�ظ������û��Ļ���";
        //ѡ�����ɳ�ʼ·��ģ��
        //SendDaily(dailyPrompt, dailyPreprompt);
        //SendDailySilicon(dailyPrompt, dailyPreprompt);
        if (isDailyRoutine)
        {
            switch (aiModel)
            {
                case AIModel.Ollama_Local_Llama3_1:
                    SendDaily(dailyPrompt, dailyPreprompt);
                    break;
                case AIModel.Silicon_Llama_3_3_70B:
                    SendDailySilicon(dailyPrompt, dailyPreprompt);
                    break;
                default:
                    SendDailySilicon(dailyPrompt, dailyPreprompt);
                    break;
            }
        }
        

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
    public void GetTextAndClear(string text)
    {
        Debug.Log("GetText");
        if (UserInputField.gameObject.activeSelf)
        {
            userInputText = UserInputField.text;
        }
        else
        {
            userInputText = text;
        }
        Debug.Log(userInputText);
        if (userInputText != "�ܽ�" && userInputText != null)
        {
            UserInputField.text = string.Empty;
            CreatBubble(CurUserName, userInputText, true, UserHead);
            //ģ��ѡ��
            switch (aiModel)
            {
                case AIModel.Ollama_Local_Llama3_1:
                    SendToChat(userInputText, SystemPre);
                    break;
                case AIModel.Silicon_Llama_3_3_70B:
                    SendToChatSilicon(userInputText, SystemPre);
                    break;
                default:
                    SendToChatSilicon(userInputText, SystemPre);
                    break;
            }
            //SendToChat(userInputText,SystemPre);
            //SendToChat(dailyPrompt);
            //SendDaily(dailyPrompt);
        }
        else if(userInputText == "�ܽ�")
        {
            //UserInputField.text = string.Empty;
            //CreatBubble(CurUserName, userInputText, true, UserHead);
            //Debug.Log(GeneralizePrompt);
            //SendToGeneralizeSilicon(GeneralizePrompt);
            StartGenerilize();
        }

    }

    public void StartGenerilize()
    {
        UserInputField.text = string.Empty;
        CreatBubble(CurUserName, userInputText, true, UserHead);
        Debug.Log(GeneralizePrompt);
        SendToGeneralizeSilicon(GeneralizePrompt);
    }

    /// <summary>
    /// ������Ollama���ͶԻ���Ϣ
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendToChat(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //ֱ�ӹ���json����
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

    /// <summary>
    /// ����Ollama����daliy routine
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendDaily(string prompt, string pre)
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
 
    /// <summary>
    /// Silicon Llama3.3 70B�Ի�
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendToChatSilicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //���systemԤ��
           if(tempDialogue.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", pre }
                };
                tempDialogue.Add(preMessage);
            }
            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            //var messages = new List<Dictionary<string, string>>
            //{
            //        new Dictionary<string, string>
            //    {
            //        { "role", "user" },
            //        { "content", pre+prompt }
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
            //����õĻظ�Ҳ���뵽tempDialogue�У�
            var responseMessage = new Dictionary<string, string>
            {
                {"role","assistant" },
                {"content", responseJson }
            };
            tempDialogue.Add(responseMessage);
            CreatBubble(CurAIName, responseJson, false, AIHeadImage);

            if (isSpeech)
            {
                speech.baiduTTS.Speak(responseJson, speech.PlayAudio);
            }

        }
        isAIRun = false;
        HeadName.text = CurAIName;
    }

    /// <summary>
    /// silicon ����Daily routine·��
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="pre"></param>
    private void SendDailySilicon(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
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
                model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = tempData,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            StartCoroutine(postDailySiliconRequest(url, jsonPayload));
        }
        else
        {
            CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, AIHeadImage);
        }
    }

    IEnumerator postDailySiliconRequest(string url, string json)
    {
        Debug.Log("now Start to Generate Silicon Daily Routine");
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
            Debug.Log("Silicon Daily Routine: " + uwr.downloadHandler.text);
            //retrieve response from the JSON ţ�� ֱ�ӽ�����
            _response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            string responseJson = apiResponse.choices[0].message.content;
            JArray formattedJson = JArray.Parse(responseJson);
            string formattedJsonString = formattedJson.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(savePath, formattedJsonString);
            Debug.Log(responseJson);
            BubbleSlider.Instance.ReadsavePath();
        }
    }

    /// <summary>
    /// Silicon�ܽ�Ի�����
    /// </summary>
    /// <param name="prompt"></param>
    private void SendToGeneralizeSilicon(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
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

            GeneralizeAPI generalizeAPI = JsonUtility.FromJson<GeneralizeAPI>(responseJson);
            Debug.Log($"DialogueEvent:{generalizeAPI.Dialogueevent}");
            Debug.Log($"tempcharacter:{generalizeAPI.tempcharacter}");
            Debug.Log($"bearing:{generalizeAPI.bearing}");

            DateTime now = DateTime.Now;
            string formattedDate = now.ToString("yyyy-MM-dd HH:mm:ss");

            var payload = new
            {
                time = formattedDate,
                DialogueEvent = generalizeAPI.Dialogueevent+","
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            File.AppendAllText(saveDialogueEvent, jsonPayload);
            //CreatBubble(CurAIName, responseJson, false, null);
        }
        isAIRun = false;
    }

    public void SendToSiliconPicture(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            var content = new List<Content>
            {
                new Content
                {
                    type = "image_url",
                    image_url = new ImageUrl
                    {
                        url = "https://sf-maas-uat-prod.oss-cn-shanghai.aliyuncs.com/dog.png",
                        detail = "auto"
                    }
                }
            };

            Payload payload = new Payload
            {
                model = "Qwen/QVQ-72B-Preview",
                
                //model = "meta-llama/Llama-3.3-70B-Instruct",
                messages = new List<ImageMessage>
            {
                new ImageMessage
                {
                    role = "user",
                    content = new List<Content>
                    {
                        new Content
                        {
                            type = "image_url",
                            image_url = new ImageUrl
                            {
                                url = "https://sf-maas-uat-prod.oss-cn-shanghai.aliyuncs.com/dog.png",
                                detail = "auto"
                            }
                        }
                    }
                }
            },
                stream = false
            };

            // ��payload�������л�ΪJSON�ַ���
            string json = JsonUtility.ToJson(payload, true);
            //string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            StartCoroutine(postRequestSiliconImage(url, json));
        }
    }
    IEnumerator postRequestSiliconImage(string url, string json)
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
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(_response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
        }
        isAIRun = false;
    }

    #region Ollama�ظ�����ʵ��
    public class ApiResponse
    {
        public string response;
    }
    #endregion

    #region Silicon�ظ�����ʵ��
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
    #endregion

    #region �ܽ����ʵ��
    [System.Serializable]
    public class GeneralizeAPI
    {
        public string Dialogueevent;
        public string tempcharacter;
        public string bearing;
    }
    #endregion
    // ����JSON�������
    // ����JSON�������
    [System.Serializable]
    public class Payload
    {
        public string model;
        public List<ImageMessage> messages;
        public bool stream;
    }

    [System.Serializable]
    public class ImageMessage
    {
        public string role;
        public List<Content> content;
    }

    [System.Serializable]
    public class Content
    {
        public string type;
        public ImageUrl image_url;
    }

    [System.Serializable]
    public class ImageUrl
    {
        public string url;
        public string detail;
    }
}

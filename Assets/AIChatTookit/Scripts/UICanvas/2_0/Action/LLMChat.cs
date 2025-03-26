using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using static sendData;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LLMChat : MonoBehaviour
{
    //��һ���¼����������
    public Settings settings;
    public BubbleControl bubbleControl;
    [Header("User Input����")]
    public InputField UserInputField;
    private string userInputText;
    [Header("����ģ������")]
    public ChatModel chatModel;
    public LLMURL url;
    public APIKey api;
    //[Header("Silicon-LLM����")]
    //public string Silicon_url = "https://api.siliconflow.cn/v1/chat/completions";
    //private string apiKey = "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";

    //[Header("DeepSeek-LLM����")]
    //public string DeepSeek_url = "https://aihub.cuc.edu.cn/console/v1/chat/completions";
    //private string DSapiKey = "sk-84b722fdcc9a41ea93917034c53457c7";



    private string PrePrompt;
    private string SpecifyPrompt;

    private string ShareEvent = "A few days ago in the town's park, I helped a lost kitten find its owner, and everyone smiled happily!";
    //public static List<Dictionary<string, string>> tempDialoguePos = new();
    private bool isAIRun;

    private void Start()
    {
        UserInputField.keyboardType = TouchScreenKeyboardType.Social;
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);
        PrePrompt = $@"���������ڰ���һ������С���飬
                        ������ֽ���{settings.AIName}��
                        ������Ը���{settings.AICharacter},
                        ��������֮��Ի��У��������ܹ����û��е����֡����˻��������������Ĵ���{DialogueEventjson}��
                        ���⣬�����û���������ܾ�����һЩ�¼���
                        �ظ���Ҫ�����������ճ������ģʽ��";

        SpecifyPrompt = $@"�û����������������¼���{ShareEvent}���ǳ�����Ȥ��
                           ���������������°�,��̸���е�һС�������ݼ��ɣ�������100�֡�";
        //StartChat(SpecifyPrompt, PrePrompt);
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
            //��������õ�������
            userInputText = text;
        }
        Debug.Log(userInputText);

        if (userInputText != "�ܽ�" && userInputText != null)
        {
            UserInputField.text = string.Empty;
            PlayerBubble(userInputText);
            RespondChat(userInputText, PrePrompt);
        }
        else if (userInputText == "�ܽ�")
        {
            //StartGenerilize();
        }

    }
    public void StartChat(string eventContent)
    {
        if (!isAIRun)
        {
            string prompt = $@"�û����������������¼���{eventContent}���ǳ�����Ȥ��
                           ���������������°�,��̸���е�һС�������ݼ��ɣ������ճ������ģʽ,������100�֡�";;
            //���systemԤ��
            if (settings.tempDialogue.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", PrePrompt }
                };
                settings.tempDialogue.Add(preMessage);
                Debug.Log("start temp1:" + settings.tempDialogue.Count);
            }
            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            settings.tempDialogue.Add(newmessage);
            Debug.Log("start temp2:" + settings.tempDialogue.Count);

            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = settings.tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            settings.tempDialogue.RemoveAt(settings.tempDialogue.Count - 1);
            StartCoroutine(postRequestSiliconChat(settings.m_SetUrl(url), jsonPayload, AvaterBubble));
        }
        else
        {
            Debug.Log("wait");
            //CreatBubble(CurAIName, "���Եȣ��һ��ڻظ���һ����Ϣ~", false, AIHeadImage);
        }
    }

    private void RespondChat(string prompt, string pre)
    {
        if (string.IsNullOrEmpty(prompt))
            return;
        if (!isAIRun)
        {
            //���systemԤ��
            if (settings.tempDialogue.Count == 0)
            {
                var preMessage = new Dictionary<string, string>
                {
                    {"role","system" },
                    {"content", pre }
                };
                settings.tempDialogue.Add(preMessage);
            }
            // ������Ϣ����
            var newmessage = new Dictionary<string, string>
            {
                {"role","user" },
                {"content", prompt }
            };
            settings.tempDialogue.Add(newmessage);

            var payload = new
            {
                model = settings.m_SetModel(chatModel),
                messages = settings.tempDialogue,
                stream = false,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            //StartCoroutine(postRequestSiliconChat(Silicon_url, jsonPayload, AvaterBubble));
            StartCoroutine(postRequestSiliconChat(settings.m_SetUrl(url), jsonPayload, AvaterBubble));
        }
        else
        {
            Debug.Log("wait respond");
        }
    }

    private void AvaterBubble(string text)
    {
        bubbleControl.SetBubble(true, text);
    }

    private void PlayerBubble(string text)
    {
        bubbleControl.SetBubble(false, text);
    }


    IEnumerator postRequestSiliconChat(string url, string json,Action<string> callback)
    {
        isAIRun = true;
        //HeadName.text = "�Է��������롭��";
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        //uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);
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
            var responseMessage = new Dictionary<string, string>
            {
                {"role","assistant" },
                {"content", responseJson }
            };
            settings.tempDialogue.Add(responseMessage);
            //CreatBubble(CurAIName, responseJson, false, AIHeadImage);

            //if (isSpeech)
            //{
            //    //speech.baiduTTS.Speak(responseJson, speech.PlayAudio);
            //    //speech.openAITTS.Speak(responseJson, speech.PlayAudio);
            //    speech.SpeakFunction(tts, responseJson);
            //}
            callback(responseJson);

        }
        isAIRun = false;
        //HeadName.text = CurAIName;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AzureTTS : MonoBehaviour
{
    [Header("������������")]
    public string Key = "��д��������";
    [Header("�������������")]
    public string serviceRegion = "eastasia";
    [Header("������������")]
    public string language = "zh-CN";

    /// <summary>
    /// �ʶ��Ľ�ɫ
    /// </summary>
    [Header("�ʶ���������")]
    public string voiceName = "zh-CN-XiaomoNeural";
    /// <summary>
    /// ����
    /// </summary>
    [Header("�ʶ�����������")]
    public string style = "chat";//chat  cheerful  angry  excited  sad

    private string url;
    [Header("��������")]
    public AudioSource m_AudioSource;
    public Button button;
    public Text text;

    private void Awake()
    {
        url = string.Format("https://{0}.tts.speech.microsoft.com/cognitiveservices/v1",serviceRegion);
    }

    private void Start()
    {
        button.onClick.AddListener(delegate { Speak(text.text, PlayAudio); });
    }

    /// <summary>
    /// �����ϳɹ�������
    /// </summary>
    /// <param name="text">��Ҫ�ϳɵ��ı�</param>
    /// <param name="callback">�ص����������ź���</param>
    public void Speak(string text, Action<AudioClip, string> callback)
    {
        StartCoroutine(GetSpeech(text, callback));
    }

    public void PlayAudio(AudioClip clip, string text)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
        Debug.Log("��Ƶʱ����" + clip.length);
    }

    private IEnumerator GetSpeech(string text, Action<AudioClip,string> callback)
    {
        string RequestBody = GenerateTextToSpeech(language, voiceName, style, 2, text);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(RequestBody);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerAudioClip(www.uri, AudioType.MPEG);

            www.SetRequestHeader("Ocp-Apim-Subscription-Key", Key);
            www.SetRequestHeader("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
            www.SetRequestHeader("Content-Type", "application/ssml+xml");

            yield return www.SendWebRequest();

            if (www.responseCode == 200)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                callback(audioClip,text);
            }
            else
            {
                Debug.LogError("�����ϳ�ʧ��: " + www.downloadHandler.text);
            }
        }
    }

    public string GenerateTextToSpeech(string lang, string name, string style, int styleDegree, string text)
    {
        string xml = string.Format(@"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis""
            xmlns:mstts=""https://www.w3.org/2001/mstts"" xml:lang=""{0}"">
            <voice name=""{1}"">
                <mstts:express-as style=""{2}"" styledegree=""{3}"">
                    {4}
                </mstts:express-as>
            </voice>
        </speak>", lang, name, style, styleDegree, text);

        return xml;
    }
}

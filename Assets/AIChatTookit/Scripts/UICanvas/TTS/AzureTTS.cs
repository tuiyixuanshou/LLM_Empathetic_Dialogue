using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AzureTTS : MonoBehaviour
{
    [Header("语音服务令牌")]
    public string Key = "填写服务令牌";
    [Header("语音服务地区码")]
    public string serviceRegion = "eastasia";
    [Header("语音服务令牌")]
    public string language = "zh-CN";

    /// <summary>
    /// 朗读的角色
    /// </summary>
    [Header("朗读声音设置")]
    public string voiceName = "zh-CN-XiaomoNeural";
    /// <summary>
    /// 情绪
    /// </summary>
    [Header("朗读的情绪设置")]
    public string style = "chat";//chat  cheerful  angry  excited  sad

    private string url;
    [Header("声音播放")]
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
    /// 语音合成公开方法
    /// </summary>
    /// <param name="text">需要合成的文本</param>
    /// <param name="callback">回调函数，播放函数</param>
    public void Speak(string text, Action<AudioClip, string> callback)
    {
        StartCoroutine(GetSpeech(text, callback));
    }

    public void PlayAudio(AudioClip clip, string text)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
        Debug.Log("音频时长：" + clip.length);
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
                Debug.LogError("语音合成失败: " + www.downloadHandler.text);
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

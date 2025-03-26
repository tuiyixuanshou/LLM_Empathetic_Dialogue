using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OpenAITTS : MonoBehaviour
{
    /// <summary>
    /// �����ϳɵ�api��ַ
    /// </summary>
    //[SerializeField] private string m_PostURL = "https://api.openai.com/v1/audio/speech";
    [SerializeField] private string m_PostURL = "https://xiaoai.plus/v1/audio/speech";
    [SerializeField] private string api_key = "sk-Coh9iogOA7LOIdzMuM35w715CEW7mdXeAoeWF3xeoYpDLLnT";
    [SerializeField] private ModelType m_ModelType = ModelType.tts_1;//ģ��
    [SerializeField] private VoiceType m_Voice = VoiceType.onyx;//����

    [Header("test")]
    public Text text;
    public Button button;
    public AudioSource m_AudioSource;

    public void PlayAudio(AudioClip clip, string text)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
        Debug.Log("��Ƶʱ����" + clip.length);
    }

    private void Start()
    {
        Debug.Log("openAI");
        //button.onClick.AddListener(delegate { Speak(text.text, PlayAudio); });
    }

    public void Speak(string text,Action<AudioClip,string> callback)
    {
        StartCoroutine(GetSpecch(text, callback));
    }
    
    IEnumerator GetSpecch(string text,Action<AudioClip,string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(m_PostURL, new WWWForm()))
        {
            PostData postData = new PostData
            {
                model = m_ModelType.ToString().Replace('_', '-'),
                voice = m_Voice.ToString(),
                input = text
            };

            string jsonText = JsonUtility.ToJson(postData).Trim();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerAudioClip(m_PostURL, AudioType.MPEG);

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {api_key}");

            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                Debug.Log("openAI do this");
                AudioClip audioClip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                callback(audioClip, text);

            }
            else
            {
                Debug.LogError("�����ϳ�ʧ��: " + request.error);
            }
        }


    }

    /// <summary>
    /// ���͵�ʵ��
    /// </summary>
    [Serializable]
    public class PostData
    {
        public string model = string.Empty;//ģ������
        public string voice = string.Empty;//����
        public string input = string.Empty;//�ı�����
    }

    /// <summary>
    /// ģ������
    /// </summary>
    public enum ModelType
    {
        tts_1,
        tts_1_hd
    }
    /// <summary>
    /// ��������
    /// </summary>
    public enum VoiceType
    {
        alloy,
        echo,
        fable,
        onyx,
        nova,
        shimmer
    }
}

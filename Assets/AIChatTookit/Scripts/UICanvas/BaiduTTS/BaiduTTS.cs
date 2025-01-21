using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BaiduTTS : MonoBehaviour
{
    [Header("API Key")]
    public string m_API_Key = string.Empty;
    [Header("Secret Key")]
    public string m_Client_secret = string.Empty;
    /// <summary>
    /// �Ƿ�ӷ�������ȡtoken
    /// </summary>
    [SerializeField]private bool m_GetTokenFromServer = true;
    /// <summary>
    /// tokenֵ
    /// </summary>
    public string m_Token = string.Empty;
    /// <summary>
    /// ��ȡtoken�ĵ�ַ
    /// </summary>
    [SerializeField] private string m_AuthorizeURL = "https://aip.baidubce.com/oauth/2.0/token";
    /// <summary>
    /// �����ϳɵ�api��ַ
    /// </summary>
    [SerializeField] private string m_PostURL = "https://tsn.baidu.com/text2audio";

    [Header("������Ϣ")]
    public PostDataSetting m_Post_Setting;

    #region ��ȡtoken
    private void Awake()
    {
        StartCoroutine(GetToken(GetTokenAction));
    }
    private void GetTokenAction(string token)
    {
        m_Token = token;
        Debug.Log("�ٶ�TTS:���token");
    }

    private IEnumerator GetToken(System.Action<string> callback)
    {
        string _token_url = string.Format(m_AuthorizeURL + "?client_id={0}&client_secret={1}&grant_type=client_credentials", m_API_Key, m_Client_secret);
        using(UnityWebRequest request = new UnityWebRequest(_token_url, "GET"))
        {
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.isDone)
            {
                string msg = request.downloadHandler.text;
                TokenInfo textback = JsonUtility.FromJson<TokenInfo>(msg);
                string token = textback.access_token;
                callback(token);
            }
        }
    }
    #endregion

    [Header("��������")]
    public Text text;
    public Button button;
    public AudioSource m_AudioSource;

    private void Start()
    {
        button.onClick.AddListener(delegate { Speak(text.text, PlayAudio); });
    }
    private void PlayAudio(AudioClip clip,string text)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
        Debug.Log("��Ƶʱ����" + clip.length);
        ////��ʼ�����ʾ���ص��ı�
        //StartTypeWords(_response);
        ////�л���˵������
        //SetAnimator("state", 2);
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

    IEnumerator GetSpeech(string text,Action<AudioClip,string> callback)
    {
        var url = m_PostURL;
        var postParams = new Dictionary<string, string>();
        postParams.Add("tex", text);//��Ҫ�ϳɵ��ı�
        postParams.Add("tok", m_Token);
        postParams.Add("cuid", SystemInfo.deviceUniqueIdentifier);
        postParams.Add("ctp", m_Post_Setting.ctp);
        postParams.Add("lan", m_Post_Setting.lan);
        postParams.Add("spd", m_Post_Setting.spd);
        postParams.Add("pit", m_Post_Setting.pit);
        postParams.Add("vol", m_Post_Setting.vol);
        postParams.Add("per", SetSpeeker(m_Post_Setting.per));
        postParams.Add("aue", m_Post_Setting.aue);

        //ƴ������
        int i = 0;
        foreach(var item in postParams)
        {
            url += i != 0 ? "&" : "?";
            url += item.Key + "=" + item.Value;
            i++;
        }
        Debug.Log(url);
        //�ϳ���Ƶ
        using (UnityWebRequest speech = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return speech.SendWebRequest();
            if(speech.error == null)
            {
                var type = speech.GetResponseHeader("Content-Type");
                if (type.Contains("audio"))
                {
                    var clip = DownloadHandlerAudioClip.GetContent(speech);
                    callback(clip, text);
                }
                else
                {
                    var response = speech.downloadHandler.data;
                    string textback = System.Text.Encoding.UTF8.GetString(response);
                    Debug.LogError(textback);
                }
            }
        }
    }

    #region �ٶ������ϳ�������Ϣ
    /// <summary>
    /// �ٶ������ϳɵ�������Ϣ
    /// </summary>
    [System.Serializable]
    public class PostDataSetting
    {
        /// <summary>
        /// �ͻ�������ѡ��web����д�̶�ֵ1
        /// </summary>
        public string ctp = "1";
        /// <summary>
        /// �̶�ֵzh������ѡ��,Ŀǰֻ����Ӣ�Ļ��ģʽ����д�̶�ֵzh
        /// </summary>
        [Header("�������ã��̶�ֵzh")] public string lan = "zh";
        /// <summary>
        /// ���٣�ȡֵ0-15��Ĭ��Ϊ5������
        /// </summary>
        [Header("���٣�ȡֵ0-15��Ĭ��Ϊ5������")] public string spd = "5";
        /// <summary>
        /// ������ȡֵ0-15��Ĭ��Ϊ5�����
        /// </summary>
        [Header("������ȡֵ0-15��Ĭ��Ϊ5�����")] public string pit = "5";
        /// <summary>
        /// ������ȡֵ0-15��Ĭ��Ϊ5��������ȡֵΪ0ʱΪ������Сֵ������Ϊ������
        /// </summary>
        [Header("������ȡֵ0-15��Ĭ��Ϊ5������")] public string vol = "5";
        /// <summary>
        /// ��������:��С��=1����С��=0������ң��������=3����ѾѾ=4
        /// ��Ʒ����:����ң����Ʒ��=5003����С¹=5118���Ȳ���=106����Сͯ=110����С��=111�����׶�=103����С��=5
        /// </summary>
        [Header("�����ʶ�������")] public SpeekerRole per = SpeekerRole.��С��;
        /// <summary>
        /// 3Ϊmp3��ʽ(Ĭ��)�� 4Ϊpcm-16k��5Ϊpcm-8k��6Ϊwav������ͬpcm-16k��; ע��aue=4����6������ʶ��Ҫ��ĸ�ʽ��
        /// ������Ƶ���ݲ�������ʶ��Ҫ�����Ȼ�˷���������ʶ��Ч������Ӱ�졣
        /// </summary>
        [Header("���÷��ص���Ƶ��ʽ")] public string aue = "6";
    }
    /// <summary>
    /// ��ѡ����
    /// </summary>
    public enum SpeekerRole
    {
        ��С��,
        ��С��,
        ����ң,
        ��ѾѾ,
        JP����ң,
        JP��С¹,
        JP�Ȳ���,
        JP��Сͯ,
        JP��С��,
        JP���׶�,
        JP��С��
    }

    //��������:��С��=1����С��=0������ң��������=3����ѾѾ=4
    /// ��Ʒ����:����ң����Ʒ��=5003����С¹=5118���Ȳ���=106����Сͯ=110����С��=111�����׶�=103����С��=5
    private string SetSpeeker(SpeekerRole _role)
    {
        if (_role == SpeekerRole.��С��) return "1";
        if (_role == SpeekerRole.��С��) return "0";
        if (_role == SpeekerRole.����ң) return "3";
        if (_role == SpeekerRole.��ѾѾ) return "4";
        if (_role == SpeekerRole.JP��С��) return "5";
        if (_role == SpeekerRole.JP����ң) return "5003";
        if (_role == SpeekerRole.JP��С¹) return "5118";
        if (_role == SpeekerRole.JP�Ȳ���) return "106";
        if (_role == SpeekerRole.JP��Сͯ) return "110";
        if (_role == SpeekerRole.JP��С��) return "111";
        if (_role == SpeekerRole.JP���׶�) return "5";

        return "0";//Ĭ��Ϊ��С��
    }
    #endregion

    #region token��ȡʵ��
    [System.Serializable]
    public class TokenInfo
    {
        public string access_token = string.Empty;
    }
    #endregion
}

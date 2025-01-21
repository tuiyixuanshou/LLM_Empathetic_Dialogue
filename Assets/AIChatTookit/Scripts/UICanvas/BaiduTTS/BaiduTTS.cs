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
    /// 是否从服务器获取token
    /// </summary>
    [SerializeField]private bool m_GetTokenFromServer = true;
    /// <summary>
    /// token值
    /// </summary>
    public string m_Token = string.Empty;
    /// <summary>
    /// 获取token的地址
    /// </summary>
    [SerializeField] private string m_AuthorizeURL = "https://aip.baidubce.com/oauth/2.0/token";
    /// <summary>
    /// 语音合成的api地址
    /// </summary>
    [SerializeField] private string m_PostURL = "https://tsn.baidu.com/text2audio";

    [Header("配置信息")]
    public PostDataSetting m_Post_Setting;

    #region 获取token
    private void Awake()
    {
        StartCoroutine(GetToken(GetTokenAction));
    }
    private void GetTokenAction(string token)
    {
        m_Token = token;
        Debug.Log("百度TTS:获得token");
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

    [Header("声音播放")]
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
        Debug.Log("音频时长：" + clip.length);
        ////开始逐个显示返回的文本
        //StartTypeWords(_response);
        ////切换到说话动作
        //SetAnimator("state", 2);
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

    IEnumerator GetSpeech(string text,Action<AudioClip,string> callback)
    {
        var url = m_PostURL;
        var postParams = new Dictionary<string, string>();
        postParams.Add("tex", text);//需要合成的文本
        postParams.Add("tok", m_Token);
        postParams.Add("cuid", SystemInfo.deviceUniqueIdentifier);
        postParams.Add("ctp", m_Post_Setting.ctp);
        postParams.Add("lan", m_Post_Setting.lan);
        postParams.Add("spd", m_Post_Setting.spd);
        postParams.Add("pit", m_Post_Setting.pit);
        postParams.Add("vol", m_Post_Setting.vol);
        postParams.Add("per", SetSpeeker(m_Post_Setting.per));
        postParams.Add("aue", m_Post_Setting.aue);

        //拼接链接
        int i = 0;
        foreach(var item in postParams)
        {
            url += i != 0 ? "&" : "?";
            url += item.Key + "=" + item.Value;
            i++;
        }
        Debug.Log(url);
        //合成音频
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

    #region 百度语音合成配置信息
    /// <summary>
    /// 百度语音合成的配置信息
    /// </summary>
    [System.Serializable]
    public class PostDataSetting
    {
        /// <summary>
        /// 客户端类型选择，web端填写固定值1
        /// </summary>
        public string ctp = "1";
        /// <summary>
        /// 固定值zh。语言选择,目前只有中英文混合模式，填写固定值zh
        /// </summary>
        [Header("语言设置，固定值zh")] public string lan = "zh";
        /// <summary>
        /// 语速，取值0-15，默认为5中语速
        /// </summary>
        [Header("语速，取值0-15，默认为5中语速")] public string spd = "5";
        /// <summary>
        /// 音调，取值0-15，默认为5中语调
        /// </summary>
        [Header("音调，取值0-15，默认为5中语调")] public string pit = "5";
        /// <summary>
        /// 音量，取值0-15，默认为5中音量（取值为0时为音量最小值，并非为无声）
        /// </summary>
        [Header("音量，取值0-15，默认为5中音量")] public string vol = "5";
        /// <summary>
        /// 基础音库:度小宇=1，度小美=0，度逍遥（基础）=3，度丫丫=4
        /// 精品音库:度逍遥（精品）=5003，度小鹿=5118，度博文=106，度小童=110，度小萌=111，度米朵=103，度小娇=5
        /// </summary>
        [Header("设置朗读的声音")] public SpeekerRole per = SpeekerRole.度小美;
        /// <summary>
        /// 3为mp3格式(默认)； 4为pcm-16k；5为pcm-8k；6为wav（内容同pcm-16k）; 注意aue=4或者6是语音识别要求的格式，
        /// 但是音频内容不是语音识别要求的自然人发音，所以识别效果会受影响。
        /// </summary>
        [Header("设置返回的音频格式")] public string aue = "6";
    }
    /// <summary>
    /// 可选声音
    /// </summary>
    public enum SpeekerRole
    {
        度小宇,
        度小美,
        度逍遥,
        度丫丫,
        JP度逍遥,
        JP度小鹿,
        JP度博文,
        JP度小童,
        JP度小萌,
        JP度米朵,
        JP度小娇
    }

    //基础音库:度小宇=1，度小美=0，度逍遥（基础）=3，度丫丫=4
    /// 精品音库:度逍遥（精品）=5003，度小鹿=5118，度博文=106，度小童=110，度小萌=111，度米朵=103，度小娇=5
    private string SetSpeeker(SpeekerRole _role)
    {
        if (_role == SpeekerRole.度小宇) return "1";
        if (_role == SpeekerRole.度小美) return "0";
        if (_role == SpeekerRole.度逍遥) return "3";
        if (_role == SpeekerRole.度丫丫) return "4";
        if (_role == SpeekerRole.JP度小娇) return "5";
        if (_role == SpeekerRole.JP度逍遥) return "5003";
        if (_role == SpeekerRole.JP度小鹿) return "5118";
        if (_role == SpeekerRole.JP度博文) return "106";
        if (_role == SpeekerRole.JP度小童) return "110";
        if (_role == SpeekerRole.JP度小萌) return "111";
        if (_role == SpeekerRole.JP度米朵) return "5";

        return "0";//默认为度小美
    }
    #endregion

    #region token获取实例
    [System.Serializable]
    public class TokenInfo
    {
        public string access_token = string.Empty;
    }
    #endregion
}

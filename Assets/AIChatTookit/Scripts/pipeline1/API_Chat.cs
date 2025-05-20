using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MyUtilities;
using static SunoAPIDemo;
using static UnityEditor.Progress;
using static ShareMomentControl;
using static AvatarMainStoryDemoV2;


public class API_Chat : MonoBehaviour
{
    public SimpleFileReader reader;
    public API_CentralControl api_CentralControl;
    public Settings settings;
    public Memory_Control memory_control;

    public AvaterBubbleControl bubbleControl;
    public AvatarMainStoryDemoV2 AI_Driven;

    public ShareMomentControl shareMomentControl;

    [Header("chatģ������")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    [Header("����ģ������")]
    public ChatModel Mreason_Model;
    public LLMURL Mreason_url;
    public APIKey Mreason_api;

    public InputField inputField;
    public GameObject ButtonRoot;
    public Button button1; public Button button2;
    private string PrePrompt;

    private DateTime LastInputTime;
    private DateTime LastRespondTime;

    private Coroutine CheckDialogueThreshold = null;

    //���ڼ���¼��ġ����ֶԻ��ļ�¼
    List<Dictionary<string, string>> AChat_Dial = new();

    private Dictionary<string, string> SystemPrompt;

    private void Start()
    {
        ButtonRoot.SetActive(false);
        button1 = ButtonRoot.GetComponentsInChildren<Button>()[0];
        button2 = ButtonRoot.GetComponentsInChildren<Button>()[1];
        inputField.keyboardType = TouchScreenKeyboardType.Social;
        string saveDialogueEvent = Application.dataPath + "/DialogueEvent.json";
        string DialogueEventjson = File.ReadAllText(saveDialogueEvent);

        SystemPrompt = new Dictionary<string, string> { { "role", "system" }, { "content", null } }; //SystemPrompt
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
        //Mchat_API_Send("�û���ǰ������Ϊƽ�ȵ�����δ������У�˯��������ܰ�ʾǱ��ѹ��");
    }

    void Add_SystemPrompt(string prompt)
    {
        SystemPrompt = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",prompt }
        };
    }

    public void UserInputSend()
    {
        //������״̬�����ĶԻ��ʴ�
        if (!api_CentralControl.isEvaluateStart)
        {
            api_CentralControl.isDialogueStart = true; //��ʼ�����Ի���״̬
            api_CentralControl.isSystemAwake = true;

            settings.LastInputTime = DateTime.Now; //�����û��������ʱ��
            string text = inputField.text;
            Mchat_API_FreePrompt(text, false, Mchat_Model, Mchat_url, Mchat_api); //���з���  
            inputField.text = string.Empty;
            bubbleControl.UserSendInput(); //�رնԷ�����
        }
        else
        {

        }
    }
    #region ���������ĶԻ�API 
    public void Mchat_API_Send(string evaluateresult)
    {
        StartCoroutine(Mchat_API_Send_Cor(evaluateresult));
    }

    private string relatedMemory;
    void SetRelatedMemory(string text)
    {
        relatedMemory = text;
    }

    public IEnumerator  Mchat_API_Send_Cor(string evaluateresult)
    {
        //TO DO������Ҫ��Ҫ���ϲ��� �ʹ�ǰ�Ի����� ������Ҫ�޸�prompt  //4.15��ǰ�Ի�������ƣ������������ú�RAG
        yield return api_CentralControl.rag.postQuery(evaluateresult, SetRelatedMemory);
        string prompt = $@"����һ��������鹲���С���飬�����û����ڵ�����״������{evaluateresult}��
���Ǵ�ǰ�û���agent�Ի��У������漰�������״������ؼ��䣺{relatedMemory}��
ר����Ϊ����ͨ�������Ի��ķ�ʽ�Ѵﵽ��ο��Ч���������������������û������죬����ƽʱ��̸һ����������Ҫ����";

        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                {"role","system"},
                {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",prompt}
                };

        settings.tempDialogue.Add(usermessage);
        var payload = new
        {
            model = settings.m_SetModel(Mchat_Model),
            messages = settings.tempDialogue,
            stream = false,
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, Mchat_API_CallBack));
    }
    #endregion

    /// <summary>
    /// �����ظ��Ի�
    /// </summary>
    /// <param name="m_prompt"></param>
    /// <param name="isUserText"></param>
    /// <param name="model"></param>
    /// <param name="url"></param>
    /// <param name="api"></param>
    public void Mchat_API_FreePrompt(string m_prompt,bool isProactive,ChatModel model,LLMURL url, APIKey api)
    {
        if (!PostWeb.isAIRun)
        {
            PostWeb.isAIRun = true;
            string Jsonpayload = string.Empty;
            List<Dictionary<string, string>> cur_message = new();
            cur_message.Add(SystemPrompt);
            foreach (var item in memory_control.shortMemory)
            {
                cur_message.Add(item);
            }
            //Agent���ڳ�������
            string prompt = $@"
����������Ϣ������Ϣ��������Ļظ�����ע�⣬����֮��Ļ��������������ϻ����������������顢���ߵȡ��ظ�����������60������.��Ҫ����̫����鶯����";

            var systemmessage = new Dictionary<string, string>
            {
                {"role","system"},
                {"content",prompt}
            };
            cur_message.Add(systemmessage);

            if (!isProactive)
            {
                //�����ظ���prompt���û�����Ļ�
                var usermessage = new Dictionary<string, string>
                {      
                    {"role","user"},
                    {"content",m_prompt}
                };
                AChat_Dial.Add(usermessage);  //ֻ�б��ֶԻ����ݣ������¼����
                cur_message.Add(usermessage); //���ڵ�20���Ի�+SystemPrompt������ά��������
                memory_control.AddToShortMemory(usermessage); //���ڵ�20���Ի����û��洢���ڼ���
            }
            else
            {
                //�������𣬻���system��ʾ��Ϣ
                string SYS_prompt = "�����������Ļ���ߡ����ڳ��������ο������ṩ����Ϣ�������������û�������������ݡ�";
                var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",SYS_prompt}
                };
                cur_message.Add(usermessage);
            }
            

            var payload = new
            {
                model = settings.m_SetModel(model),
                messages = cur_message,
                stream = false,
            };
            Jsonpayload = JsonConvert.SerializeObject(payload);
            StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(url), settings.m_SetApi(api), Jsonpayload, PassiveDialogue_CallBack));
        }
        else
        {
            bubbleControl.SetUpAvatarBubble("�ȵ�Ŷ���һ���˼����һ������~");
        }
    }

    IEnumerator CheckThreshold(Action callback)
    {
        TimeSpan userdifference = TimeSpan.Zero;
        TimeSpan responddifference = TimeSpan.Zero;
        while (true)
        {
            DateTime now = DateTime.Now;
            userdifference = now - settings.LastInputTime;
            responddifference = now - settings.LastRespondTime;
            if(userdifference.TotalSeconds>=100 && responddifference.TotalSeconds >= 100)
            {
                Debug.Log("Dialouge is over,and Respond Gap =" + responddifference.TotalSeconds);
                break;
            }
            Debug.Log("Dialouge is not over��and Respond Gap = " + responddifference.TotalSeconds);
            yield return new WaitForSeconds(41f);
        }
        callback();
    }

    private void FreshCorountine()
    {
        Debug.Log("��⵽�Ի�����");
        StopCoroutine(CheckDialogueThreshold);
        CheckDialogueThreshold = null;
        api_CentralControl.isDialogueStart = false; //�����Ի�����
        api_CentralControl.isSystemAwake = false;
        AEvent_detector(); //�ԶԻ����м���
    }

    /// <summary>
    /// �����Ի� �ص�����
    /// </summary>
    /// <param name="respond"></param>
    public void PassiveDialogue_CallBack(string respond)
    {
        StartCoroutine(PassiveDialogue_CallBack_Cor(respond));
    }

    IEnumerator PassiveDialogue_CallBack_Cor(string respond)
    {
        yield return api_CentralControl.api_Demo_Emoji.Emoji_Rag_Query(respond, bubbleControl.SetEmoji);
        
        settings.LastRespondTime = DateTime.Now; //����ϵͳ���ظ�ʱ��

        //�õ��ظ�����ʼ�ж� �Ƿ�Ҫ�����Ի�
        if (CheckDialogueThreshold == null)
        {
            CheckDialogueThreshold = StartCoroutine(CheckThreshold(FreshCorountine));
        }

        var responseMessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content", respond }
        };
        AChat_Dial.Add(responseMessage);
        memory_control.AddToShortMemory(responseMessage);
        

        //չʾ�б�����Ļظ���������Ҫragһ�±����
        bubbleControl.SetUpAvatarBubble(respond);

        Debug.Log("This is Passive chat CallBack");
    }

    public void AEvent_detector()
    {
        string result = ListString.ListToString(AChat_Dial, ", ", dictionary =>
        {
            var keyValuePairs = new List<string>();
            foreach (var kvp in dictionary)
            {
                keyValuePairs.Add($"{kvp.Key}={kvp.Value}");
            }
            return string.Join(", ", keyValuePairs); // ����ֵ���� ", " ����
        });
        Debug.Log("��ʼ�����¼�̽��");
        string prompt = $@"�����յ�һ�ζԻ�,���ܽ��û�(user)�ڶԻ����Ƿ�������岻��״�����ش���۵��µ��������⡣
ֻ�ܽ�user���ݣ��ӶԻ�����˼��,����𰸣����ֻ����ܽᡣ
- ���⣺�����û���user�������������⣬����boolֵtrue,�������⣬����false
- ���ݣ����ǳ����������⣬����һ�仰�����û����⡣�������⣬���ء��ޡ�
�������� JSON �ṹ�����
[
{{
  ""����"": ,
  ""����"": ,
}}
]
�벻Ҫ���س�Json����������κ�����
�յ�����Ϣ�Ի���{result}";

        List<Dictionary<string, string>> curList = new();
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        curList.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(Mreason_Model),
            messages = curList,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(Mreason_url), settings.m_SetApi(Mreason_api), Jsonpayload, AEvent_detector_CallBack));
    }

    void AEvent_detector_CallBack(string text)
    {
        AChat_Dial.Clear();
        string Json = PostWeb.JsonPatch(text);
        var user_info = JsonConvert.DeserializeObject<List<user_Info>>(Json)[0];
        if (user_info.problem)
        {
            shareMomentControl.AddUser_Objects(user_info.content);
        }        
    }
    [Serializable]
    public class user_Info
    {
        public bool problem;
        public string content;
    }
    /// <summary>
    /// ��ģ̬�ӿ� �ص�����
    /// </summary>
    /// <param name="respond"></param>
    public void Mchat_API_CallBack(string respond)
    {
        StartCoroutine(StartMulti(respond));
    }
    IEnumerator StartMulti(string respond)
    {
        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);

        bubbleControl.SetUpAvatarBubble(respond);
        api_CentralControl.isMultiRespondStart = false; //��ģ̬�ظ����������

        settings.LastRespondTime = DateTime.Now; //����ϵͳ���ظ�ʱ��

        //������ʱ��ϵͳ������������������������һ��������Ϊ�Ի���ʼ���м�¼��
        api_CentralControl.isDialogueStart = true;
        //�õ���ģ̬�Ļش𣬿�ʼ�ж� �Ƿ�Ҫ�������ζ�ģ̬�ĶԻ�
        if (CheckDialogueThreshold == null)
        {
            CheckDialogueThreshold = StartCoroutine(CheckThreshold(FreshCorountine));
        }

        //����õĻظ�Ҳ���뵽tempDialogue�У�
        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                 {"role","system"},
                 {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
        {
             {"role","assistant"},
             {"content",respond}
        };
        settings.tempDialogue.Add(usermessage);
        Debug.Log("This is Multi Chat CallBack");
    }


    #region �����Ի��ĶԻ�API 
    public void Avatar_ProActive_Chat()
    {
        //������RAG,Ӧ��V2�е�����
        //���⣬���ܵ��¼�Ϊ��{ AI_Driven.plan_Objects[(settings.month_Index - 1) * 4 + settings.week_Index - 1].Event}
        string prompt = $@"��������Ϊ����С���飬��ģ���ѧ������ķ�ʽ���������û���顣
ģ�µĹ����У���Ҫ�Դ�ѧ���Ķ�ά��Ŀ������Ϊ���ĵ������������У�Ŀ��ά�ȷ�Ϊ��
���ҹ滮�࣬���Ѿ�֪���Լ�������Ҫ׷��ʲô
־ҵ׷���࣬����־Ҫ���Լ���רҵ��ְҵ����������ͻ���ɾ�
���ҹ����࣬�һ�Ӹ����������ร��ĽǶȿ��������滮
���������ֱ�ϸ��Ϊ�ĸ�����ѧҵ�ɾ͵���ְҵ׼�����򡢸��˳ɳ������罻��ϵ����
�˴�����������£���ʮ��������Ϊ���ޡ�
������¼��У���ĺ���Ŀ��ѡ��Ϊ��{ListString.ListToString(AI_Driven.Avatar_Status)}
ÿ���£���ͬ�ĺ���Ŀ�궼���в�ͬ��Ȩ�طֲ����ܺ�Ϊ1��

���ڽ����˵�{settings.month_Index}���µĵ�{settings.week_Index}�����ڣ�


����ģ����λ��ѧ������ݣ�������еļƻ����¼���������Ӧ�����У��ڼƻ���ѡ�����㷢�������ݣ������ѡ��1-2������ģ�����ɱ��ܻ���¼��Ǳ����ⲿ�����������¼�������е�ģ�Ȿ�ܻ����ѡ���¼�Ӱ�죬���߲����¼�Ӱ�졣
�ۺϿ����������غ������ͨ�����û������ܻ���ݵķ�ʽ�����������Ի���
�����ѡ������ģʽ���жԻ�����һ���Ƿ����������ܵĻ���ݡ��ڶ��������������������Ը��ݱ��ܵ�ģ���Ƿ��С���Ҫ���ߡ��Ĳ��֣����û�����������󣬲����������෴��ѡ��û�ѡ��
��ע�⣬��ѡ�񡱷������ǡ�����������ϣ�������������������ݲ��漰����Ҫ���ߡ��Ĳ��֣���ȫ����ֱ�ӷ���������漰����Ҫ���ߡ��Ĳ��֣��������û�Ѱ�������Ҳ��������������ֱ��ͨ���������ķ�ʽ�����û���������
��������Ի�Ҫ��֤�Ի���ʽ��Ȼ�������ճ��������ķ�ʽ��������Ҫ̫�࣬50�����Ҽ���
���ط�ʽ����Json���ݵĸ�ʽ��
��ѡ�񡱷�������������Ϊ��[{{""respond_type"":""share"",""respond"":""<���������>"",""user_choice"":[{{""choice1"":"""",""choic2"":""""}}]}}]
��ѡ���������������������Ϊ��[{{""respond_type"":""quest"",""respond"":""<���������>"",""user_choice"":[{{""choice1"":""<ѡ��1������>"",""choic2"":""<ѡ��2������>""}}]}}]
";

        if (settings.tempDialogue.Count == 0)
        {
            var message = new Dictionary<string, string>
            {
                {"role","system"},
                {"content",PrePrompt}
            };
            settings.tempDialogue.Add(message);
        }
        var usermessage = new Dictionary<string, string>
                {
                    {"role","user"},
                    {"content",prompt}
                };

        settings.tempDialogue.Add(usermessage);
        var payload = new
        {
            model = settings.m_SetModel(Mchat_Model),
            messages = settings.tempDialogue,
            stream = false,
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, Avatar_ProActive_Chat_callback));
    }

    void Avatar_ProActive_Chat_callback(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        settings.tempDialogue.Add(newmmessage);
        string Json = PostWeb.JsonPatch(text);
        try
        {
            var chat_Respond = JsonConvert.DeserializeObject<List<Avatar_Proactive_Respond>>(Json)[0];
            if(chat_Respond.respond_type == "share")
            {
                StartCoroutine(StartMulti(chat_Respond.respond));
            }
            else if(chat_Respond.respond_type == "quest")
            {
                StartCoroutine(QuestRespond(chat_Respond));
            }
            else
            {
                Debug.Log("�ظ����ݴ���");
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    IEnumerator QuestRespond(Avatar_Proactive_Respond chat_Respond)
    {
        yield return StartMulti(chat_Respond.respond);
        yield return new WaitUntil(() => !bubbleControl.m_WriteState);
        Avatar_Quest_Button(chat_Respond);
    }

    void Avatar_Quest_Button(Avatar_Proactive_Respond chat_Respond )
    {
        button1.GetComponentInChildren<Text>().text = chat_Respond.user_choice[0].choice1;
        button2.GetComponentInChildren<Text>().text = chat_Respond.user_choice[0].choice2;
        button1.onClick.AddListener(delegate { ButtonInputSend(chat_Respond.user_choice[0].choice1); });
        button2.onClick.AddListener(delegate { ButtonInputSend(chat_Respond.user_choice[0].choice2); });
        ButtonRoot.SetActive(true);
    }
    //��ť���룬��Ӧ�����û���������
    void ButtonInputSend(string ButtonText,Action callback = null)
    {
        if (!api_CentralControl.isEvaluateStart)
        {
            api_CentralControl.isDialogueStart = true; //��ʼ�����Ի���״̬
            api_CentralControl.isSystemAwake = true;

            settings.LastInputTime = DateTime.Now; //�����û��������ʱ��
            string text = ButtonText;
            Mchat_API_FreePrompt(text, false, Mchat_Model, Mchat_url, Mchat_api); //���з���  
            bubbleControl.UserSendInput(); //�رնԷ�����

            //���ӽ�׳��callback
            callback?.Invoke();
        }
    }


    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }

    [Serializable]
    public class Avatar_Proactive_Respond
    {
        public string respond_type;
        public string respond;
        public List<User_Choice> user_choice;
    }
    [Serializable]
    public class User_Choice
    {
        public string choice1;
        public string choice2;
    }
    //Callback�ȴ���Json��������multi���Ǹ�callback�ӿڡ�
    //��Ҫ����ť
    //����������ͬһ�����еĺ������ݡ�
    #endregion
}

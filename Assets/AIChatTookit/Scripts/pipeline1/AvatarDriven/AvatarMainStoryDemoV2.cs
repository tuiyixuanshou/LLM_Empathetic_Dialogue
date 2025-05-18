using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MyUtilities;
using static AvatarMainStoryDemo;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

public class AvatarMainStoryDemoV2 : MonoBehaviour
{
    public ShareMomentControl shareMomentControl;
    public Settings settings;
    public SimpleFileReader reader;
    public API_Chat api_chat;

    [Header("Agent������ά��")]
    List<Dictionary<string, string>> APlan_Dial= new();
    List<Dictionary<string, string>> AEvent_Dial= new();  //�ⲿ�¼����ɡ���̬����
    List<Dictionary<string, string>> AUnexpect_Dial = new();  //ͻ���¼��ɷ�

    //���ֱ�Ӽ���Setting�е�tempDialogue��tempDialogue��RAG����ʹ��
    List<Dictionary<string, string>> ARespond_Dial = new();

    [Header("����ģ��")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;

    [Header("Avatar�켣������������")]
    [TextArea(5,5)]
    public string Event_Pool;
    public int MomentIndex;
    public List<Event_Object> event_Objects ;
    public List<Event_Object> Unexpect_Objects;
    public List<Target_Object> Avatar_Target;
    public List<Target_WeightObject> Avatar_TargetWeight;

    public List<Week_Plan> CurMon_Plan;

    private void Start()
    {
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
    }

    /// <summary>
    /// �첽����system prompt֮��ʼ���������߼ƻ�
    /// </summary>
    /// <param name="prompt"></param>
    void Add_SystemPrompt(string prompt)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","system" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        AEvent_Dial.Add(newmmessage);
        AUnexpect_Dial.Add(newmmessage);
        APlan_Target();
        //AEvent_Generation();
    }

    public void UpdateControl()
    {
        //settings �� monthindex��week index�Ŀ��ƣ�˳����мƻ��������ݡ�
        //��ʼʱ���ܼƻ����¡����¼����Ÿ��¡�Ȩ�ظ��¡���С��λ���ɸ���+��Ӧ�Ը���
        //������+1����С��λ���ɸ���+��Ӧ�Ը���
        //������+1����Ӧ�Ը���
        if (settings.month_Index > 3) 
        { 
            Debug.LogError("ֹͣ��������");
        }
        if (settings.week_Index< 4)
        {
            settings.week_Index++;
            api_chat.Avatar_ProActive_Chat();
        }
        else
        {
            settings.month_Index++;
            settings.week_Index = 1;
            APlan_SpecifyPlan();
        }
    }

    public void APlan_Target()
    {
        Debug.Log("��ʼ��Ŀ������");
        string prompt = $@"ģ��һ����ѧ�������ɴ�ѧ���Ķ�ά�Ⱥ���Ŀ�ꡣ����Ŀ��ά�ȷ�Ϊ��
���ҹ滮�࣬���Ѿ�֪���Լ�������Ҫ׷��ʲô
־ҵ׷���࣬����־Ҫ���Լ���רҵ��ְҵ����������ͻ���ɾ�
���ҹ����࣬�һ�Ӹ����������ร��ĽǶȿ��������滮
�������ֱ�ϸ��Ϊ�ĸ�����ѧҵ�ɾ͡�ְҵ׼�������˳ɳ����罻��ϵ��
���ۺ����ϵ���Ϣ�������ĸ�����ľ���Ŀ��׷��
ע�⣺Ŀ��׷��Ӧ����һ�����塢�賤��Ŭ�������ϴ�ѧ��ʵ��������¼���ֻ��Ҫ����һ��Ŀ�꼴�ɣ�������ˡ�
��Json�ĸ�ʽ�ظ���[
{{
""ѧҵ�ɾ�"":""Ŀ������""��
""ְҵ׼��"":""Ŀ������""��
""���˳ɳ�"":""Ŀ������""��
""�罻��ϵ"":""Ŀ������""
}}
]
�벻Ҫ���س�Json����������κ�����";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = APlan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, APlan_Target_CallBack));
    }

    void APlan_Target_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        APlan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("����Json");
            Avatar_Target = JsonConvert.DeserializeObject<List<Target_Object>>(Json);
            //Debug.Log(Avatar_Target[0].ְҵ׼������);
            APlan_SpecifyPlan();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }
    }


    public void AEvent_Generation()
    {
        Debug.Log("��ʼ�ⲿ�¼�����");
        string prompt = $@"��Ϊ�¼�����ϵͳ������7���͹��¼�����Щʱ����System prompt��ģ�µĴ�ѧ�����ܻ������ġ�
�¼�����ʱ��������system prompt���ṩ����ʵʱ��Ϊ׼��7���¼���ʱ��˳�����С�
���԰�����У���¼��������¼�������¼�
У���¼����ڴ�ѧ�л��������¼��������ѧУ����֯����
�����¼����罻Ⱥ���лᷢ�����¼�������顢�ƻ����ε� 
����¼����������¼� 
����Ҫ�������ﷴӦ��ֻ��Ҫ���ɿ͹��¼����ɡ�
�����ʽΪJson��ʽ����˳���������
[
    {{""event_index"":1,""Event"":""<�����¼�����>""}},
    {{""event_index"":2,""Event"":""<�����¼�����>""}},
]
�벻Ҫ���س�Json����������κ�����";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        AEvent_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = AEvent_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, AEvent_Generation_CallBack));
    }

    void AEvent_Generation_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        AEvent_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        event_Objects = JsonConvert.DeserializeObject<List<Event_Object>>(Json);

        //�ռ������¼�������Agent-Plan����ָ��
        //APlan_TargetWeigh();
    }

    public void APlan_TargetWeigh()
    {
        //�����һ�����ṩĿǰ��֪��ÿ���µ��¼����ţ�{ListString.ListToString<Event_Object>(expose_EventObjects)}
        Debug.Log("��ʼ����С�¼���λĿ��Ȩ������");
        string prompt = $@"ģ��һ����ѧ���������������еĺ���Ŀ��Ȩ�طֲ���ÿ������Ŀ���Ȩ���ܺ�Ϊ1��
�ĸ�����ֱ�Ϊ��ѧҵ�ɾ͡�ְҵ׼�������˳ɳ����罻��ϵ��
�ۺ�ÿ�µ��¼�������ÿ���µ���Ŀ��Ȩ�ء�
�����ۺ�ÿ�µ��¼�������ÿ���µ���Ŀ��Ȩ�ء�
����Json��ʽ���лظ���[
{{
""month_index"":1,
""ѧҵ�ɾ�"":0.1,
""ְҵ׼��"":0.1,
""���˳ɳ�"":0.1,
""�罻��ϵ"":0.1
}}
]
�벻Ҫ���س�Json����������κ�����";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = APlan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, APlan_TargetWeigh_CallBack));
    }

    void APlan_TargetWeigh_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        APlan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("����Json");
            Avatar_TargetWeight = JsonConvert.DeserializeObject<List<Target_WeightObject>>(Json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }
        APlan_SpecifyPlan();
    }

    public void APlan_SpecifyPlan(Action CallBack = null)
    {
        Debug.Log("��ʼ���мƻ�����");
        //List<Target_WeightObject> newWeigh = new();
        //newWeigh.Add(Avatar_TargetWeight[settings.month_Index - 1]);
        //���Ŀ��:{ListString.ListToString(Avatar_Target)}
        string prompt = $@"������ģ�´�ѧ���������ɽ�����7��ʱ���ļƻ����ƻ����ܵ�����Ը񡢻�����Ŀ��ȶ෽��Ӱ�졣
�ƻ�ʱ��������system prompt���ṩ����ʵʱ��Ϊ׼��7���¼���ʱ��˳�����С�
����Ҫ���¼����У�ѡ���¼�����.�¼��أ�{Event_Pool}.
Ŀ�굼�����¼�ѡ�����Ӱ�죬����Ӱ��Ϊ��
ѧ���ɾͣ�ѧϰ��ѵռ���������ӣ�����֧����ѹ�����޳��Ͷ���
ְҵ׼�����г��Ͷ�/����ѧϰ���ȼ�����������֧��ƫ��ʵ�û�����ͨ���Χ����
���˳ɳ���������֧���߶Ƚṹ������������Ż���ѧϰ��ѵ�����ò�
�罻��ϵ��������֧���罻�����޳��Ͷ�Э������ǿ��ѧϰ��ѵЧ��Ҫ�����
��1��2������Ŀ����Ϊ������Ϊ��ʱ����µļƻ��¼���
����ѡ��ÿ��ʱ���ļƻ��¼����ƻ��¼������ӳ���Ļ���ߡ��ظ���������Json��ʽ
[
{{""Index"":1,
""Event"":[{{""type"":""<ѡȡ�¼������ͣ���<ѧϰ��ѵ><����֧��>>"",""driven_type"":""<˵���������ͣ���ѧ���ɾ͡�ְҵ׼����>""""specify_event"":""<�����¼�>""}}��{{��type��:""<ѡȡ�¼������ͣ���<ѧϰ��ѵ><����֧��>>"",""driven_type"":""<˵���������ͣ���ѧ���ɾ͡�ְҵ׼����>""""specify_event"":""<�����¼�>""}}]}},
]
�벻Ҫ���س�Json����������κ�����";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        APlan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = APlan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, APlan_SpecifyPlan_CallBack));
    }

    void APlan_SpecifyPlan_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        APlan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("����Json");
            CurMon_Plan = JsonConvert.DeserializeObject<List<Week_Plan>>(Json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }

        //ֱ���������������Ի�
        //api_chat.Avatar_ProActive_Chat();
        Debug.Log("���е�һ��share moment");
        shareMomentControl.ShareMomentStart();
    }

    public void AUnexpect_Event(Action CallBack = null)
    {
        Debug.Log("��ʼͻ���¼�����");
        reader.LoadFile("Unexpect_Pool.json", Add_Unexpect);
    }
    void Add_Unexpect(string prompt)
    {
        string full_prompt = $@"����һ������ͻ���¼���agent���������ģ�»��񣬷������ʱ������������ͻ���¼���
�һ�����ṩһ��ͻ���¼�����Ϊ��Ĳο���������һ��ͻ���¼����ݼ��ɡ�ͻ���¼��أ�"+ prompt;

        string simplePrompt = "����������һʱ����ͻ���¼����ݡ�";

        string m_Prompt = AUnexpect_Dial.Count > 1 ? simplePrompt : full_prompt;

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",m_Prompt }
        };
        AUnexpect_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = AUnexpect_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, AUnexpect_Event_CallBack));
    }

    void AUnexpect_Event_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        AUnexpect_Dial.Add(newmmessage);
        Debug.Log("ͻ���¼���" + text);
        Unexpect_Objects.Add(new Event_Object
        {
            event_index = MomentIndex, 
            Event = text
        });
    }
   


    string JsonPatch(string rawText)
    {
        try
        {
            // ����ƽ������ȡ����� [...]
            string pattern = @"\[(?:[^[\]]+|(?<open>\[)|(?<-open>\]))+(?(open)(?!))\]";
            Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);
            if (!match.Success) return null;
            Debug.Log("�ᴿJson"+match.Value);
            // �ٽ�����ȡ��� JSON
            return match.Value;
        }
        catch (Exception ex)
        {
            Debug.LogError($"����ʧ�ܣ�{ex.Message}");
            return null;
        }
    }
    #region ����Ŀ��
    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }

    [Serializable]
    public class Target_Object
    {
        public string ѧҵ�ɾ�;
        public string ְҵ׼��;
        public string ���˳ɳ�;
        public string �罻��ϵ;
    }

    [Serializable]
    public class Target_WeightObject
    {
        public int month_index;
        public float ѧҵ�ɾ�;
        public float ְҵ׼��;
        public float ���˳ɳ�;
        public float �罻��ϵ;
    }
    [Serializable]
    public class Week_Plan
    {
        public int Index;
        public List<Week_Event> Event;
    }
    [Serializable]
    public class Week_Event
    {
        public string type;
        public string driven_type;
        public string specify_event;
    }
    #endregion

}


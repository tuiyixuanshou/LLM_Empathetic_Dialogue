using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MyUtilities;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using static AvatarMainStoryDemoV2;

public class AvatarMainStoryDemoV2 : MonoBehaviour
{
    public ShareMomentControl shareMomentControl;
    public Settings settings;
    public SimpleFileReader reader;
    public API_Chat api_chat;

    [Header("Agent������ά��")]
    List<Dictionary<string, string>> A_Status_Dial= new();
    List<Dictionary<string, string>> A_Eval_Dial = new();
    List<Dictionary<string, string>> A_Plan_Dial= new();  //
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
    public int timeIndex = 0;
    public string World_Object;
    public List<Status_Object> Avatar_Status; //Avatar״̬
    public List<Eval_WeightObject> Eval_Weight; //Avatar��Ϊ������׼
    public List<A_ConcreteBehavior> plan_Objects ;//������Ϊ���ɺ��ɸѡ��Ϊ
    public List<A_ConcreteBehavior> concreteBehaviors; //һ�վ�����Ϊ

    private List<AbstractBehavior> Abstract_behaviors; //�������¼���

    //public List<Event_Object> Unexpect_Objects;

    private void Awake()
    {
        StartSampleandConcreteBehavior();
    }
    private void Start()
    {
        reader.LoadFile("System_Prompt.json", Add_SystemPrompt);
        reader.LoadFile("Abstract_Event_Pool.json", Abstract_EventPoolSet);
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
        A_Status_Dial.Add(newmmessage);
        A_Eval_Dial.Add(newmmessage);
        A_Plan_Dial.Add(newmmessage);

        AUnexpect_Dial.Add(newmmessage);
        //A_Status_t();
    }

    void Abstract_EventPoolSet(string text)
    {
        Abstract_behaviors = JsonConvert.DeserializeObject<List<AbstractBehavior>>(text);
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
            //A_Eval_t();
        }
    }

    public void A_Status_t()
    {
        Debug.Log("��ʼ��ɫ״̬����");
        string prompt = $@"������ģ��һ�������ѧ�����ճ�����ϵͳ��
������ݵ�ǰ��ʱ�����龳�������Ʋ������������һ�������硢���硢��������ʱ��ε�����������״̬��
���������½ṹ����Ϣ��
1. ʱ��Σ����ɵ��������ݷֱ�������硢���硢����ʱ��Σ�
2. ������0-100֮�������������ǰ�������뾫��״̬��
3. ���飨-5��5�������������ɫ��������������ֵΪ�����ģ���ֵΪ���ģ�
4. ѹ����0.0 �� 1.0����������ѹ��ˮƽ��
5. �������ֱ��г������·���ĵ�ǰ����������ֵ��ΧΪ-1.0��1.0����
   - ����
   - ѧϰ
   - �罻
   - �˶�
   - ��������

�������� JSON �ṹ�����
[
{{
  ""ʱ��"": ""����"",
  ""����"": ,
  ""����"": ,
  ""ѹ��"": ,
  ""����"": {{
    ""����"": ,
    ""ѧϰ"": ,
    ""�罻"": ,
    ""�˶�"": ,
    ""��������"": 
  }}
}},
{{
  ""ʱ��"": ""����"",
  ""����"": ,
  ""����"": ,
  ""ѹ��"": ,
  ""����"": {{
    ""����"": ,
    ""ѧϰ"": ,
    ""�罻"": ,
    ""�˶�"": ,
    ""��������"": 
  }}
}},
{{
  ""ʱ��"": ""����"",
  ""����"": ,
  ""����"": ,
  ""ѹ��"": ,
  ""����"": {{
    ""����"": ,
    ""ѧϰ"": ,
    ""�罻"": ,
    ""�˶�"": ,
    ""��������"": 
  }}
}}
]
�벻Ҫ���س�Json����������κ�����";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        A_Status_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = A_Status_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, A_Status_t_CallBack));
    }

    void A_Status_t_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        A_Status_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("����Json");
            Avatar_Status = JsonConvert.DeserializeObject<List<Status_Object>>(Json);
            A_Eval_t(timeIndex);

        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }
    }

    public void A_Eval_t(int timeIndex)
    {
        Debug.Log("��ʼ������׼Ȩ������");
        //List<Target_WeightObject> newWeigh = new();
        //newWeigh.Add(Avatar_TargetWeight[settings.month_Index - 1]);
        //���Ŀ��:{ListString.ListToString(Avatar_Status)}
        string prompt = $@"����һ��������ΪȨ���������������Ǹ��ݽ�ɫ���Ը񡢻���������������״̬���Լ��ⲿ�������¼���Ϊ�����ɵ��µ���Ϊƫ�÷ֲ���
����ǰ��ɫ״̬��
- ʱ�䣺{Avatar_Status[timeIndex].ʱ��}
- ����ֵ��{Avatar_Status[timeIndex].����}��0-100��Խ�ߴ���Խ��������
- ����ֵ��{Avatar_Status[timeIndex].����}��-5��5����ֵ�����������䣩
- ѹ��ˮƽ��{Avatar_Status[timeIndex].ѹ��}��0.0-1.0��Խ�߱�ʾѹ��Խ��
- ����ֵ��
  - ���ɣ�{Avatar_Status[timeIndex].����.����}
  - ѧϰ��{Avatar_Status[timeIndex].����.ѧϰ}
  - �罻��{Avatar_Status[timeIndex].����.�罻}
  - �˶���{Avatar_Status[timeIndex].����.�˶�}
  - ����������{Avatar_Status[timeIndex].����.��������}
���ⲿ�����¼���
{World_Object}����������ڽ�ɫ�Ը���ԣ����¼������ΪȨ�ز�����Ӱ�죩

���������״̬����������һ����Ϊ���Ȩ�أ���ʾ��ǰ��ɫ��������Ϊ�ϵ�����ע�⣺
- �����ɫ�����ϵ͡�ѹ���ϴ�ͨ����ƫ�á����ɡ�
- �����ɫ����á��������㣬�����������˶�������ܸ���
- ����ֵ�ǽ�ɫ���������������Ȳο�����ҲҪ���Ǿ�����ѹ���ĵ�������
- �ⲿ�¼��ǵ�����ʵ�����з������¼������ۺϽ�ɫ�Ը�����״̬�������ⲿ�¼����ڽ�ɫ���ΪȨ�ص�Ӱ��

���������JSON �ṹ��ֵ��Χ0.0~1.0����
[
{{
  ""����"": ,
  ""ѧϰ"": ,
  ""�罻"": ,
  ""�˶�"": ,
  ""��������"": ,
  ""����"":""""
}}
]";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        A_Eval_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = A_Eval_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, A_Eval_CallBack));
    }

    void A_Eval_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        A_Eval_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            Debug.Log("����Json");
            var Weight = JsonConvert.DeserializeObject<List<Eval_WeightObject>>(Json)[0];
            Eval_Weight.Add(Weight);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }

        //Debug.Log("���е�һ��share moment");
        //shareMomentControl.ShareMomentStart();
        if (timeIndex < 2)
        {
            timeIndex++;
            A_Eval_t(timeIndex);
        }
        else
        {
            Debug.Log("Eval������׼���ɽ�����������һ��");
            timeIndex = 0;
            Debug.Log("��ʼ�Գ�����Ϊ����в���");
            StartSampleandConcreteBehavior();

        }
    }

    public void StartSampleandConcreteBehavior()
    {
        if (timeIndex < 3)
        {
            var cur_AbstractBehavior = SampleTopBehaviors(Eval_Weight[timeIndex], 1);
            Debug.Log("��ó�����Ϊ��" + cur_AbstractBehavior[0].name);
            A_Plan_t(cur_AbstractBehavior, timeIndex);
            timeIndex++;
        }
        else
        {
            Debug.Log("ͬ�������Ѿ����ɽ�����");
            for(int i = 0; i < 3; i++)
            {
                Scene_Recording newScene = new Scene_Recording
                {
                    timeIndex = i,
                    state_Object = Avatar_Status[i],
                    a_ConcreteBehavior = concreteBehaviors[i],
                    World_Plan = World_Object,
                };
                settings.Share_Scenes_List.Add(newScene);
            }
            Debug.Log("��ȫ������Settings��");
        }
        
    }

    public List<AbstractBehavior> SampleTopBehaviors(Eval_WeightObject preference, int topN = 3)
    {
        return Abstract_behaviors
            .OrderByDescending(b =>
                preference.ѧϰ * b.behavior_traits.ѧϰ +
                preference.���� * b.behavior_traits.���� +
                preference.�罻 * b.behavior_traits.�罻 +
                preference.�˶� * b.behavior_traits.�˶� +
                preference.��������* b.behavior_traits.��������
            )
            .Take(topN)
            .ToList();
    }

    public void A_Plan_t(List<AbstractBehavior> preference,int timeIndex)
    {
        Debug.Log("��ʼϸ���¼�����");
        string prompt = $@"�㽫����������Ϊϸ����
����ݸ����ġ�������Ϊ������ʵ�������ݡ��롰��ǰ��ɫ״̬�������ɶ��ֿɹ�ѡ��ľ�����Ϊѡ�ÿ��ѡ��Ӧ���к�������ݡ��ص�͵�ǰƫ�����֣����ں�����������
��������Ϊ����{ListString.ListToString(preference)}
����ʵ�������ݡ���{World_Object}��������ʵ�����������ɾ���ƻ���
����ɫ״̬����
- ��ǰ���飺{Avatar_Status[timeIndex].����}����Χ��-5��+5��
- ��ǰ������{Avatar_Status[timeIndex].����}����Χ��0-100��
- ��ǰѹ����{Avatar_Status[timeIndex].ѹ��}(��Χ��0.0 �� 1.0����������ѹ��ˮƽ)
- ����ֵ��
  - ���ɣ�{Avatar_Status[timeIndex].����.����}
  - ѧϰ��{Avatar_Status[timeIndex].����.ѧϰ}
  - �罻��{Avatar_Status[timeIndex].����.�罻}
  - �˶���{Avatar_Status[timeIndex].����.�˶�}
  - ����������{Avatar_Status[timeIndex].����.��������}

�����Ҫ�󡿣�
������ 3��5 �� JSON ��ʽ�ĺ�ѡ��Ϊѡ�ÿ�����������ֶΣ�
[
  {{
    ""title"": ""��Ϊ���⣬���磺�����Ѿ۲�"",
    ""description"": ""һ�仰������ΪĿ�ĺ͸���"",
    ""location"": ""��Ϊ�����ĳ���"",
    ""weight"": 0.0��1.0����ʾ�˿̽�ɫ�Ը���Ϊ������""
  }}
]";
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        A_Plan_Dial.Add(newmmessage);
        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = A_Plan_Dial,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(PostWeb.postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, A_Plan_CallBack));
    }

    void A_Plan_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        A_Plan_Dial.Add(newmmessage);
        string Json = JsonPatch(text);
        plan_Objects.Clear();
        plan_Objects = JsonConvert.DeserializeObject<List<A_ConcreteBehavior>>(Json);

        Debug.Log("����ж����ݣ�����������ݲ���");
        var item = SampleConcreteBehavior(plan_Objects);
        concreteBehaviors.Add(item);
    }

    private A_ConcreteBehavior SampleConcreteBehavior(List<A_ConcreteBehavior> options) 
    {
        float totalchance = 0f;
        foreach(var item in options)
        {
            totalchance += item.weight;
        }
        float rand = UnityEngine.Random.Range(0, totalchance);
        float cur_chance = 0f;
        foreach(var item in options)
        {
            cur_chance += item.weight;
            if (rand <= cur_chance)
            {
                return item;
            }
        }
        return options[0];
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
        //Unexpect_Objects.Add(new Event_Object
        //{
        //    event_index = MomentIndex, 
        //    Event = text
        //});
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

    [System.Serializable]
    public class AbstractBehavior
    {
        public string id;
        public string name;
        public string[] tags;
        public string activation_hint;
        public string[] typical_locations;
        public A_S_Desire behavior_traits;
    }

    [Serializable]
    public class Event_Object
    {
        public int event_index;
        public string Event;
    }


    [Serializable]
    public class Eval_WeightObject
    {
        public float ����;
        public float ѧϰ;
        public float �罻;
        public float �˶�;
        public float ��������;
        public string ����;
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


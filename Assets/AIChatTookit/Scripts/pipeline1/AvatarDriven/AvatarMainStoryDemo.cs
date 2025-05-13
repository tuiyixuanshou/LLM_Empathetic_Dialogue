using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static AvatarMainStoryDemo;
using System.Reflection;
using Unity.VisualScripting;

public class AvatarMainStoryDemo : MonoBehaviour
{
    public Settings settings;
    public KLingAPIImage API_Image;
    public KLingAPIDemo API_Video;
    public API_Chat api_chat;
    public quadVideo quadvideo;
    public AvaterBubbleControl bubbleControl;
    [Header("�������߹������ݵ�ģ��")]
    public ChatModel MainLine_Model;
    public LLMURL MainLine_url;
    public APIKey MainLine_api;

    [Header("�������߹������ݵ�ģ��")]
    public ChatModel MainLineChat_Model;
    public LLMURL MainLineChat_url;
    public APIKey MainLineChat_api;

    public string AvatarBackGround = "��ѧ��";  //���Կ����Ƿ��ƽ���Settings��������˵ʵ�ڵ�setting��ʵҲ�����ã��������Ҫ���ش洢��
    public string MainTarget="ȥ��һ���ݳ���";
    [TextArea(5,5)]
    public string EventPool;
    public List<MonthTargetRespond> monthTargetResponds = new();
    public List<WeekTargetRespond> weekTargetResponds = new();
    public List<WeekEvent> weekEvents = new();
    public List<WeekEvent> choseWeekEvents = new();

    [TextArea(5, 5)]
    public string newImagePrompt;

    public string CurDialogue;
    public string ImageURL;

    public int month = 1;
    public int week = 0;

    List<Dictionary<string, string>> tempDialogue = new();
    List<Dictionary<string, string>> tempChatDialogue = new();

    private void Start()
    {
        //StartGenerateImage();
        //string Json = "[\r\n{\"month_index\":1,\"month_target\":\"��Ӧ��ѧ�ڿγ̽��࣬�滮����ʱ�䣻�˽��ݳ������ʱ���볡����Ϣ���ƶ�������ƱԤ�㣻�μ�У�����Ż����չ�罻Ȧ�������ճ��ܲ�������Ϊ��;���д�������\"},\r\n{\"month_index\":2,\"month_target\":\"������п��Ը�ϰ����ͬѧ���Ԥ��������ͨƱ��ͨ����ְ/��ʡ���֧�����ݳ�����𣻲μ�У԰������ĸ質��ϰ��������ݳ����ܱ���ʳ����\"},\r\n{\"month_index\":3,\"month_target\":\"Э����ĩ��ҵ�����ʱ�䣻���ñ�Яʽ��籦��������Ʒ����������ҹ̸���ƶ�ӦԮ������ά�ֹ�����Ϣ���������ӣ���ע��������׼�����ʷ���\"}\r\n]";
        //string m_Json = JsonPatch(Json);
        //monthTargetResponds = JsonConvert.DeserializeObject<List<MonthTargetRespond>>(m_Json);
        //Debug.Log(monthTargetResponds);
        //monthTargetResponds = JsonConvert.DeserializeObject<List<MonthTargetRespond>>("[\r\n{\"month_index\":1,\"month_target\":\"�����ſ���ʱ����Ϥ�ݳ��������Ϣ���𲽵�����ѧ�ڿγ̽��ࣻ������ʱ���о��ݳ��᳡�ؽ�ͨ���ܱ���ʩ�����������ĩ����ͬѧ�罻���������Ĭ��������ÿ�����ε�У԰������Ӧ��������\"},\r\n{\"month_index\":2,\"month_target\":\"ͨ���ҽ̼�ְ���������ʽ𴢱��˻���ÿ���ʡ15Ԫ������֧��ΪӦ�����𣻼���У԰����������չ�赥�����������μ����γ���̽�����Ϥ��վ��Ŧ·�ߣ���ĩ���У԰��Ᵽ�����ܻ�׼\"},\r\n{\"month_index\":3,\"month_target\":\"ȷ�����ս�ͨס�޷�������ͬ��Э�̷ֹ������ñ�ҪӦԮװ��������Ԥ��30%���Ż��γ���ҵ����ȷ���������޿��˳�ͻ��ά��ҹ��ϰ����������վ���������������θ������¾ۻ������ֳ���������\"}\r\n]");
        //weekTargetResponds = JsonConvert.DeserializeObject<List<WeekTargetRespond>>("[\r\n{\"month_Index\":1,\"week_Index\":1,\"week_target\":\"��Ϥ�ݳ���������Ŀ�������Ϣ�������γ�ʱ�����\",\"eventType\":\"1-�Ͽ�,12-д��ҵ,7-ɸѡ�ݳ���赥\"},\r\n{\"month_Index\":1,\"week_Index\":2,\"week_target\":\"���õ�ͼ����о�·�ߣ��μ�ͬѧ�۲�Э�̽��\",\"eventType\":\"9-��;̽�ý�ͨվ��,8-�༶�����,4-�����ַ���\"},\r\n{\"month_Index\":1,\"week_Index\":3,\"week_target\":\"ά��У԰������Ӧ�ƻ������רҵ��Ԥϰ\",\"eventType\":\"5-У԰ҹ�ܴ�,2-ͼ�����ϰ,6-����׷������\"},\r\n{\"month_Index\":1,\"week_Index\":4,\"week_target\":\"����������Ʊ���������ܱ߲�������\",\"eventType\":\"8-�������²軰��,7-̽��ֱ���ۿ�,5-��ë����ϰ\"},\r\n\r\n{\"month_Index\":2,\"week_Index\":1,\"week_target\":\"�����ҽ̼�ְ�����ˣ��ƶ��ڼ���ʳ����\",\"eventType\":\"3-��ѧ��ѧ����,4-���Ʊ㵱�ƻ�,5-�����٤\"},\r\n{\"month_Index\":2,\"week_Index\":2,\"week_target\":\"�������ּ��͹����������Ի���·�߿�����\",\"eventType\":\"8-��������ɳ��,9-������·����,12-���Ŀ�ܴ\"},\r\n{\"month_Index\":2,\"week_Index\":3,\"week_target\":\"����Ӧ���ʽ���֧������������׼��\",\"eventType\":\"3-����Ӣ������,5-800��ǿ��ѵ��,6-ƴװģ�ͼ�ѹ\"},\r\n{\"month_Index\":2,\"week_Index\":4,\"week_target\":\"���̽�ͨԤ��©��������ͬ�Ǹ��Խ���\",\"eventType\":\"8-����QQȺ����,1-����ѡ�޿�,7-��ӰԺ��Ӱ\"},\r\n\r\n{\"month_Index\":3,\"week_Index\":1,\"week_target\":\"Ԥ������Ʊ���ȶ�ס�ޣ��ɹ�ӫ����ȵ���\",\"eventType\":\"7-����ȼ۹���,9-����ʵ�ؿ���,12-�γ̻㱨�Ϲ�\"},\r\n{\"month_Index\":3,\"week_Index\":2,\"week_target\":\"Э���ֳ�ӦԮ�ֹ����������ܷ�ֵ״̬\",\"eventType\":\"8-ӦԮ��ְ������,5-��������ѵ��,4-׷�粹��˯��\"},\r\n{\"month_Index\":3,\"week_Index\":3,\"week_target\":\"�����ĩ��ҵǰ�ã�ģ���ݳ���վ������\",\"eventType\":\"12-ʵ�鱨��׫д,5-����ѭ��������ϰ,6-�ַ�DIY����\"},\r\n{\"month_Index\":3,\"week_Index\":4,\"week_target\":\"�����г̺˶ԣ��μ�ǿ����ӦԮ�ں�����\",\"eventType\":\"8-����ӦԮ��ѵӪ,3-��ʱ������ְ,4-ڤ����������\"}\r\n]");
    }
    public void MonthTarget()
    {
        Debug.Log("��ʼÿ��Ŀ������");
        string prompt = $@"��������Ҫģ��һ��{AvatarBackGround}�������º��㽫{MainTarget}����Ŀ����ڵ������µ�ĩβ������
�뽫��Ŀ������������ڣ�ÿ������Ҫ��ɵ�СĿ�ꡣ�����ʱ���ά�ȣ��������ÿ���µĻ���ݡ�
ע�⣺���ɵ�ÿ�»Ŀ�겻Ҫ����Ŀ�ĵ���Ҫ����������ճ�����滮������ʵ��Ŀ��֮����Ҫ���Ƿ������ﱳ���趨���ճ������еĻ��
����Json��ʽ�������ݣ���ʽ�ο����£�[
{{""month_index"":1,""month_target"":""��һ����Ŀ��""}},
{{""month_index"":2,""month_target"":""�ڶ�����Ŀ��""}},
{{""month_index"":3,""month_target"":""��������Ŀ��""}},
]
�벻Ҫ���س�Json����������κ�����";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = tempDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, MonthTarget_CallBack));
    }

    void MonthTarget_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        tempDialogue.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            monthTargetResponds = JsonConvert.DeserializeObject<List<MonthTargetRespond>>(Json);
            Debug.Log("monthTarget ���ɳɹ�");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }
    }

    public void WeekTargetAndBenchMark()
    {
        Debug.Log("��ʼÿ��Ŀ����");
        string prompt = $@"������������ɵ�ÿ����Ŀ�꣬����Ŀ���������ÿ�ܵĻĿ��ͱ�׼�¼����͡�����ÿ���������ܡ�
�����׼�¼�������ָ�ܹ����ϱ��ܻĿ����¼����ͣ���Ҫ���¼����н���ѡ���¼��أ�{EventPool}��
����Json��ʽ�������ݣ����磺[
{{""month_Index"":1,""week_Index"":1,""week_target"":""��Ŀ��"",""eventType"":""��׼�¼�����,չʾ��ʽΪ���+�����¼�""}}��
{{""month_Index"":1,""week_Index"":2,""week_target"":""��Ŀ��"",""eventType"":""��׼�¼�����,չʾ��ʽΪ���+�����¼�""}}��
{{""month_Index"":1,""week_Index"":3,""week_target"":""��Ŀ��"",""eventType"":""��׼�¼�����,չʾ��ʽΪ���+�����¼�""}}��
{{""month_Index"":1,""week_Index"":4,""week_target"":""��Ŀ��"",""eventType"":""��׼�¼�����,չʾ��ʽΪ���+�����¼�""}}��
]�벻Ҫ���س�Json����������κ�����";

        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = tempDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, WeekTargetAndBenchMark_CallBack));
    }

    void WeekTargetAndBenchMark_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        tempDialogue.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            //weekTargetResponds = JsonUtility.FromJson<List<WeekTargetRespond>>(Json);
            weekTargetResponds = JsonConvert.DeserializeObject<List<WeekTargetRespond>>(Json);
            Debug.Log("weekTarget ���ɳɹ�");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }
    }

    public void WeekEventGenerate()
    {
        Debug.Log("ÿ���¼�����");
        UpdateMonthAndWeek();
        string LastWeekEvent = CheckLastWeek();
        string prompt = $@"���ڽ����˵�{month}���µ�{week}�ܣ�������ǰ���ɵ��������ݣ������Ŀ����{MainTarget}����Ŀ����{monthTargetResponds[month-1].month_target},
����Ŀ���ǣ�{weekTargetResponds[week - 1].week_target},���ܱ�׼�¼��ǣ�{weekTargetResponds[week-1].eventType}.
����ѡ��Ļ���ݣ�{LastWeekEvent}��
����Ϊ��ģ���˵���ʵ״̬������ʱ���ܹ��ı����е����߹滮���������ճ������е�ͻ���¼���
Ϊ�˻�ԭ���ֲ�ȷ���ͣ��������¼��������ѡ��3-4���¼����ͣ���ϸ�������¼����ݡ�
ϸ�������жϴ��¼��ͱ�׼�¼�������ԣ�����ͱ�׼�¼��ǳ���أ�Ҳ�뱾�ܡ����µ�Ŀ��ǿ��أ���ô����·����ĸ��ʾ͸ߣ�
������¼���ȫΥ���˱�׼�¼���Υ���˱��ܡ�����Ŀ�꣬��ô����·������ʾ͵͡�
�������¼��ͱ���Ŀ���Լ���׼�¼����͵�����ԣ���ȫ���Ϊ1����ȫ�����Ϊ0
���ٸ�������ԣ����ɸ��¼����ܷ����ĸ��ʣ���֤��Щ���ʵ��ܺ���100%��������Ե����ݸ��ʿ��Ժ͵���������ݸ��ʲ����Ĳ�ֵ�ϴ�
����Json��ʽ�������ݣ����磺[
{{""event_Index"":1,""event_Type"":""�¼�����"",""eventDetails"":""ϸ���¼�����"",""correlation"":0.5��""occurance"":0.5}}��
{{""event_Index"":2,""event_Type"":""�¼�����"",""eventDetails"":""ϸ���¼�����"",""correlation"":0.5��""occurance"":0.5}}��
]�벻Ҫ���س�Json����������κ�����";
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = tempDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, WeekEventGenerate_CallBack));

    }

    void WeekEventGenerate_CallBack(string text)
    {
        var newmmessage = new Dictionary<string, string>
        {
            {"role","assistant" },
            {"content",text }
        };
        tempDialogue.Add(newmmessage);
        string Json = JsonPatch(text);
        try
        {
            //weekTargetResponds = JsonUtility.FromJson<List<WeekTargetRespond>>(Json);
            //weekEvents = JsonUtility.FromJson<List<WeekEvent>>(Json);
            weekEvents = JsonConvert.DeserializeObject<List<WeekEvent>>(Json);
            Debug.Log("weekEvents ���ɳɹ�");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON ����ʧ�ܣ�" + ex.Message);
        }
    }
    string CheckLastWeek()
    {
        if(choseWeekEvents.Count == 0)
        {
            return "����";
        }
        else
        {
            string text = $"{"�¼����ͣ�" + choseWeekEvents[choseWeekEvents.Count - 1].event_Type + " �¼�ϸ�ڣ�" + choseWeekEvents[choseWeekEvents.Count - 1].eventDetails + " �¼���������ضȣ�" + choseWeekEvents[choseWeekEvents.Count - 1].correlation + " �¼��������ʣ�" + choseWeekEvents[choseWeekEvents.Count - 1].occurance}";
            return text;
        }
    }

    void UpdateMonthAndWeek()
    {
        if(month > 3)
        {
            Debug.LogError("ֹͣ��������");
        }
        if (week<4)
        {
            week++;
        }
        else
        {
            month++;
            week = 1;
        }
    }

    public void ChooseWeekEvent(int index)
    {
        choseWeekEvents.Add(weekEvents[index]);
    }

    public void SceneImagePromptGenerate()
    {
        Debug.Log("ͼƬprompt����");
        string prompt = $@"��������һ������ḻ��AIGCprompt��׫д��,��Ҳ�зḻ������ѧ���顣
���ڣ�����Ҫ�����Ѿ�ѡ�����ѡ��ı����¼����ݣ�����һ�������������ͼƬ��prompt��
������ѡ�������ǣ�{"�¼����ͣ�"+choseWeekEvents[choseWeekEvents.Count - 1].event_Type+" �¼�ϸ�ڣ�"+choseWeekEvents[choseWeekEvents.Count-1].eventDetails+ " �¼���������ضȣ�"+choseWeekEvents[choseWeekEvents.Count - 1].correlation+" �¼��������ʣ�"+ choseWeekEvents[choseWeekEvents.Count - 1].occurance};
����Ҫ�ж�һ�����ϱ����¼������ĳ��������ɸó�����λ�����ƣ������ɸó����е�ϸ�����ݣ�ʹ���ɵ����ݿ�����Ϊ����ͼƬ��prompt��
��ע�⣬�����Ѿ�����һ�����˹���������Ҫ�����˹����뱳���У�����Ҫ�������˹��ڳ����еļ����������ͱ�����������Ҫ�����˹�����ò���������������ݣ�ͳһʹ�á����˹������档
�����ؼ����������ͱ�����������Ҫ���������κ����ݡ�";
        Debug.Log(prompt);
        List<Dictionary<string, string>> temp = new();
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        temp.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = temp,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, SceneImagePrompt_CallBack));
    }

    void SceneImagePrompt_CallBack(string text)
    {
        newImagePrompt = text;
    }

    public void StartGenerateImage()
    {
        Debug.Log("ͼƬ����");
        StartCoroutine(GenerateImage());
    }

    public IEnumerator GenerateImage()
    {
        API_Image.prompt = newImagePrompt+"���ɵĳ�����Ҫ�ɰ�������ܰ������";
        Debug.Log(API_Image.prompt);
        yield return API_Image.GenerateImage(GenerateImage_CallBack);
    }

    void GenerateImage_CallBack(string text)
    {
        ImageURL = text;
    }

    public void ChatGenerate()
    {
        Debug.Log("�Ի���������");
        string prompt = $@"��������һ���������С���飬��������Ҫģ��һ��{AvatarBackGround}�������º��㽫{MainTarget}��
���ڽ����˵�{month}���µ�{week}�ܣ�������ǰ���ɵ��������ݣ������Ŀ����{MainTarget}����Ŀ����{monthTargetResponds[month - 1].month_target},
����Ŀ���ǣ�{weekTargetResponds[(month-1)*4+ week - 1].week_target},������ѡ�������ǣ�{"�¼����ͣ�" + choseWeekEvents[choseWeekEvents.Count - 1].event_Type + " �¼�ϸ�ڣ�" + choseWeekEvents[choseWeekEvents.Count - 1].eventDetails + " �¼���������ضȣ�" + choseWeekEvents[choseWeekEvents.Count - 1].correlation + " �¼��������ʣ�" + choseWeekEvents[choseWeekEvents.Count - 1].occurance}��
���ڵĳ��������������{newImagePrompt}��
����Ҫ���ݱ��ܵ�������������������������ĺ��û��ĶԻ����Ի����ݼ򵥿ɰ���������15���֣������ճ��Ի����ݡ�";
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        tempChatDialogue.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLineChat_Model),
            messages = tempChatDialogue,
            stream = false
        };

        string Jsonpayload = JsonConvert.SerializeObject(payload);
        StartCoroutine(postRequest(settings.m_SetUrl(MainLineChat_url), settings.m_SetApi(MainLineChat_api), Jsonpayload, ChatGenerate_CallBack));
    }

    void ChatGenerate_CallBack(string text)
    {
        CurDialogue = text;
    }


    public void StartGenerateVideo()
    {
        StartCoroutine(GenerateVideo());
    }

    IEnumerator GenerateVideo()
    {
        Debug.Log("��Ƶ����"); 
        string prompt = $@"��������һ������ḻ��AIGCprompt��׫д��,��Ҳ�зḻ������ѧ���顣
���ڣ�����Ҫ������ѡ��ı����¼����ݣ����Ѿ����ɵĳ���ͼƬ������һ�������Ƶ���ɵ�prompt��
������ѡ�������ǣ�{"�¼����ͣ�" + choseWeekEvents[choseWeekEvents.Count - 1].event_Type + " �¼�ϸ�ڣ�" + choseWeekEvents[choseWeekEvents.Count - 1].eventDetails + " �¼���������ضȣ�" + choseWeekEvents[choseWeekEvents.Count - 1].correlation + " �¼��������ʣ�" + choseWeekEvents[choseWeekEvents.Count - 1].occurance}���Ѿ����ɵı���������{newImagePrompt}��
����Ҫ�������˹��ڴ˳����µļ򵥡�����Ķ�����
��ע�⣬�˶�����Ҫ���ϵ��³��������˹�Ŀǰ�����ƣ���������򵥡�
�����ؼ򵥵Ķ�����������Ҫ���������κ����ݡ�";
        List<Dictionary<string, string>> temp = new();
        var newmmessage = new Dictionary<string, string>
        {
            {"role","user" },
            {"content",prompt }
        };
        temp.Add(newmmessage);

        var payload = new
        {
            model = settings.m_SetModel(MainLine_Model),
            messages = temp,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        yield return postRequest(settings.m_SetUrl(MainLine_url), settings.m_SetApi(MainLine_api), Jsonpayload, VideoPrompt_CallBack);
        yield return API_Video.GenerateVideo(API_Video.prompt, tempShowVideo);
    }

    void VideoPrompt_CallBack(string text)
    {
        API_Video.prompt = $@"���ڽ��ͼƬ����ԭ�������Լ�������ǰ���£��Զ�����һ�κ����{text},�����ͱ�����Ҫ�ɰ�����";
        API_Video.ImageBase64 = ImageURL;
    }

    //��Ƶ���ţ�û���ֵ䶨�壬��������
    void tempShowVideo(string url)
    {
        quadvideo.RespondToM_Action(url);
        api_chat.PassiveDialogue_CallBack(CurDialogue);
    }

    IEnumerator postRequest(string url, string api, string json, Action<string> callback)
    {
        using(var uwr = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Authorization", "Bearer " + api);

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error While Sending: " + uwr.error);
                Debug.Log("Full respond:" + uwr.downloadHandler.text);
            }
            else
            {
                Debug.Log("Received: " + uwr.downloadHandler.text);
                string response = uwr.downloadHandler.text;
                ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
                string responseJson = apiResponse.choices[0].message.content;
                Debug.Log(responseJson);
                callback(responseJson);
            }
        }
        
    }

    string JsonPatch(string rawText)
    {
        string pattern = @"\[.*?\]";
        Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string extractedJson = match.Value;
            Debug.Log("��ȡ�� JSON ���ݣ�" + extractedJson);
            return extractedJson;
        }
        else
        {
            Debug.Log("û���ҵ��������ڵ����ݣ�Json���ݷ�����ȫʧ�ܣ�");
            return null;
        }
    }

    #region ����Ŀ��
    [Serializable]
    public class MonthTargetRespond
    {
        public int month_index;
        public string month_target;
    }

    [Serializable]
    public class WeekTargetRespond
    {
        public int month_Index;
        public int week_Index;
        public string week_target;
        public string eventType;
    }

    [Serializable]
    public class WeekEvent
    {
        public int event_Index;
        public string event_Type;
        public string eventDetails;
        public float correlation;
        public float occurance;
    }
    #endregion
}

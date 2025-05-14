using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static sendData;

public class API_CentralControl : MonoBehaviour
{
    public Settings settings;
    public quadVideo quad;
    public RAG_TempDialogue rag;
    [Header("��ģ̬�ӿ�")]
    public evaluateTest evaluate;
    public API_Chat api_Chat;
    public KLingAPIDemo api_Action;
    public SunoAPIDemo api_Sound;
    public API_Scene api_Scene;
    public Emoji_Control api_Demo_Emoji;
    public AvatarDriven api_AvatarDriven;

    public ImageCreatBridge api_ImageCreat;  //ͼƬ����


    [Header("���̿���")]
    public bool isAgentStart; //�򿪳���ʱ 3.31��û��

    public bool isDialogueStart;  //�Ƿ��ڽ��жԻ� �������� ϵͳ�������𣬶���

    public bool isMultiRespondStart; //��ģ̬ѡ���Ƿ�ʼ 

    public bool isEvaluateStart;

    //public bool isStartEvaluateTemp;

    public bool isSystemAwake = false;

    private TimeSpan LastSystemRespond = TimeSpan.Zero;


    private IEnumerator Start()
    {

        if (isSystemAwake)
        {
            evaluate.GenerateEvaluate();
        }
        settings.LastRespondTime = DateTime.Now;
        yield return api_AvatarDriven.CheckUpdate(GetSourceUpadate);
    }

    void GetSourceUpadate(SourceUpdate source)
    {
        if (source.show)
        {
            Debug.Log("�����³������¶�������");
        }
        else
        {
            //��ʱ����Ĭ�����ã����Ǻ�����Ҫ�����ݴ洢����ȡ�ϴ��뿪ʱ�ĳ�������
            Debug.Log("��ʱ���п��ȹ�����");
            settings.CurSceneName = "coffee_shop";
            quad.isStartPlayVideo = true;
        }
    }

    private void Update()
    {
        LastSystemRespond = DateTime.Now-settings.LastRespondTime;
        //ÿʮ���ӿ�ʼһ���������
        if (LastSystemRespond.TotalMinutes >= 1000 && !isDialogueStart && !isMultiRespondStart && !isEvaluateStart && !isSystemAwake)
        {
            isSystemAwake = true;       //�����SystemAwake 
            Debug.Log("�Զ���ʼ����״̬������");
            //TO DO �����￴����ֱ�Ӳ������� �������̼���

            //
            evaluate.GenerateEvaluate();
        }

        //��ʱ�������ҹ��µ�����
        //�٣����³����������� ���
        //�ڣ���ģ̬��Ӧ�����������ġ�������ͼ�ģ������� ���

        //��ʼ������û���
        //1����ʼʱ�Ͷ�ȡ�ļ��м�¼�����ݣ�����һ������ƥ�䡣Ȼ����һ��Dictionary��ת����Dictionary<string,SceneData> //Ҳ���Ա���List
        //2�������³�����true���Ͳ������һ�������򲥷ŵ�һ�����Ƿ�Ҫ��һ����¼������������������ͬ����setting�С�
        //3��2��ȷ���˲��ŵ�λ֮�󣬺�quadvideo��һ��ͳһ����¼cur_scene������ȷ�ϲ���list
        //4��KL������Ƶ����֡��ȡ������quadvideo�����cur_scene,������Ƶ��ȡ��Ҫ���еģ�Dictionary���ң���֡
        //5��API_Scene���޸�cur_scene���ɡ�

        //rag��һ�£�python�˵�����  ���䵽�����޺�  ������

        //�ÿ�������һ�±����--->������ߺ���  û���ɣ���������rag

        //����ͼ�Ľ�һ��
    }


    //��������CallBack
    public void EvaluateResultAccept(string result)
    {
        string newJson = JsonPatch(result);
        Debug.Log("Evaluate Result CallBack:"+ newJson);
        try
        {
            //Debug.Log(result);
            List<EResultAndSelect> eResultAndSelectList = JsonConvert.DeserializeObject<List<EResultAndSelect>>(newJson);

            //��ģ̬�ӿ�ת��
            API_Control(eResultAndSelectList[0]);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("���������Ͷ�ģ̬ѡ�� JSON ����ʧ�ܣ�" + ex.Message);
            //TO DO:��߿��Է���һ��Ĭ�ϵĶ�ģ̬״̬���ݶ�chat
        }
    }

    //Json���ݲ���
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
            //Console.WriteLine("û���ҵ��������ڵ����ݣ�Json���ݷ�����ȫʧ�ܣ�");
            Debug.Log("û���ҵ��������ڵ����ݣ�Json���ݷ�����ȫʧ�ܣ�");
            return null;
        }
    }

    private void API_Control(EResultAndSelect eResultAndSelect)
    {
        if (!isMultiRespondStart)
        {
            isMultiRespondStart = true;
            switch (eResultAndSelect.MMChoose)
            {
                case "chat":
                    //ת��Chat�ӿ�
                    Debug.Log("chat");
                    api_Chat.Mchat_API_Send(eResultAndSelect.Evaluate);
                    //api_Action.MAction_API_Send(eResultAndSelect.Evaluate);
                    //api_Sound.MSound_API_Send(eResultAndSelect.Evaluate);
                    //api_Scene.MScene_API_Send(eResultAndSelect.Evaluate);
                    break;
                case "sound":
                    Debug.Log("sound");
                    //api_Chat.Mchat_API_Send(eResultAndSelect.Evaluate);
                    //api_Action.MAction_API_Send(eResultAndSelect.Evaluate);
                    api_Sound.MSound_API_Send(eResultAndSelect.Evaluate);
                    //api_Scene.MScene_API_Send(eResultAndSelect.Evaluate);
                    break;
                case "action":
                    Debug.Log("action"); //��ʱ��û�н���prompt���޸ģ�ֻ�ǿ�������һ���µĶ���
                    //api_Chat.Mchat_API_Send(eResultAndSelect.Evaluate);
                    api_Action.MAction_API_Send(eResultAndSelect.Evaluate);
                    //api_Sound.MSound_API_Send(eResultAndSelect.Evaluate);
                    //api_Scene.MScene_API_Send(eResultAndSelect.Evaluate);
                    break;
                case "scene":
                    Debug.Log("scene");
                    //api_Chat.Mchat_API_Send(eResultAndSelect.Evaluate);
                    //api_Action.MAction_API_Send(eResultAndSelect.Evaluate);
                    //api_Sound.MSound_API_Send(eResultAndSelect.Evaluate);
                    api_Scene.MScene_API_Send(eResultAndSelect.Evaluate);
                    break;
                default:
                    Debug.Log("��ģָ̬��ʧ�ܣ�");
                    break;
            }
        }
        else
        {
            Debug.Log("��ģ̬ת���ص�!");
        }
        
    }

    
    #region ���������Ͷ�ģ̬ѡ�����
    [Serializable]
    public class EResultAndSelect
    {
        public string Evaluate;
        public string MMChoose;
    }
    #endregion
}

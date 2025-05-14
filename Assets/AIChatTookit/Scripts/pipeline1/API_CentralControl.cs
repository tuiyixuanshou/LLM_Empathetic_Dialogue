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
    [Header("多模态接口")]
    public evaluateTest evaluate;
    public API_Chat api_Chat;
    public KLingAPIDemo api_Action;
    public SunoAPIDemo api_Sound;
    public API_Scene api_Scene;
    public Emoji_Control api_Demo_Emoji;
    public AvatarDriven api_AvatarDriven;

    public ImageCreatBridge api_ImageCreat;  //图片生成


    [Header("流程控制")]
    public bool isAgentStart; //打开程序时 3.31还没用

    public bool isDialogueStart;  //是否在进行对话 被动发起 系统主动发起，都算

    public bool isMultiRespondStart; //多模态选择是否开始 

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
            Debug.Log("进行新场景、新动作设置");
        }
        else
        {
            //暂时进行默认设置，但是后面需要做数据存储，读取上次离开时的场景数据
            Debug.Log("暂时进行咖啡馆设置");
            settings.CurSceneName = "coffee_shop";
            quad.isStartPlayVideo = true;
        }
    }

    private void Update()
    {
        LastSystemRespond = DateTime.Now-settings.LastRespondTime;
        //每十分钟开始一次心理测评
        if (LastSystemRespond.TotalMinutes >= 1000 && !isDialogueStart && !isMultiRespondStart && !isEvaluateStart && !isSystemAwake)
        {
            isSystemAwake = true;       //这里的SystemAwake 
            Debug.Log("自动开始心理状态评估：");
            //TO DO ：这里看下是直接测评还是 分析长短记忆

            //
            evaluate.GenerateEvaluate();
        }

        //定时发起自我故事的内容
        //①：故事场景内容生成 完成
        //②：多模态响应：动作、配文、声音（图文？？？） 完成

        //初始化内容没完成
        //1、开始时就读取文件中记录的内容，设置一个类来匹配。然后做一个Dictionary的转化。Dictionary<string,SceneData> //也可以保留List
        //2、播放新场景是true，就播放最后一个，否则播放第一个（是否要做一个记录？？？），场景描述同步到setting中。
        //3、2中确认了播放单位之后，和quadvideo做一个统一，记录cur_scene，用于确认播放list
        //4、KL生成视频的首帧读取。听从quadvideo里面的cur_scene,但是视频读取需要类中的（Dictionary查找）首帧
        //5、API_Scene：修改cur_scene即可。

        //rag做一下，python端的连线  记忆到达上限后  做好了

        //用可灵生成一下表情包--->明天或者后天  没生成，但是做了rag

        //流程图改进一下
    }


    //心理评估CallBack
    public void EvaluateResultAccept(string result)
    {
        string newJson = JsonPatch(result);
        Debug.Log("Evaluate Result CallBack:"+ newJson);
        try
        {
            //Debug.Log(result);
            List<EResultAndSelect> eResultAndSelectList = JsonConvert.DeserializeObject<List<EResultAndSelect>>(newJson);

            //多模态接口转接
            API_Control(eResultAndSelectList[0]);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("心理评估和多模态选择 JSON 解析失败：" + ex.Message);
            //TO DO:这边可以返回一个默认的多模态状态，暂定chat
        }
    }

    //Json数据补充
    string JsonPatch(string rawText)
    {
        string pattern = @"\[.*?\]";
        Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string extractedJson = match.Value;
            Debug.Log("提取的 JSON 内容：" + extractedJson);
            return extractedJson;
        }
        else
        {
            //Console.WriteLine("没有找到中括号内的内容，Json数据返回完全失败！");
            Debug.Log("没有找到中括号内的内容，Json数据返回完全失败！");
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
                    //转接Chat接口
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
                    Debug.Log("action"); //暂时还没有进行prompt的修改，只是可以生成一个新的动作
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
                    Debug.Log("多模态指派失败！");
                    break;
            }
        }
        else
        {
            Debug.Log("多模态转接重叠!");
        }
        
    }

    
    #region 心理评估和多模态选择接收
    [Serializable]
    public class EResultAndSelect
    {
        public string Evaluate;
        public string MMChoose;
    }
    #endregion
}

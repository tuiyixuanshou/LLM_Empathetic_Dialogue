using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using System.IO;

public class Settings:Singleton<Settings>
{
    protected override void Awake()
    {
        base.Awake();
        InitializeScenesDictionary();
    }

    [Header("�û���AI����")]
    public string UserName = "�д���������";
    public string AIName = "�������С��";
    //public int Bearing = 0;
    [TextArea(5,5)]
    public string AICharacter = "�ֹ����������Ǿ����е�����";
    public Texture2D tex;
    public Sprite Headsprite;
    public bool isBule;

    //��Щ���ǳ�ʼ���Ļ���ˡ��洢���������Ѿ�д���ˡ� Ĭ�ϵ�λ�þ��ڿ��ȹ��С�
    public string Scene_Discribtion = "���ɲο�ͼ�е�С�����ڿ��ȹ��У�������ǰ��һ�����ӣ��������п��Ⱥ͵��⣬��������һ�����������׵������ϣ������ǹ�̨����̨����͸����չʾ�񣬹���������ݷø��������͵ĵ��⡣";

    public string CurSceneName;
    public Dictionary<String, Scene_Recording> Scenes_Dict = new();  // TO DO:3.31������Ҫ��ʼ����ȡ  �Ѿ���ȡ��
    
    [Header("�ϴ�����ʱ��")]
    public DateTime LastInputTime;
    public DateTime LastRespondTime;
    [Header("state threshold")]
    public float PassiveChat = 2f; //agent������Ӧ�Ի�

    [Header("�����������ݿ���")]
    public int month_Index = 1;
    public int week_Index = 1;

    #region LLM API�ӿ�����
    public string m_SetModel(ChatModel siliconModel)
    {
        switch (siliconModel)
        {
            case ChatModel.Silicon_DeepSeek_V3:
                return "deepseek-ai/DeepSeek-V3";
            case ChatModel.Silicon_Llama_3_3_70B_Instruct:
                return "meta-llama/Llama-3.3-70B-Instruct";
            case ChatModel.CUC_DeepSeek_R1:
                return "DeepSeek-R1-Distill-Qwen-32B";
            case ChatModel.Silicon_DeepSeek_R1:
                return "deepseek-ai/DeepSeek-R1";
            case ChatModel.DeepSeek_Web:
                return "deepseek-chat";
            default:
                return "meta-llama/Llama-3.3-70B-Instruct";
        }
    }

    public string m_SetUrl(LLMURL url)
    {
        switch (url)
        {
            case LLMURL.Silicon_URL:
                return "https://api.siliconflow.cn/v1/chat/completions";
            case LLMURL.CUC_Deepseek_URL:
                return "https://aihub.cuc.edu.cn/console/v1/chat/completions";
            case LLMURL.DeepSeek_URL:
                return "https://api.deepseek.com/chat/completions";
            default:
                return "https://api.siliconflow.cn/v1/chat/completions";
        }
    }

    public string m_SetApi(APIKey api)
    {
        switch (api)
        {
            case APIKey.SiliconKey:
                return "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
            case APIKey.CUCKey:
                return "sk-84b722fdcc9a41ea93917034c53457c7";
            case APIKey.DeepSeekKey:
                return "sk-0e5049d058f64e2aa17946507519ac53";
            default:
                return "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
        }
    }
    #endregion

    public Sprite ConvertToSprite(Texture2D texture)
    {
        // Rect: ���� Sprite ������Rect(0, 0, width, height) ��ʾ��������
        // Pivot: ���� Sprite �����ĵ㣨0.5f, 0.5f ��ʾ�������ģ�
        // PixelsPerUnit: ����ÿ��λ����������ͨ��Ϊ 100��
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    public List<Dictionary<string, string>> tempDialogue = new();
    // ���ʵ��ķ�����(����Start(), Awake()��������ʼ������)�������
    void InitializeScenesDictionary()
    {
        Scene_Recording firstScene = new Scene_Recording
        {
            First_Frame_Image = ReadDefaultImage("C:\\Users\\TF\\Desktop\\defualt_Image.txt"),
            Video_Links = new List<string> { "Assets\\AIChatTookit\\Video\\avatar\\Coffee\\coffee_HUG_1.mp4", "Assets\\AIChatTookit\\Video\\avatar\\Coffee\\ƾ�տ���2.mp4", "Assets\\AIChatTookit\\Video\\avatar\\Coffee\\ҡͷ����1.mp4" }
        };
        Scenes_Dict.Add("coffee_shop", firstScene);

        // ��һ����ӷ�ʽ
        Scenes_Dict["InRoomSofa"] = new Scene_Recording
        {
            First_Frame_Image = ReadDefaultImage("C:\\Users\\TF\\Desktop\\InRoomStatic.txt"),
            Video_Links = new List<string> { "Assets\\AIChatTookit\\Video\\avatar\\InRoom\\in_Room_Default1.mp4", "Assets\\AIChatTookit\\Video\\avatar\\InRoom\\in_Room_Default2.mp4", "Assets\\AIChatTookit\\Video\\avatar\\InRoom\\in_Room_Default3.mp4" }
        };
    }

    private string ReadDefaultImage(string filePath)
    {
        if (File.Exists(filePath))
        {
            // ʹ��File.ReadAllText������ȡ�ļ�����
            string fileContent = File.ReadAllText(filePath);
            return fileContent;
        }
        else
        {
            Debug.LogError("�ļ������ڣ�����·���Ƿ���ȷ��");
            return null;
        }
    }
}

[System.Serializable]
public class Scene_Recording
{
    public string First_Frame_Image;
    public List<string> Video_Links = new();
}




#region ������
public enum AIModel
{
    Ollama_Local_Llama3_1,
    Silicon_Llama_3_3_70B,
    Deepseek_chat
}
#endregion

public enum TTSs
{
    BaiduTTS, OpenAITTS, AzureTTS
}
public enum ChatModel
{
    Silicon_DeepSeek_R1,
    Silicon_DeepSeek_V3,
    Silicon_Llama_3_3_70B_Instruct,
    CUC_DeepSeek_R1,
    DeepSeek_Web
}

public enum LLMURL
{
    Silicon_URL,
    CUC_Deepseek_URL,
    DeepSeek_URL
}

public enum APIKey
{
    SiliconKey, CUCKey,DeepSeekKey
}





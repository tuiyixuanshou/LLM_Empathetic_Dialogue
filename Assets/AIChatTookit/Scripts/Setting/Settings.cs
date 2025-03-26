using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using UnityEngine;

public class Settings:Singleton<Settings>
{
    [Header("用户和AI设置")]
    public string UserName = "中传最后的温柔";
    public string AIName = "真新镇的小智";
    //public int Bearing = 0;
    [TextArea(5,5)]
    public string AICharacter = "乐观善良，但是经常有点脱线";
    public Texture2D tex;
    public Sprite Headsprite;
    public bool isBule;

    //这些都是初始化的活儿了。存储的内容我已经写好了。 默认的位置就在咖啡馆中。
    public string Scene_Discribtion = "生成参考图中的小动物在咖啡馆中，动物面前有一个桌子，桌子上有咖啡和蛋糕，动物坐在一个和桌子配套的椅子上，背后是柜台，柜台下有透明的展示柜，柜子里整理拜访各种三角型的蛋糕。";

    [Header("上次输入时间")]
    public DateTime LastInputTime;
    public DateTime LastRespondTime;
    [Header("state threshold")]
    public float PassiveChat = 2f; //agent被动响应对话

    #region LLM API接口设置
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
            default:
                return "sk-cjktrxbohzgcvvcgkeppefasertnysxdmerrowgadqkciews";
        }
    }
    #endregion

    public Sprite ConvertToSprite(Texture2D texture)
    {
        // Rect: 定义 Sprite 的区域（Rect(0, 0, width, height) 表示整个纹理）
        // Pivot: 定义 Sprite 的中心点（0.5f, 0.5f 表示纹理中心）
        // PixelsPerUnit: 纹理每单位的像素数（通常为 100）
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    public List<Dictionary<string, string>> tempDialogue = new();
}


#region 不用了
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
    CUC_DeepSeek_R1
}

public enum LLMURL
{
    Silicon_URL,
    CUC_Deepseek_URL
}

public enum APIKey
{
    SiliconKey, CUCKey
}





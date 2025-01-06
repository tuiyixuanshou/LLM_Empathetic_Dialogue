using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class Settings:Singleton<Settings>
{
    public string UserName = "中传最后的温柔";
    public string AIName = "真新镇的小智";
    [TextArea(15,20)]
    public string AICharacter = "";

    public Texture2D tex;

    public Sprite Headsprite;

    public bool isBule;


    public Sprite ConvertToSprite(Texture2D texture)
    {
        // Rect: 定义 Sprite 的区域（Rect(0, 0, width, height) 表示整个纹理）
        // Pivot: 定义 Sprite 的中心点（0.5f, 0.5f 表示纹理中心）
        // PixelsPerUnit: 纹理每单位的像素数（通常为 100）
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}



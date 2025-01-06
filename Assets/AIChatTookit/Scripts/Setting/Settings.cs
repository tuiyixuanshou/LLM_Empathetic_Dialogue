using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class Settings:Singleton<Settings>
{
    public string UserName = "�д���������";
    public string AIName = "�������С��";
    [TextArea(15,20)]
    public string AICharacter = "";

    public Texture2D tex;

    public Sprite Headsprite;

    public bool isBule;


    public Sprite ConvertToSprite(Texture2D texture)
    {
        // Rect: ���� Sprite ������Rect(0, 0, width, height) ��ʾ��������
        // Pivot: ���� Sprite �����ĵ㣨0.5f, 0.5f ��ʾ�������ģ�
        // PixelsPerUnit: ����ÿ��λ����������ͨ��Ϊ 100��
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}



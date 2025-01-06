using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvaterInteract : MonoBehaviour,IPointerClickHandler
{
    public Settings settings;
    public Image image;
    private void OnEnable()
    {
        Sprite sprite = ConvertToSprite(settings.tex);
        image.sprite = sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click Avater");
        CreatBubble(Settings.Instance.AIName, "��������", false, null);

    }
    public void CreatBubble(string name, string content, bool isRight, Sprite head)
    {
        Bubble mybubble = new Bubble(name, content, isRight, head);
        BubbleSlider.Instance.bubbles.Add(mybubble);
        BubbleSlider.Instance.CreatBubble(mybubble);
    }
    public Sprite ConvertToSprite(Texture2D texture)
    {
        // ������
        // Rect: ���� Sprite ������Rect(0, 0, width, height) ��ʾ��������
        // Pivot: ���� Sprite �����ĵ㣨0.5f, 0.5f ��ʾ�������ģ�
        // PixelsPerUnit: ����ÿ��λ����������ͨ��Ϊ 100��
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

}

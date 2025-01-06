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
        CreatBubble(Settings.Instance.AIName, "别动我啦！", false, null);

    }
    public void CreatBubble(string name, string content, bool isRight, Sprite head)
    {
        Bubble mybubble = new Bubble(name, content, isRight, head);
        BubbleSlider.Instance.bubbles.Add(mybubble);
        BubbleSlider.Instance.CreatBubble(mybubble);
    }
    public Sprite ConvertToSprite(Texture2D texture)
    {
        // 参数：
        // Rect: 定义 Sprite 的区域（Rect(0, 0, width, height) 表示整个纹理）
        // Pivot: 定义 Sprite 的中心点（0.5f, 0.5f 表示纹理中心）
        // PixelsPerUnit: 纹理每单位的像素数（通常为 100）
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

}

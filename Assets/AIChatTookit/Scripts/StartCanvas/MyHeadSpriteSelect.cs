using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyHeadSpriteSelect : MonoBehaviour
{
    public Button button;
    public Settings settings;
    public Image image;

    private void Start()
    {
        button.onClick.AddListener(SetMyHeadSprite);
    }

    void SetMyHeadSprite()
    {
        settings.Headsprite = image.sprite;
    }
}

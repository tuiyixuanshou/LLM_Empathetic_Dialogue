using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LocalImageInput : MonoBehaviour
{
    public RawImage rawImage;
    public Texture2D DefualtImage;
    Texture2D tex;
    public Button button;

    private ComfyUI_Pool pool;

    [Header("哪张图片上传")]
    [SerializeField] private bool Base_Image;
    [SerializeField] private bool Style_Image1;
    [SerializeField] private bool Style_Image2;

    private void Start()
    {
        pool = FindObjectOfType<ComfyUI_Pool>();
        button.onClick.AddListener(LoadImage);
        DisplayImage(DefualtImage);
    }

    public void DisplayImage(Texture2D preview)
    {
        rawImage.texture = preview;
    }

    public void LoadImage()
    {
        //EditorUtility.OpenFilePanel显示一个文件选择对话框
        //"Load Image"：文件选择面板的标题。
        //""：初始打开路径（为空表示默认打开项目路径）。
        //"png,jpg,jpeg"：允许选择的文件类型（扩展名）。
        string path = EditorUtility.OpenFilePanel("Load Image", "", "png,jpg,jpeg");

        //确保用户确实选择了一个文件
        if (path.Length != 0)
        {
            //读取选中的文件内容，并将其转换为字节数组
            var fileContent = System.IO.File.ReadAllBytes(path);
            Debug.Log(path);

            if(tex == null)
            {
                tex = new Texture2D(2, 2);
            }
            tex.LoadImage(fileContent);
            DisplayImage(tex);

            if (Base_Image)
            {
                pool.jsonImageDatas[0].inputs.image = path;
            }
            else if(Style_Image1)
            {
                pool.jsonImageDatas[1].inputs.image = path;
            }
            else if (Style_Image2)
            {
                pool.jsonImageDatas[2].inputs.image = path;
            }
        }
    }
}

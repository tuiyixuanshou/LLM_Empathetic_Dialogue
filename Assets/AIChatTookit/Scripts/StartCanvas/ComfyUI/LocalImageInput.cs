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

    [Header("����ͼƬ�ϴ�")]
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
        //EditorUtility.OpenFilePanel��ʾһ���ļ�ѡ��Ի���
        //"Load Image"���ļ�ѡ�����ı��⡣
        //""����ʼ��·����Ϊ�ձ�ʾĬ�ϴ���Ŀ·������
        //"png,jpg,jpeg"������ѡ����ļ����ͣ���չ������
        string path = EditorUtility.OpenFilePanel("Load Image", "", "png,jpg,jpeg");

        //ȷ���û�ȷʵѡ����һ���ļ�
        if (path.Length != 0)
        {
            //��ȡѡ�е��ļ����ݣ�������ת��Ϊ�ֽ�����
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

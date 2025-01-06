using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadLocalImage : MonoBehaviour
{
    public RawImage _rawImage;
    Texture2D tex;
    string old_imagename;
    string new_imagename;
    public string LocalPath;

    IEnumerator Start()
    {
        while (true)
        {
            LoadImageFile(LocalPath);
            yield return new WaitForSeconds(2f);
        }
    }

    void LoadImageFile(string path)
    {
        string[] pathes = Directory.GetFiles(path);
        if(pathes == null || pathes.Length == 0)
        {
            return;
        }

        new_imagename = pathes[pathes.Length-1];
        if(old_imagename == new_imagename)
        {
            if(tex != null)
            {
                Debug.Log("��ͣͼ����£���ʡ�ڴ�");
            }
            return;
        }
        byte[] data = File.ReadAllBytes(new_imagename);
        tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        _rawImage.texture = tex;
        old_imagename = new_imagename;
        Debug.Log("����ͼ��");

    }

    public void SaveTex()
    {
        Settings.Instance.tex = tex;
    }
}

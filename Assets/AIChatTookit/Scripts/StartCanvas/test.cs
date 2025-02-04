using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using static sendData;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    // Զ����Ƶ�ļ�URL
    private string imageUrl = "https://visionary-ceremony-1789-8888.east4.casdao.com/files/home/tom/ComfyUI/output/hunyun-cf_00005.png";
    // ���ر���·��
    private string localFilePath = Application.dataPath+"/Video.png";

    public RawImage image;

    void Start()
    {
        StartCoroutine(DownloadAndSaveImage(imageUrl, localFilePath));
    }

    // ����ͼƬ�����浽����·��
    IEnumerator DownloadAndSaveImage(string url, string path)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);  // ʹ�� DownloadHandlerBuffer ��ȡ�ֽ�����
        yield return request.SendWebRequest();

        // ��������Ƿ�ɹ�
        if (request.result == UnityWebRequest.Result.Success)
        {
            // ��ȡԭʼ�ֽ�����
            byte[] imageBytes = request.downloadHandler.data;
            Debug.Log(imageBytes.Length);

            // �ֶ�������������ͼƬ����
            Texture2D texture = new Texture2D(2, 2);  // �����յ�����
            texture.LoadRawTextureData(imageBytes);
            texture.Apply();
            image.texture = texture;
            Debug.Log("over");
            //bool isLoaded = texture.LoadImage(imageBytes);  // ���ֽ����ݼ���ͼƬ

            //if (isLoaded)
            //{
            //    // ������ת��Ϊ PNG ��ʽ�ֽ�����
            //    byte[] pngBytes = texture.EncodeToPNG();

            //    // ȷ������Ŀ¼����
            //    string directory = Path.GetDirectoryName(path);
            //    if (!Directory.Exists(directory))
            //    {
            //        Directory.CreateDirectory(directory);
            //    }

            //    // ���ֽ�����д�뱾���ļ�
            //    File.WriteAllBytes(path, pngBytes);
            //    Debug.Log("Image saved to: " + path);
            //}
            //else
            //{
            //    Debug.LogError("Failed to load image from bytes.");
            //}
        }
        else
        {
            Debug.LogError("Image download failed: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode);
            Debug.LogError("Response Body: " + request.downloadHandler.text);
        }
    }

}

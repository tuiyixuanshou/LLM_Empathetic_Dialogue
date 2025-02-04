using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using static sendData;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    // 远程视频文件URL
    private string imageUrl = "https://visionary-ceremony-1789-8888.east4.casdao.com/files/home/tom/ComfyUI/output/hunyun-cf_00005.png";
    // 本地保存路径
    private string localFilePath = Application.dataPath+"/Video.png";

    public RawImage image;

    void Start()
    {
        StartCoroutine(DownloadAndSaveImage(imageUrl, localFilePath));
    }

    // 下载图片并保存到本地路径
    IEnumerator DownloadAndSaveImage(string url, string path)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);  // 使用 DownloadHandlerBuffer 获取字节数据
        yield return request.SendWebRequest();

        // 检查请求是否成功
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 获取原始字节数据
            byte[] imageBytes = request.downloadHandler.data;
            Debug.Log(imageBytes.Length);

            // 手动创建纹理并加载图片数据
            Texture2D texture = new Texture2D(2, 2);  // 创建空的纹理
            texture.LoadRawTextureData(imageBytes);
            texture.Apply();
            image.texture = texture;
            Debug.Log("over");
            //bool isLoaded = texture.LoadImage(imageBytes);  // 从字节数据加载图片

            //if (isLoaded)
            //{
            //    // 将纹理转换为 PNG 格式字节数组
            //    byte[] pngBytes = texture.EncodeToPNG();

            //    // 确保保存目录存在
            //    string directory = Path.GetDirectoryName(path);
            //    if (!Directory.Exists(directory))
            //    {
            //        Directory.CreateDirectory(directory);
            //    }

            //    // 将字节数组写入本地文件
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

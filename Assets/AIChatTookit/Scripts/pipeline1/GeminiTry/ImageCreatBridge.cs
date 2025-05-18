using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageCreatBridge : MonoBehaviour
{
    public Settings settings;
    public float checkInterval = 5f;

    private string scene_string;

    /// <summary>
    /// 给后台发送prompt
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    public IEnumerator SendPrompt(string prompt)
    {
        scene_string = prompt;
        string json = $"{{\"prompt\": \"{prompt}\"}}";
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest("http://localhost:5000/send_prompt", "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Prompt sent successfully.");
        }
        else
        {
            Debug.LogError("Failed to send prompt: " + request.error);
        }
    }


    //轮询查询
    private bool imageReady = false;

    public IEnumerator CheckImageStatusRepeatedly()
    {
        yield return null;
        while (!imageReady)
        {
            UnityWebRequest request = UnityWebRequest.Get("http://localhost:5000/check_status");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log(json);
                if (json.Contains("\"ready\":true"))
                {
                    imageReady = true;
                    Debug.Log("图片已就绪！");
                    //        // 你可以在这里调用下载图片的函数
                    //        FindObjectOfType<ImageReceiver>().GetImageFromServer();
                    yield return GetImage();
                }
                else
                {
                    Debug.Log("图片尚未准备好...");
                }
            }
            else
            {
                Debug.Log("轮询失败: " + request.error);
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
        imageReady = false;
    }
    
    public RawImage displayImage;
    //获取图片
    private IEnumerator GetImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture("http://localhost:5000/get_image");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 获取下载的图片并转换为Texture2D
            Texture2D tex = DownloadHandlerTexture.GetContent(request);
            // 将Texture2D应用到RawImage组件
            displayImage.texture = tex;
            //转发Image Base64
            string base64Image = TextureToBase64(tex);
            Debug.Log("增加场景字典:");
            settings.Add_Scene(scene_string, base64Image);
            Debug.Log("生成新场景中的第一个视频");
        }
        else
        {
            Debug.LogError("Failed to get image: " + request.error);
        }
    }

    public string TextureToBase64(Texture2D texture)
    {
        byte[] textureBytes = texture.EncodeToPNG();
        return Convert.ToBase64String(textureBytes);
    }

}

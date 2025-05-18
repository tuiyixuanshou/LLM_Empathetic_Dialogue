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
    /// ����̨����prompt
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


    //��ѯ��ѯ
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
                    Debug.Log("ͼƬ�Ѿ�����");
                    //        // ������������������ͼƬ�ĺ���
                    //        FindObjectOfType<ImageReceiver>().GetImageFromServer();
                    yield return GetImage();
                }
                else
                {
                    Debug.Log("ͼƬ��δ׼����...");
                }
            }
            else
            {
                Debug.Log("��ѯʧ��: " + request.error);
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
        imageReady = false;
    }
    
    public RawImage displayImage;
    //��ȡͼƬ
    private IEnumerator GetImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture("http://localhost:5000/get_image");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // ��ȡ���ص�ͼƬ��ת��ΪTexture2D
            Texture2D tex = DownloadHandlerTexture.GetContent(request);
            // ��Texture2DӦ�õ�RawImage���
            displayImage.texture = tex;
            //ת��Image Base64
            string base64Image = TextureToBase64(tex);
            Debug.Log("���ӳ����ֵ�:");
            settings.Add_Scene(scene_string, base64Image);
            Debug.Log("�����³����еĵ�һ����Ƶ");
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

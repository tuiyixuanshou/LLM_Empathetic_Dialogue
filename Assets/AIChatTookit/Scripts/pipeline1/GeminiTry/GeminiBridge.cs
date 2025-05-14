using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GeminiBridge : MonoBehaviour
{
    [Header("UI")]
    public InputField promptInput;
    public RawImage outputImage;
    public Button sendButton;

    [Header("Server Config")]
    public string serverUrl = "http://localhost:5000/generate-image";

    private byte[] imageBytes;

    void Start()
    {
        imageBytes = File.ReadAllBytes("Assets\\AIChatTookit\\image\\default_Avatar\\default_avatar.png");
        
        sendButton.onClick.AddListener(OnSendRequest);
    }

    /// <summary>
    /// Set image data from file picker or other external input.
    /// </summary>
    public void SetImageBytes(byte[] bytes)
    {
        imageBytes = bytes;
    }

    void OnSendRequest()
    {
        if (imageBytes == null)
        {
            Debug.LogError("No image set!");
            return;
        }

        string prompt = promptInput.text.Trim();
        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogWarning("Prompt is empty.");
            return;
        }
        StartCoroutine(SendImageToServer(prompt, imageBytes));
    }

    IEnumerator SendImageToServer(string prompt, byte[] imgBytes)
    {
        WWWForm form = new WWWForm();
        form.AddField("prompt", prompt);
        form.AddBinaryData("image", imgBytes, "image.png", "image/png");

        using UnityWebRequest request = UnityWebRequest.Post(serverUrl, form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Request Failed: {request.error}");
        }
        else
        {
            var json = request.downloadHandler.text;
            GeminiImageResponse response = JsonUtility.FromJson<GeminiImageResponse>(FixJsonArray(json));
            string base64Image = null;
            foreach (var r in response.results)
            {
                if (r.type == "image")
                {
                    base64Image = r.data;
                    Debug.Log(base64Image);
                    break;
                }
            }
            if (!string.IsNullOrEmpty(base64Image))
            {
                byte[] imageData = Convert.FromBase64String(base64Image);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(imageData);
                outputImage.texture = tex;
            }

        }
    }
    // Helper: make JSON parsable by Unity (wrap array if necessary)
    string FixJsonArray(string raw)
    {
        if (!raw.StartsWith("{\"results\"")) // already valid
            return "{\"results\":" + raw + "}";
        return raw;
    }

    [Serializable]
    public class GeminiImageResponse
    {
        public Result[] results;
    }

    [Serializable]
    public class Result
    {
        public string type;
        public string data;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleFileReader : MonoBehaviour
{
    /// <summary>
    /// 异步读取 StreamingAssets 中的文件
    /// </summary>
    /// <param name="fileName">文件名（例如 "example.txt"）</param>
    /// <param name="onComplete">读取完成后回调，参数是文件内容</param>
    public void LoadFile(string fileName, Action<string> onComplete)
    {
        StartCoroutine(ReadFileCoroutine(fileName, onComplete));
    }

    private IEnumerator ReadFileCoroutine(string fileName, Action<string> onComplete)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isHttpError || request.isNetworkError)
#endif
        {
            Debug.LogError("读取失败: " + request.error);
            onComplete?.Invoke(null);
        }
        else
        {
            string content = request.downloadHandler.text;
            Debug.Log("读取成功：" + fileName);
            onComplete?.Invoke(content);
        }
    }
}

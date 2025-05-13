using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleFileReader : MonoBehaviour
{
    /// <summary>
    /// �첽��ȡ StreamingAssets �е��ļ�
    /// </summary>
    /// <param name="fileName">�ļ��������� "example.txt"��</param>
    /// <param name="onComplete">��ȡ��ɺ�ص����������ļ�����</param>
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
            Debug.LogError("��ȡʧ��: " + request.error);
            onComplete?.Invoke(null);
        }
        else
        {
            string content = request.downloadHandler.text;
            Debug.Log("��ȡ�ɹ���" + fileName);
            onComplete?.Invoke(content);
        }
    }
}

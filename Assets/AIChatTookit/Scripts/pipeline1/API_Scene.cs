using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sendData;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class API_Scene : MonoBehaviour
{
    public quadVideo quad;
    public ChangeCanvas changeCanvas;
    public API_CentralControl api_CentralControl;
    public Settings settings;
    //1.ѡ��һ���³�������ʱ�������µĳ�������Ϊapiû�п���������
    public M_Scene cur_scene;
    public M_Scene target_scene;

    [Header("��������")]
    public ChatModel Mchat_Model;
    public LLMURL Mchat_url;
    public APIKey Mchat_api;

    private string PrePrompt;
    private string SceneChangeText;

    private void Start()
    {
        PrePrompt = $@"���������ڰ���һ������С���飬������ֽ���{settings.AIName}��������Ը���{settings.AICharacter}��";
        //MScene_API_Send("�û���ǰ������Ϊƽ�ȵ�����δ������У�˯��������ܰ�ʾǱ��ѹ��");
    }

    public void MScene_API_Send(string evaluateResult)
    {
        StartCoroutine(SceneChangeProcess(evaluateResult));
    }

    IEnumerator SceneChangeProcess(string evaluateResult)
    {
        yield return null; //��һ��������APIѡ�񳡾�  ����̨�����³�������
        if (cur_scene == M_Scene.cafe)
        {
            target_scene = M_Scene.InDoor_Sofa;
        }
        else target_scene = M_Scene.cafe;  //����ֻ����ʱ������������

        yield return new WaitUntil(() => !api_CentralControl.isDialogueStart);

        //2.�������л�url��Ƶ
        //��ʼ����
        yield return changeCanvas.LoadFadeIn();
        quad.cur_Scene = target_scene;
        yield return GetText(evaluateResult);
        yield return quad.ChangePlayVideo2_URL(true);
        yield return changeCanvas.LoadFadeOut();
        cur_scene = target_scene;
        //չʾ˵�Ļ�
        api_CentralControl.api_Chat.Mchat_API_CallBack(SceneChangeText);

        api_CentralControl.isMultiRespondStart = false;
    }


    IEnumerator GetText(string evaluateResult)
    {
        string prompt = $@"�������Ѿ��л������ڳ�������{cur_scene.ToString()}�л�����{target_scene.ToString()},
��ο��û�Ŀǰ������״̬��{evaluateResult},���ɷ��ϳ����л�����������ĵ�һ��Ի������һЩ�������ճ��Ի�ϰ�߼��ɡ�";
        Debug.Log(prompt);
        List<Dictionary<string, string>> temp = new();
        var preMessage = new Dictionary<string, string>
        {
            {"role", "system" },
            {"content",PrePrompt }
        };
        var newMessage = new Dictionary<string, string>
        {
            {"role", "user" },
            {"content",prompt }
        };
        temp.Add(preMessage);
        temp.Add(newMessage);
        var payload = new
        {
            model = settings.m_SetModel(Mchat_Model),
            messages = temp,
            stream = false
        };
        string Jsonpayload = JsonConvert.SerializeObject(payload);
        yield return postRequest(settings.m_SetUrl(Mchat_url), settings.m_SetApi(Mchat_api), Jsonpayload, GetTextCallBack);
    }

    void GetTextCallBack(string text)
    {
        SceneChangeText = text;
    }



    //3.�����µĶ�����

    IEnumerator postRequest(string url, string api, string json, Action<string> callback)
    {
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + api);
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            Debug.Log("Full respond:" + uwr.downloadHandler.text);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            string response = uwr.downloadHandler.text;
            ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
            string responseJson = apiResponse.choices[0].message.content;
            Debug.Log(responseJson);
            callback(responseJson);
        }
    }
}

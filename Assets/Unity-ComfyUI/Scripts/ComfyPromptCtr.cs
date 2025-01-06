using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;


[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}
public class ComfyPromptCtr : MonoBehaviour
{
    public bool autoQueuePrompt = false;
    public float autoQueuePromptInterval = 0.2f;
    private bool onCooldown = false;
    public ComfyInputs comfyInputs;


    public event Action OnGeneratePrompt;
    

    public void SetNewPPrompt(string prompt)
    {
        comfyInputs.positivePrompt.SetPrompt(prompt);
    }

    public void SetNewNPrompt(string prompt)
    {
        comfyInputs.negativePrompt.SetPrompt(prompt);
    }

    public void ChangeDenoiseStrength(float strength)
    {
        comfyInputs.denoise.strength = strength;
    }

    public void ChangeStepsValue(float steps)
    {
        comfyInputs.steps.stepsValue = (int)steps;
    }

    private void Start()
    {
        // QueuePrompt("pretty man","watermark");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            QueuePrompt();
        }

        if (autoQueuePrompt)
        {
            QueuePrompt();
        }
    }

    IEnumerator CoolDown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(autoQueuePromptInterval);
        onCooldown = false;
    }

    public void QueuePrompt()
    {
        if (onCooldown) return;

        OnGeneratePrompt?.Invoke();

        StartCoroutine(CoolDown());

        StartCoroutine(QueuePromptCoroutine());                
    }

    private IEnumerator QueuePromptCoroutine()
    {
        string url = "http://127.0.0.1:8188/prompt";
        string promptText = GeneratePromptJson();
        promptText = promptText.Replace(comfyInputs.positivePrompt.inputID, comfyInputs.positivePrompt.prompt);
        promptText = promptText.Replace(comfyInputs.negativePrompt.inputID, comfyInputs.negativePrompt.prompt);
        promptText = promptText.Replace(comfyInputs.seed.inputID, comfyInputs.seed.GetSeed().ToString());
        promptText = promptText.Replace(comfyInputs.loadImage.inputID, comfyInputs.loadImage.ConvertToBase64(comfyInputs.loadImage.image));
        promptText = promptText.Replace(comfyInputs.denoise.inputID, ((float)Math.Round(comfyInputs.denoise.strength, 2)).ToString());
        promptText = promptText.Replace(comfyInputs.steps.inputID, comfyInputs.steps.stepsValue.ToString());
        Debug.Log(promptText);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Prompt queued successfully." + request.downloadHandler.text);

            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            Debug.Log("Prompt ID: " + data.prompt_id);
            GetComponent<ComfyWebsocket>().promptID = data.prompt_id;
            // GetComponent<ComfyImageCtr>().RequestFileName(data.prompt_id);
        }
    }


    //multiple lines of text in the inspector
    [TextArea(3, 20)]
    public string promptJson;

    private string GeneratePromptJson()
    {
        string guid = Guid.NewGuid().ToString();

        string promptJsonWithGuid = $@"
{{
    ""id"": ""{guid}"",
    ""prompt"": {promptJson}
}}";

        return promptJsonWithGuid;
    }
}





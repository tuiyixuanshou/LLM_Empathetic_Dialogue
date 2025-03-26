using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using System;
//using UnityEngine.Networking;
//using UnityEditor.VersionControl;
//using UnityEngine.UIElements;

public class WhisperContorller : MonoBehaviour
{
    [Header("Whisper")]
    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;

//    public bool streamSegments = true;
    public bool printLanguage = false;
    private string _buffer;

    [Header("UI")]
//    public Text changetext;
    public Button StartButton;
    public Button InputButton;
    public InputField inputField;

    [Header("输送辨别声音")]
    public sendData data;

    private bool isPress;
    private float holdTimeThreshold = 0.2f;
    private float holdTime = 0f;

    private void Awake()
    {
        //inputField.gameObject.SetActive(true);
        //InputButton.gameObject.SetActive(false);
    }

    void Start()
    {
        
        //inputField.gameObject.SetActive(true);
        //InputButton.gameObject.SetActive(false);

        Debug.Log("start");
        StartButton.onClick.AddListener(delegate { SwitchInputButton(); });
        //        changetext.text = "wait...";

        //whisper set
        //        whisper.OnNewSegment += OnNewSegment;
        microphoneRecord.OnRecordStop += OnRecordStop;
    }

    private void SwitchInputButton()
    {
        inputField.gameObject.SetActive(!inputField.gameObject.activeSelf);
        InputButton.gameObject.SetActive(!InputButton.gameObject.activeSelf);
    }

    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        _buffer = "";

        var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        if (res == null)
            return;

        var text = res.Result;
        //if (printLanguage)
        //    text += $"\n\nLanguage: {res.Language}";

        Debug.Log(text);

        data.GetTextAndClear(text);
        //接下来将这个text发送给千帆
        //SenttoChat(text);
    }

    public void RecordStart()
    {
        if(!microphoneRecord.IsRecording)
            microphoneRecord.StartRecord();
    }

    public void RecordStop()
    {
        if (microphoneRecord.IsRecording)
            microphoneRecord.StopRecord();
    }
}

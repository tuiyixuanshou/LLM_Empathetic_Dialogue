//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Whisper;
//using Whisper.Utils;
//using Button = UnityEngine.UI.Button;
//using Debug = UnityEngine.Debug;
//using Text = UnityEngine.UI.Text;
//using System;
//using UnityEngine.Networking;
//using UnityEditor.VersionControl;
//using UnityEngine.UIElements;
//using UnityEngine.UI;

//public class WhisperContorller : MonoBehaviour
//{
//    //whisper
//    public WhisperManager whisper;
//    public MicrophoneRecord microphoneRecord;
//    public bool streamSegments = true;
//    public bool printLanguage = false;
//    private string _buffer;

//    [Header("UI")]
//    public Text changetext;
//    public Button StartButton;

//    void Start()
//    {
//        StartButton.GetComponent<Button>().onClick.AddListener(delegate { OnButtonPressed(); });
//        changetext.text = "wait...";
//        //whisper
//        whisper.OnNewSegment += OnNewSegment;
//        microphoneRecord.OnRecordStop += OnRecordStop;
//    }

//    private void OnButtonPressed()
//    {
//        if (!microphoneRecord.IsRecording)
//        {
//            microphoneRecord.StartRecord();
//            StartButton.GetComponentInChildren<Text>().text = "Stop";
//        }
//        else
//        {
//            microphoneRecord.StopRecord();
//            StartButton.GetComponentInChildren<Text>().text = "Record test";
//        }
//    }

//    private async void OnRecordStop(AudioChunk recordedAudio)
//    {
//        StartButton.GetComponentInChildren<Text>().text = "Record Test";
//        _buffer = "";

//        var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
//        if (res == null)
//            return;

//        var text = res.Result;
//        if (printLanguage)
//            text += $"\n\nLanguage: {res.Language}";

//        changetext.text = text;
        
//        //接下来将这个text发送给千帆
//        //SenttoChat(text);
//    }

//    private void OnNewSegment(WhisperSegment segment)
//    {
//        if (!streamSegments)
//            return;

//        _buffer += segment.Text;
//        changetext.text = _buffer + "OnNew";
//    }
//}

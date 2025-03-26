using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sendSpeech : MonoBehaviour
{
    [Header("声音播放")]
    public AudioSource m_AudioSource;

    [Header("播放内容")]
    public BaiduTTS baiduTTS;
    public OpenAITTS openAITTS;
    public AzureTTS azureTTS;


    public void SpeakFunction(TTSs tts,string text)
    {
        switch (tts)
        {
            case TTSs.BaiduTTS:
                baiduTTS.Speak(text, PlayAudio);
                break;
            case TTSs.AzureTTS:
                azureTTS.Speak(text, PlayAudio);
                break;
            case TTSs.OpenAITTS:
                openAITTS.Speak(text, PlayAudio);
                break;
            default:
                azureTTS.Speak(text, PlayAudio);
                break;
        }
    }



    public void PlayAudio(AudioClip clip, string text)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
        Debug.Log("音频时长：" + clip.length);
        ////开始逐个显示返回的文本
        //StartTypeWords(_response);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sendSpeech : MonoBehaviour
{
    [Header("��������")]
    public AudioSource m_AudioSource;

    [Header("��������")]
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
        Debug.Log("��Ƶʱ����" + clip.length);
        ////��ʼ�����ʾ���ص��ı�
        //StartTypeWords(_response);
    }
}

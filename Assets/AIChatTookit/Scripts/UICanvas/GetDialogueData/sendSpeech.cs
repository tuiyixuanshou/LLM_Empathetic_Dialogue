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


    public void PlayAudio(AudioClip clip, string text)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
        Debug.Log("��Ƶʱ����" + clip.length);
        ////��ʼ�����ʾ���ص��ı�
        //StartTypeWords(_response);
        ////�л���˵������
        //SetAnimator("state", 2);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class quadVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;  // Video Player ���
    public VideoPlayer videoPlayer2;  // Video Player ���
    public Transform quadTransform;  // Quad �� Transform
    public Transform quadTransform2;  // Quad �� Transform
    //public Camera mainCamera;        // �����
    //public float adjustfloat = 2.0f;

    private float videoAspect = 9f / 16f; // 9:16 ��Ƶ����
    private float timer;
    private float TimeGap = 10f;

    public VideoClip[] videoClips;

    public M_Scene cur_Scene;

    public List<string> videoURLs;

    public List<string> coffeeURLs;

    public List<string> inRoomURLs;


    void Start()
    {
        // TO DO: Start��һ�黹����Ҫ��������һ��

        // ���� VideoPlayer Ŀ����Ⱦ�� Quad
        //videoPlayer.Play();
        //videoPlayer = gameObject.AddComponent<VideoPlayer>();
        //videoPlayer2 = gameObject.AddComponent<VideoPlayer>();
        videoPlayer2.gameObject.SetActive(false); // ��ʼʱ���صڶ��� 
        // ������Ļ
        AdjustQuadSize(quadTransform);
        AdjustQuadSize(quadTransform2);

        //RandomPlayVideo();
    }

    private void Update()
    {
        if (timer <= TimeGap)
        {
            timer += Time.deltaTime;
        }
        else
        {
            Debug.Log("ѡ�񲥷�����Ƶ");
            timer = 0;
            TimeGap = UnityEngine.Random.Range(10, 21);
            StartCoroutine(ChangePlayVideo2_URL(true));
        }
    }

    /// <summary>
    /// ÿ������ӣ������벥�Ÿ�����
    /// </summary>
    /// <param name="newVedioUrl"></param>
    public void RespondToM_Action(string newVedioUrl)
    {
        videoURLs.Add(newVedioUrl);
        timer = 0;
        TimeGap = UnityEngine.Random.Range(10, 21);
        StartCoroutine(ChangePlayVideo2_URL(false));
    }
    //������Ƶ���
    void RandomPlayVideo()
    {
        videoPlayer.Stop();  // ֹͣ��ǰ��Ƶ
        Debug.Log("��Ƶ������ֹͣ");
        int index = UnityEngine.Random.Range(0, videoClips.Length);
        videoPlayer.clip = videoClips[index];
        //videoPlayer.Play();  // �����µ���Ƶ
        // Ԥ������Ƶ
        //videoPlayer.Prepare();
        //videoPlayer.prepareCompleted += (source) =>
        //{
        //    videoPlayer.Play();  // �����µ���Ƶ
        //};
        videoPlayer.frameReady += OnFrameReady;
        videoPlayer.Prepare();
    }
    void OnFrameReady(VideoPlayer vp, long frame)
    {
        videoPlayer.frameReady -= OnFrameReady;
        videoPlayer.Play();
    }

    IEnumerator RandomPlayVideo2()
    {
        VideoPlayer current = videoPlayer;
        VideoPlayer next = videoPlayer2;
        Vector3 newPosition = videoPlayer.transform.position;
        Vector3 newPosition2 = videoPlayer2.transform.position;
        //current.gameObject.transform.SetAsLastSibling();
        newPosition.z = 0;
        newPosition2.z = 1;
        videoPlayer.transform.position = newPosition;
        videoPlayer2.transform.position = newPosition2;
        int index = UnityEngine.Random.Range(0, videoClips.Length);
        next.clip = videoClips[index];
        next.gameObject.SetActive(true);
        next.Prepare();
        next.Play();
        yield return new WaitForSeconds(0.5f);
        current.Stop();
        newPosition.z = 1;
        newPosition2.z = 0;
        videoPlayer.transform.position = newPosition;
        videoPlayer2.transform.position = newPosition2;
        current.gameObject.SetActive(false);
        //next.prepareCompleted += (source) =>
        //{
        //    next.Play();
        //    yield return new WaitForSeconds(0.5f);
        //    current.gameObject.SetActive(false);
        //};

        // ���� current �� next �Ľ�ɫ
        (videoPlayer, videoPlayer2) = (videoPlayer2, videoPlayer);
    }

    public IEnumerator ChangePlayVideo2_URL(bool isRandom)
    {
        videoURLs = SetSceneURLs();
        VideoPlayer current = videoPlayer;
        VideoPlayer next = videoPlayer2;
        Vector3 newPosition = videoPlayer.transform.position;
        Vector3 newPosition2 = videoPlayer2.transform.position;
        newPosition.z = 0;
        newPosition2.z = 1;
        videoPlayer.transform.position = newPosition;
        videoPlayer2.transform.position = newPosition2;
        int index;
        if (isRandom)
        {
            index = UnityEngine.Random.Range(0, videoURLs.Count);
        }
        else
        {
            //�¶�������֮�󲥷��¶���
            index = videoURLs.Count - 1;
        }
        next.url = videoURLs[index];
        next.gameObject.SetActive(true);
        next.Prepare();
        yield return new WaitUntil(() => next.isPrepared);
        next.Play();
        yield return new WaitForSeconds(0.7f);
        current.Stop();
        newPosition.z = 1;
        newPosition2.z = 0;
        videoPlayer.transform.position = newPosition;
        videoPlayer2.transform.position = newPosition2;
        current.gameObject.SetActive(false);
        (videoPlayer, videoPlayer2) = (videoPlayer2, videoPlayer);
    }

    List<string> SetSceneURLs()
    {
        switch (cur_Scene)
        {
            case M_Scene.cafe:
                return coffeeURLs;
            case M_Scene.InDoor_Sofa:
                return inRoomURLs;
            default:
                return coffeeURLs;
        }
    }

    //��Ļ��С����
    void AdjustQuadSize(Transform quadTransform)
    {
        // ��ȡ��ǰ��Ļ�Ŀ�߱�
        float screenAspect = (float)Screen.width / Screen.height;
        Debug.Log("screenAspect:" + screenAspect + " Screen.width:" + Screen.width + " Screen.height:" + Screen.height);
        // �����µ� Quad �ߴ�
        Vector3 newScale = quadTransform.localScale;

        if (screenAspect > videoAspect)
        {
            // ��Ļ��������߶ȣ��߶ȹ̶�
            //newScale.y = newScale.y;
            newScale.x = newScale.x * (screenAspect / videoAspect);
        }
        else
        {
            // ��Ļ��խ�������ȣ���ȹ̶�
            //newScale.x = 1f;
            newScale.y = newScale.y * (videoAspect / screenAspect);
        }

        // Ӧ�õ�����Ĵ�С
        quadTransform.localScale = newScale;

        // �� Quad ����λ�����ǰ��
        //float distanceFromCamera = adjustfloat; // ����Ϊ�ʺ��㳡����ֵ
        //quadTransform.position = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;
    }

}

public enum M_Scene{
    cafe,InDoor_Sofa,
}

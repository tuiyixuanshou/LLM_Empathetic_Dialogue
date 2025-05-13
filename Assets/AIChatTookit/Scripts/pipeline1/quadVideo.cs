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
    public Settings settings;
    public ChangeCanvas changeCanvas;
    public bool isStartPlayVideo = false;   //控制一次事件开始
    public VideoPlayer videoPlayer;  // Video Player 组件
    public VideoPlayer videoPlayer2;  // Video Player 组件
    public Transform quadTransform;  // Quad 的 Transform
    public Transform quadTransform2;  // Quad 的 Transform
    //public Camera mainCamera;        // 主相机
    //public float adjustfloat = 2.0f;

    private float videoAspect = 9f / 16f; // 9:16 视频比例
    private float timer;
    private float TimeGap = 10f;

    public VideoClip[] videoClips;

    public M_Scene cur_Scene;
    //public string cur_scene_name;
    private Scene_Recording scene;

    public List<string> curvideoURLs;

    public List<string> coffeeURLs;

    public List<string> inRoomURLs;


     IEnumerator Start()
    {
        // TO DO: Start这一块还是需要重新设置一下

        // 设置 VideoPlayer 目标渲染到 Quad
        //videoPlayer.Play();
        //videoPlayer = gameObject.AddComponent<VideoPlayer>();
        //videoPlayer2 = gameObject.AddComponent<VideoPlayer>();

        changeCanvas.canvasGroup.alpha = 1f;

        videoPlayer2.gameObject.SetActive(false); // 初始时隐藏第二个 
        // 适配屏幕
        AdjustQuadSize(quadTransform);
        AdjustQuadSize(quadTransform2);
        yield return new WaitUntil(() => isStartPlayVideo);
        //RandomPlayVideo();
        yield return ChangePlayVideo2_URL(true);
        yield return changeCanvas.LoadFadeOut();
    }

    private void Update()
    {
        if (timer <= TimeGap && isStartPlayVideo)
        {
            timer += Time.deltaTime;
        }
        else if(isStartPlayVideo)
        {
            Debug.Log("选择播放新视频");
            timer = 0;
            TimeGap = UnityEngine.Random.Range(10, 21);
            StartCoroutine(ChangePlayVideo2_URL(true));
        }
    }

    /// <summary>
    /// 每次新添加，都必须播放该内容
    /// </summary>
    /// <param name="newVedioUrl"></param>
    public void RespondToM_Action(string newVedioUrl)
    {
        //curvideoURLs.Add(newVedioUrl);
        //TO DO:同步到改场景的List中。 3.31已完成
        settings.Scenes_Dict[settings.CurSceneName].Video_Links.Add(newVedioUrl);
        timer = 0;
        TimeGap = UnityEngine.Random.Range(10, 21);
        StartCoroutine(ChangePlayVideo2_URL(false));
    }
    //播放视频随机
    void RandomPlayVideo()
    {
        videoPlayer.Stop();  // 停止当前视频
        Debug.Log("视频播放已停止");
        int index = UnityEngine.Random.Range(0, videoClips.Length);
        videoPlayer.clip = videoClips[index];
        //videoPlayer.Play();  // 播放新的视频
        // 预加载视频
        //videoPlayer.Prepare();
        //videoPlayer.prepareCompleted += (source) =>
        //{
        //    videoPlayer.Play();  // 播放新的视频
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

        // 交换 current 和 next 的角色
        (videoPlayer, videoPlayer2) = (videoPlayer2, videoPlayer);
    }

    public IEnumerator ChangePlayVideo2_URL(bool isRandom)
    {
        scene = null;
        settings.Scenes_Dict.TryGetValue(settings.CurSceneName, out scene);
        //TO DO:这里需要搞一下如果scene为null怎么办。
        curvideoURLs = scene.Video_Links;

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
            index = UnityEngine.Random.Range(0, curvideoURLs.Count);
        }
        else
        {
            //新动作增加之后播放新动作
            index = curvideoURLs.Count - 1;
        }
        next.url = curvideoURLs[index];
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

    //屏幕大小适配
    void AdjustQuadSize(Transform quadTransform)
    {
        // 获取当前屏幕的宽高比
        float screenAspect = (float)Screen.width / Screen.height;
        Debug.Log("screenAspect:" + screenAspect + " Screen.width:" + Screen.width + " Screen.height:" + Screen.height);
        // 计算新的 Quad 尺寸
        Vector3 newScale = quadTransform.localScale;

        if (screenAspect > videoAspect)
        {
            // 屏幕更宽，适配高度，高度固定
            //newScale.y = newScale.y;
            newScale.x = newScale.x * (screenAspect / videoAspect);
        }
        else
        {
            // 屏幕更窄，适配宽度，宽度固定
            //newScale.x = 1f;
            newScale.y = newScale.y * (videoAspect / screenAspect);
        }

        // 应用调整后的大小
        quadTransform.localScale = newScale;

        // 让 Quad 正好位于相机前方
        //float distanceFromCamera = adjustfloat; // 调整为适合你场景的值
        //quadTransform.position = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;
    }

}

public enum M_Scene{
    cafe,InDoor_Sofa,
}

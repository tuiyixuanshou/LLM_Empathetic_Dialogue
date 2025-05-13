using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCanvas : MonoBehaviour
{
    public List<Canvas> SceneCanvas = new();
    [Header("¹ý¶Écanvas")]
    public Canvas LoadingCanvas;


    public Canvas DialogueCanvas;
    public Canvas StartCanvas;

    public CanvasGroup canvasGroup;
    private void Awake()
    {

    }

    private void Start()
    {
        //canvasGroup = LoadingCanvas.GetComponent<CanvasGroup>();
    }

    public void CanvasChange(Canvas targetCanvas)
    {

        if (!SceneCanvas.Contains(targetCanvas))
        {
            Debug.LogWarning("targetCanvas not in List!");
            return; 
        }

        StartCoroutine(LoadFade(targetCanvas));
    }

   
    IEnumerator LoadFade(Canvas targetCanvas)
    {
        yield return LoadFadeIn();

        foreach (Canvas canvas in SceneCanvas)
        {
            canvas.gameObject.SetActive(canvas == targetCanvas);
        }
        yield return new WaitForSeconds(1f);


        yield return LoadFadeOut();
    }

    //´¿ºÚÆÁ³öÏÖ
    public IEnumerator LoadFadeIn()
    {
        canvasGroup.alpha = 0;
        LoadingCanvas.gameObject.SetActive(true);
        float a = canvasGroup.alpha;
        while (a < 0.8f)
        {
            a += 0.05f;
            canvasGroup.alpha = a;
            yield return new WaitForSeconds(0.1f);
        }
        canvasGroup.alpha = 1;
    }

    //´¿ºÚÆÁÀë¿ª
    public IEnumerator LoadFadeOut()
    {
        canvasGroup.alpha = 1;
        float b = canvasGroup.alpha;
        while (b > 0.2f)
        {
            b -= 0.05f;
            canvasGroup.alpha = b;
            yield return new WaitForSeconds(0.1f);
        }
        canvasGroup.alpha = 0;
        LoadingCanvas.gameObject.SetActive(false);
    }



    public void StartChangeCanvas(Canvas targetCanvas)
    {
        LoadingCanvas.GetComponent<CanvasGroup>().alpha = 0;
        LoadingCanvas.gameObject.SetActive(true);
        StartCoroutine(FadeInAndOut(targetCanvas));
    }

    IEnumerator FadeInAndOut(Canvas targetCanvas)
    {
        CanvasGroup canvasGroup = LoadingCanvas.GetComponent<CanvasGroup>();
        float a = canvasGroup.alpha;
        while (a < 0.9f)
        {
            a += 0.05f;
            canvasGroup.alpha = a;
            yield return new WaitForSeconds(0.1f);
        }
        canvasGroup.alpha = 1;
        yield return new WaitForSeconds(2f);
        targetCanvas.gameObject.SetActive(false);
        DialogueCanvas.gameObject.SetActive(true);


        float b = canvasGroup.alpha;
        while (b > 0.05f)
        {
            b -= 0.05f;
            canvasGroup.alpha = b;
            yield return new WaitForSeconds(0.1f);
        }
        canvasGroup.alpha = 0;
        LoadingCanvas.gameObject.SetActive(false);


    }

}

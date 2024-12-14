using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCanvas : MonoBehaviour
{
    public Canvas LoadingCanvas;
    public Canvas DialogueCanvas;
    public Canvas StartCanvas;

    private void Start()
    {
        
    }

    public void StartChangeCanvas()
    {
        LoadingCanvas.GetComponent<CanvasGroup>().alpha = 0;
        LoadingCanvas.gameObject.SetActive(true);
        StartCoroutine(FadeInAndOut());
    }

    IEnumerator FadeInAndOut()
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
        StartCanvas.gameObject.SetActive(false);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputButtonLongPress : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public WhisperContorller whisperContorller;
    public Button InputButton;
    private bool isPress;
    private float holdTimeThreshold = 0.2f;
    private float holdTime = 0f;
    public void OnPointerDown(PointerEventData eventData)
    {
        isPress = true;
        holdTime = 0f;
        InputButton.GetComponentInChildren<Text>().text = "松开 发送";
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPress = false;
        if(holdTime >= holdTimeThreshold)
        {
            whisperContorller.RecordStop();
        }
        else
        {
            Debug.Log("未启动");
        }
        InputButton.GetComponentInChildren<Text>().text = "按住 说话";
    }


    // Update is called once per frame
    void Update()
    {
        if (isPress)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= holdTimeThreshold)
            {
                whisperContorller.RecordStart();
            }
        }
    }
}

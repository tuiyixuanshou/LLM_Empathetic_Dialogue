using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CheakGeneralize : MonoBehaviour
{
    public sendData AIData;

    private DateTime LastTime;

    private int oldDialogueAmount = 0;

    private IEnumerator Start()
    {
        while (true)
        {
            if(AIData.tempDialogue.Count != 0 && AIData.tempDialogue.Count == oldDialogueAmount)
            {
                //有内容
                DateTime now = DateTime.Now;

                TimeSpan difference = now - LastTime;

                // 检查差值是否等于五分钟
                if (difference.TotalMinutes >= 5)
                {
                    Debug.Log("已经过去五分钟了！");
                    //开始总结
                    AIData.StartGenerilize();
                }
            }
            else
            {
                oldDialogueAmount = AIData.tempDialogue.Count;
                LastTime = DateTime.Now;
            }
            yield return new WaitForSeconds(30f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memory_Control : MonoBehaviour
{
    public List<Dictionary<string, string>> shortMemory = new();

    public void AddToShortMemory(Dictionary<string, string> newEntry)
    {
        shortMemory.Add(newEntry);
        //确保列表最多只有 20 条记录
        if (shortMemory.Count > 20)
        {
            shortMemory.RemoveAt(0); // 移除第一条（最旧的一条）
        }
    }
}

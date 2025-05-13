using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memory_Control : MonoBehaviour
{
    public List<Dictionary<string, string>> shortMemory = new();

    public void AddToShortMemory(Dictionary<string, string> newEntry)
    {
        shortMemory.Add(newEntry);
        //ȷ���б����ֻ�� 20 ����¼
        if (shortMemory.Count > 20)
        {
            shortMemory.RemoveAt(0); // �Ƴ���һ������ɵ�һ����
        }
    }
}

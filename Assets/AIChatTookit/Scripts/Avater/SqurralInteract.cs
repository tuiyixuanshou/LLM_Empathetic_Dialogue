using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SqurralInteract : MonoBehaviour
{
    public float moveSpeed;
    void Update()
    {
        // ����Ƿ��д�������
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // ��ȡ��һ��������

            // �жϴ����Ƿ��ǿ�ʼ�׶�
            if (touch.phase == TouchPhase.Began)
            {
                // �Ӵ����㴴��һ������
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                // ��������Ƿ�����˶���
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Debug.Log("Avatar was touched!");
                        //OnAvatarTouched();
                        ChangePosition();
                        CreatBubble(Settings.Instance.AIName, "��������" , false, null);
                    }
                }
            }
        }
    }
    public void CreatBubble(string name, string content, bool isRight, Sprite head)
    {
        Bubble mybubble = new Bubble(name, content, isRight, head);
        BubbleSlider.Instance.bubbles.Add(mybubble);
        BubbleSlider.Instance.CreatBubble(mybubble);
    }
    // ���� Avatar ����߼�
    void OnAvatarTouched()
    {
        Debug.Log("You touched the Avatar!");
    }

    private Vector3 newPosition;
    void ChangePosition()
    {
        newPosition = GetRandomPositionInScreen();
        Debug.Log(newPosition);
        StartCoroutine(moveToRandom(newPosition));
        //transform.position = newPosition;
    }

    Vector3 GetRandomPositionInScreen()
    {
        float screenX = Random.Range(0f, Screen.width);
        float screenY = Random.Range(0f, Screen.height);

        Vector3 randomScreenPosition = new Vector3(screenX, screenY, Camera.main.nearClipPlane + 5f);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(randomScreenPosition);
        worldPosition = new Vector3(worldPosition.x, worldPosition.y, gameObject.transform.position.z);
        return worldPosition;
    }

    IEnumerator moveToRandom(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;

    }
}

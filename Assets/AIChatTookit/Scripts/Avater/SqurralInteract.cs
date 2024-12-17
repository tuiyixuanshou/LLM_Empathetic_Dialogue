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
        // 检测是否有触摸输入
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // 获取第一个触摸点

            // 判断触摸是否是开始阶段
            if (touch.phase == TouchPhase.Began)
            {
                // 从触摸点创建一条射线
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                // 检测射线是否击中了对象
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Debug.Log("Avatar was touched!");
                        //OnAvatarTouched();
                        ChangePosition();
                        CreatBubble(Settings.Instance.AIName, "别动我啦！" , false, null);
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
    // 触摸 Avatar 后的逻辑
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

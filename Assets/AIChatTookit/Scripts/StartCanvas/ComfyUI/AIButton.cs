using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIButton : MonoBehaviour
{
    public Button button;
    [SerializeField]private ComfyUI_Pool pool;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(pool.Delivery_task);
    }

}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HistoryButton : MonoBehaviour
{
    public Button button;
    public GameObject HistoryPanel;
    private void Awake()
    {
        button.onClick.AddListener(delegate { SetHistoryPanelActive(); });  
    }

    private void SetHistoryPanelActive()
    {
        HistoryPanel.SetActive(!HistoryPanel.activeInHierarchy);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    public Button thisButton;
    public Canvas targetCanvas;

    public ChangeCanvas changeCanvas;

    private void Start()
    {
        changeCanvas = FindObjectOfType<ChangeCanvas>();

        thisButton.onClick.AddListener(ChangeCanvas);
    }

    private void ChangeCanvas()
    {
        changeCanvas.CanvasChange(targetCanvas);
    }
}

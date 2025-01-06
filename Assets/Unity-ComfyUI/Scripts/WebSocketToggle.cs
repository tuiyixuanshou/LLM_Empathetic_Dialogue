using UnityEngine;
using UnityEngine.UI;

public class WebSocketToggle : MonoBehaviour
{
    public ComfyWebsocket websocket;
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        websocket.OnConnectionChange += UpdateToggleState;
        toggle.onValueChanged.AddListener(ToggleWebSocket);
    }

    private void OnDestroy()
    {
        websocket.OnConnectionChange -= UpdateToggleState;
        toggle.onValueChanged.RemoveListener(ToggleWebSocket);
    }

    private void UpdateToggleState(bool isConnected)
    {
        toggle.isOn = isConnected;
    }

    private void ToggleWebSocket(bool isOn)
    {
        if (isOn)
        {
            websocket.Connect();
        }
        else
        {
            websocket.Disconnect();
        }
    }
}

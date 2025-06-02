using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class WebSocketMessageReceiver : MonoBehaviour
{
    [SerializeField] WebSocketConnectUI webSocketConnectUI;
    [SerializeField] private GameObject connectPanel; // ��J�A�� UI ����
    [SerializeField] private ReconnectPanelController reconnectUI; // �O�o�b Inspector ����

    [SerializeField] private WebSocketClient webSocketClient;
    public bool CanReceiveBrushMessage = true;

    [System.Serializable]
    public class BrushUpdateEvent : UnityEvent<List<BrushData>> { }
    public BrushUpdateEvent OnBrushDataReceived = new();

    private void Start()
    {
        webSocketClient.OnMessageReceive.AddListener(ReceiveMessage);
        webSocketClient.OnConnected.AddListener(OnWebSocketConnected);
        webSocketClient.OnConnectionError.AddListener(webSocketConnectUI.OnConnectionFaild);
        webSocketClient.OnDisconnected.AddListener(OnWebSocketDisconnected);
    }
    private void OnWebSocketDisconnected()
    {
        if (!connectPanel.activeSelf) // �u���b�C�����~�Ұ� reconnect
        {
            reconnectUI.ShowFlicker(); // ����_�u����
            webSocketClient.allowReconnect = true;
            webSocketClient.isReconnectAttempt = true;
            Debug.Log("���u���A�۰ʱҥέ��s����");
        }
        else
        {
            Debug.Log("ConnectPanel �}�Ҥ��A���۰ʭ��s");
        }
    }

    private void OnWebSocketConnected()
    {
        reconnectUI.ShowSuccessAndHide(); // ��ܦ��\���ܨæ۰ʦ��_

        if (connectPanel != null)
        {
            connectPanel.SetActive(false);
        }

        webSocketClient.allowReconnect = false;

        if (webSocketClient.isReconnectAttempt)
        {
            Debug.Log("���s�s�u���\");
            webSocketClient.isReconnectAttempt = false; // reset flag
        }
    }

    private void ReceiveMessage(string messageContent)
    {
        if (!CanReceiveBrushMessage) return;

        try
        {
            var newBrushMessage = JsonConvert.DeserializeObject<BrushPositionJson>(messageContent);
            if (newBrushMessage?.data != null)
            {
                OnBrushDataReceived.Invoke(newBrushMessage.data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Can't deserialize message: {messageContent}. Error: {e.Message}");
        }
    }

    /// <summary>
    /// ���� UI �s�u��J�ΡA�榡�GIP:Port
    /// </summary>
    public void ConnectToServer(string ip, string port)
    {
        string address = $"ws://{ip}:{port}";
        Debug.Log($"[WebSocketReceiver] Connecting to: {address}");

        webSocketClient.CloseConnection();         // ����������{���s�u
        webSocketClient.StartConnection(address);  // �A�إ߷s�s�u
    }

    // ���դ�ʰe�J��ƥ�
    public void SendMessageManually(string json)
    {
        ReceiveMessage(json);
    }
}

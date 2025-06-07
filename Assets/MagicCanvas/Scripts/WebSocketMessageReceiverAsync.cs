using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WebSocketMessageReceiverAsync : MonoBehaviour
{
    [Header("�s�� UI ����")]
    [SerializeField] private WebSocketConnectUI webSocketConnectUI;
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private ReconnectPanelController reconnectUI;

    [Header("WebSocket �Ȥ��")]
    [SerializeField] private WebSocketClient webSocketClient;

    [Header("�O�_���\�B�z Brush ���")]
    public bool CanReceiveBrushMessage = true;

    [System.Serializable]
    public class BrushUpdateEvent : UnityEvent<List<BrushData>> { }
    [Header("���� Brush ��ƨƥ�")]
    public BrushUpdateEvent OnBrushDataReceived = new();
    private readonly ConcurrentQueue<List<BrushData>> mainThreadQueue = new();
    private int messagesProcessedThisSecond = 0;
    private float messageTimer = 0f;

    private void Start()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnMessageReceive.AddListener(message =>
            {
                //receivedCountThisSecond++;
                ReceiveMessage(message);
            });
            webSocketClient.OnConnected.AddListener(OnWebSocketConnected);
            webSocketClient.OnConnectionError.AddListener(webSocketConnectUI.OnConnectionFaild);
            webSocketClient.OnDisconnected.AddListener(OnWebSocketDisconnected);
        }
    }

    private void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    // 1. �إ߼��� BrushData �� JSON �r��
        //    Vector2 mousePos = Input.mousePosition;
        //    Vector2 normalized = new Vector2(
        //        mousePos.x / Screen.width,
        //        1f - (mousePos.y / Screen.height)
        //    );

        //    string json = $"{{\"data\":[{{\"roller_id\":0,\"point\":[{normalized.x},{normalized.y}]}}]}}";
        //    Debug.Log("normalized.y :" + normalized.y);
        //    // 2. �I�s WebSocketMessageReceiverAsync �B�z JSON
        //    SendMessageManually(json);

        //    //scratchCount++;
        //}
        int processed = 0;
        {//����C�V��Ƶ���
            //int maxProcessPerFrame = 128;
            //while (processed < maxProcessPerFrame && mainThreadQueue.TryDequeue(out var brushList))
            //{
            //    OnBrushDataReceived.Invoke(brushList);

            //    processed++;
            //}
        }

        {//������C�V��Ƶ���
            while (mainThreadQueue.TryDequeue(out var brushList))
            {
                OnBrushDataReceived.Invoke(brushList);

                processed++;
            }
        }

        // �ʱ�
        messageTimer += Time.deltaTime;
        if (messageTimer >= 1f)
        {
            Debug.Log($"[�ʱ�] ���V�B�z {processed} �� brushList�CQueue �Ѿl�G{mainThreadQueue.Count}");
            messageTimer = 0f;
        }
    }

    private void ReceiveMessage(string messageContent)
    {
        //Debug.Log("[ReceiveMessage] �禡���Q�I�s");

        if (!CanReceiveBrushMessage)
        {
            Debug.LogWarning("�����T���GCanReceiveBrushMessage �� false");
            return;
        }

        //Debug.Log($"[ReceiveMessage] ����T�����e�G{messageContent}");

        try
        {
            var newBrushMessage = JsonConvert.DeserializeObject<BrushPositionJson>(messageContent);
            if (newBrushMessage == null)
            {
                Debug.LogError("[ReceiveMessage] JSON �ѪR���ѡI");
                return;
            }

            if (newBrushMessage.data == null || newBrushMessage.data.Count == 0)
            {
                Debug.LogWarning("[ReceiveMessage] JSON �ѪR���\�� data ����");
                return;
            }

            //Debug.Log($"[ReceiveMessage] ���\�ѪR�A�@ {newBrushMessage.data.Count} �� BrushData");
            mainThreadQueue.Enqueue(newBrushMessage.data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Can't deserialize message: {messageContent}. Error: {e.Message}");
        }
    }

    private void OnWebSocketDisconnected()
    {
        if (!connectPanel.activeSelf)
        {
            reconnectUI?.ShowFlicker();
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
        reconnectUI?.ShowSuccessAndHide();

        if (connectPanel != null)
        {
            connectPanel.SetActive(false);
        }

        webSocketClient.allowReconnect = false;

        if (webSocketClient.isReconnectAttempt)
        {
            Debug.Log("���s�s�u���\");
            webSocketClient.isReconnectAttempt = false;
        }
    }

    /// <summary>
    /// ���� UI ��J�s�u��T�����f
    /// </summary>
    public void ConnectToServer(string ip, string port)
    {
        string address = $"ws://{ip}:{port}";
        Debug.Log($"[WebSocketReceiverAsync] Connecting to: {address}");

        webSocketClient.CloseConnection();
        webSocketClient.StartConnection(address);
    }

    public void SendMessageManually(string json)
    {
        Debug.Log($"[Receiver] SendMessageManually �I�s��");

        if (!CanReceiveBrushMessage)
        {
            //Debug.LogWarning("[Receiver] �����T���GCanReceiveBrushMessage �� false");
            return;
        }

        ReceiveMessage(json);
    }
}

// class BrushPositionJson { public List<BrushData> data; }
// class BrushData { public int roller_id; public List<float> point; }

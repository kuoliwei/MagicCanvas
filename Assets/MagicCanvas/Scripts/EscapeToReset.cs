using UnityEngine;

public class EscapeToReset : MonoBehaviour
{
    public WebSocketClient webSocketClient;
    public GameObject connectPanel;
    public ScratchCard scratchCard;
    public GameObject reconnectPanel; // ReconnectPanel�]��ӭ��O�^
    public ReconnectPanelController reconnectUI; // �i��A�Ψ����� coroutine ��

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("���U ESC�G�j���^�D�e��");

            // 1. ��������۰ʭ��s����
            webSocketClient.allowReconnect = false;
            webSocketClient.isReconnectAttempt = false;
            webSocketClient.CloseConnection();

            // 2. ���� ReconnectPanel�]�p�G�٦b��ܡ^
            if (reconnectPanel != null && reconnectPanel.activeSelf)
            {
                reconnectPanel.SetActive(false);
            }

            // 3. ����{�{��{
            if (reconnectUI != null)
            {
                reconnectUI.CancelAndHideImmediately(); // ���T�a����{�{�P pending ������{
            }

            // 4. ��� ConnectPanel
            connectPanel.SetActive(true);

            // 5. ���m ScratchCard �e��
            scratchCard.ResetScratch();
        }
    }
}

using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;

public class MouseTriggeredJsonFeeder : MonoBehaviour
{
    public WebSocketMessageReceiverAsync receiver;
    public int simulatedMessagesPerSecond = 100;

    private CancellationTokenSource cts;

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    StartFeeding();
        //}

        //if (Input.GetMouseButtonUp(0))
        //{
        //    StopFeeding();
        //}
    }

    public void StartFeeding()
    {
        Debug.Log("StartFeeding ����");
        if (receiver == null || cts != null) return;

        cts = new CancellationTokenSource();
        var token = cts.Token;
        //Debug.Log($"[Feeder] �}�l��J���ո�ơA�C�� {simulatedMessagesPerSecond} ��");
        Debug.Log("token.IsCancellationRequested : " + token.IsCancellationRequested);
        Task.Run(async () =>
        {
            Debug.Log("�D�P�B ����");
            int intervalMS = Mathf.Max(1, 1000 / simulatedMessagesPerSecond);

            while (!token.IsCancellationRequested)
            {
                //System.Random rand = new System.Random();
                //Debug.Log("while ����");
                //float x = (float)rand.NextDouble();
                //Debug.Log("x ���ü�");
                //float y = (float)rand.NextDouble();
                //Debug.Log("y ���ü�");
                string json = $"{{\"data\":[{{\"roller_id\":0,\"point\":[{0.5f},{0.5f}]}}]}}";
                //Debug.Log("���� json");
                //Debug.Log($"[Feeder] SendMessageManually(json) �I�s���G{json}");
                //Debug.Log("SendMessageManually ���e");
                receiver.SendMessageManually(json);
                //Debug.Log("SendMessageManually ����");
                await Task.Delay(intervalMS, token);
            }
            //Debug.Log("[Feeder] �w������J");
        }, token);
        Debug.Log("StartFeeding ����");
    }

    public void StopFeeding()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts = null;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ScratchCard : MonoBehaviour, IPointerDownHandler
{
    public Sprite maskImage;
    public RawImage rawImage;
    public Texture brushTexture;
    public Material eraseMaterial;
    public float brushSize = 64f;

    [Header("�i���]�w")]
    public float clearThreshold = 0.6f;
    public float restoreSpeed = 1f;

    private RenderTexture renderTex;
    private RectTransform rect;
    private Texture2D croppedTex;
    private bool isFullyRevealed = false;

    public System.Action OnFullyRevealed;
    private readonly Queue<Vector2> uvQueue = new();
    [SerializeField] int maxUvPerFrame = 32;
    private void Start()
    {
        //rawImage.material = eraseMaterial;
        rect = rawImage.rectTransform;
        renderTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        renderTex.Create();

        ResetScratch();
        rawImage.texture = renderTex;
    }

    public void ResetScratch()
    {
        isFullyRevealed = false;

        if (renderTex == null || maskImage == null)
        {
            Debug.LogWarning("RenderTex �� maskImage �� null�A�L�k���m");
            return;
        }

        Texture2D tex = maskImage.texture;
        Rect spriteRect = maskImage.rect;

        croppedTex = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
        croppedTex.SetPixels(tex.GetPixels(
            (int)spriteRect.x,
            (int)spriteRect.y,
            (int)spriteRect.width,
            (int)spriteRect.height
        ));
        croppedTex.Apply();

        Graphics.Blit(croppedTex, renderTex);
        rawImage.texture = renderTex;
    }

    private void DrawTransparentAt(Vector2 uv)
    {
        if (brushTexture == null || eraseMaterial == null || renderTex == null) return;

        eraseMaterial.SetTexture("_MainTex", renderTex);
        eraseMaterial.SetTexture("_BrushTex", brushTexture);
        eraseMaterial.SetVector("_BrushPos", new Vector4(
            uv.x, uv.y,
            brushSize / renderTex.width,
            brushSize / renderTex.height
        ));

        RenderTexture tempRT = RenderTexture.GetTemporary(renderTex.width, renderTex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(renderTex, tempRT, eraseMaterial);
        Graphics.Blit(tempRT, renderTex);
        RenderTexture.ReleaseTemporary(tempRT);

        rawImage.texture = renderTex;

        if (!isFullyRevealed && GetClearedRatio() >= clearThreshold)
        {
            ShowFullImage();
        }
    }
    public void DrawTransparentAt(List<Vector2> uvList)
    {
        //{//�h��blit
        //    if (brushTexture == null || eraseMaterial == null || renderTex == null || uvList == null || uvList.Count == 0)
        //        return;

        //    RenderTexture tempRT = RenderTexture.GetTemporary(renderTex.width, renderTex.height, 0, RenderTextureFormat.ARGB32);
        //    Graphics.Blit(renderTex, tempRT); // ���ƻs�ثe�B�n���A

        //    eraseMaterial.SetTexture("_MainTex", tempRT);
        //    eraseMaterial.SetTexture("_BrushTex", brushTexture);

        //    foreach (var uv in uvList)
        //    {
        //        eraseMaterial.SetVector("_BrushPos", new Vector4(
        //            uv.x, uv.y,
        //            brushSize / renderTex.width,
        //            brushSize / renderTex.height
        //        ));

        //        Graphics.Blit(tempRT, renderTex, eraseMaterial);

        //        // �� renderTex �A�ƻs�^ tempRT�A���U�����b�̷s���A�W
        //        Graphics.Blit(renderTex, tempRT);
        //    }

        //    RenderTexture.ReleaseTemporary(tempRT);
        //    rawImage.texture = renderTex;

        //    if (!isFullyRevealed && GetClearedRatio() >= clearThreshold)
        //    {
        //        ShowFullImage();
        //    }
        //}
        {//�榸blit
            if (brushTexture == null || eraseMaterial == null || renderTex == null || uvList == null || uvList.Count == 0)
                return;

            eraseMaterial.SetTexture("_MainTex", renderTex);
            eraseMaterial.SetTexture("_BrushTex", brushTexture);
            eraseMaterial.SetFloat("_AlphaDecayFactor", 0.1f);

            const int MaxBrushCount = 10;
            Vector4[] brushMultiPos = new Vector4[MaxBrushCount];

            int count = Mathf.Min(uvList.Count, MaxBrushCount);
            float sizeX = brushSize / renderTex.width;
            float sizeY = brushSize / renderTex.height;

            for (int i = 0; i < count; i++)
            {
                Vector2 uv = uvList[i];
                brushMultiPos[i] = new Vector4(uv.x, uv.y, sizeX, sizeY);
            }

            eraseMaterial.SetVectorArray("_BrushMultiPos", brushMultiPos);

            RenderTexture tempRT = RenderTexture.GetTemporary(renderTex.width, renderTex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(renderTex, tempRT, eraseMaterial);
            Graphics.Blit(tempRT, renderTex);
            RenderTexture.ReleaseTemporary(tempRT);

            rawImage.texture = renderTex;

            if (!isFullyRevealed && GetClearedRatio() >= clearThreshold)
            {
                ShowFullImage();
            }
        }
    }
    public float GetClearedRatio()
    {
        Texture2D tempTex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTex;
        tempTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tempTex.Apply();
        RenderTexture.active = null;

        Color32[] pixels = tempTex.GetPixels32();
        int cleared = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 2) cleared++;
        }

        Destroy(tempTex);
        return (float)cleared / pixels.Length;
    }

    public void ShowFullImage()
    {
        isFullyRevealed = true;

        Texture2D clearTex = new Texture2D(renderTex.width, renderTex.height);
        Color32[] pixels = new Color32[renderTex.width * renderTex.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);
        clearTex.SetPixels32(pixels);
        clearTex.Apply();

        Graphics.Blit(clearTex, renderTex);
        Destroy(clearTex);

        rawImage.texture = renderTex;
        OnFullyRevealed?.Invoke();
    }

    public IEnumerator SmoothRestoreMask()
    {
        if (croppedTex == null || renderTex == null) yield break;

        Texture2D restoreTex = new Texture2D(croppedTex.width, croppedTex.height);
        restoreTex.SetPixels(croppedTex.GetPixels());
        restoreTex.Apply();

        float t = 0;
        while (t < 1f)
        {
            for (int y = 0; y < restoreTex.height; y++)
            {
                for (int x = 0; x < restoreTex.width; x++)
                {
                    Color c = croppedTex.GetPixel(x, y);
                    c.a *= t;
                    restoreTex.SetPixel(x, y, c);
                }
            }
            restoreTex.Apply();

            Graphics.Blit(restoreTex, renderTex);
            t += Time.deltaTime * restoreSpeed;
            yield return null;
        }

        Graphics.Blit(croppedTex, renderTex);
        Destroy(restoreTex);
    }

    public void EraseAtScreenPosition(Vector2 screenPos)
    {
        if (rect == null) rect = rawImage.rectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out Vector2 localPos))
        {
            Vector2 uv = new Vector2(
                (localPos.x + rect.rect.width * 0.5f) / rect.rect.width,
                (localPos.y + rect.rect.height * 0.5f) / rect.rect.height
            );
            //Debug.Log("uv : " + uv.x + ", " + uv.y);
            uvQueue.Enqueue(uv); // �אּ�[�J queue
        }
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        //Vector2 screenPos = eventData.position;
        //EraseAtScreenPosition(screenPos);
    }
    // �C������Ʋέp
    public WebSocketMessageReceiverAsync receiver;
    private int scratchCount = 0;
    private float timer = 0f;
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
        //    receiver.SendMessageManually(json);

        //    //scratchCount++;
        //}

        //timer += Time.deltaTime;
        //if (timer >= 1f)
        //{
        //    Debug.Log($"[ScratchCardTest] �C������ơ]�z�L WebSocketMessageReceiver -> ReceiveMessage�^�G{scratchCount}");
        //    scratchCount = 0;
        //    timer = 0f;
        //}
        //{//�@�V�B�̤@��uv
        //    int maxPerFrame = 10;
        //    int processed = 0;

        //    while (uvQueue.Count > 0 && processed < maxPerFrame)
        //    {
        //        Vector2 uv = uvQueue.Dequeue();
        //        DrawTransparentAt(uv);
        //        processed++;
        //    }
        //}

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector2 screenPos = Input.mousePosition;

        //    if (rect == null) rect = rawImage.rectTransform;

        //    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out Vector2 localPos))
        //    {
        //        Vector2 uv = new Vector2(
        //            (localPos.x + rect.rect.width * 0.5f) / rect.rect.width,
        //            (localPos.y + rect.rect.height * 0.5f) / rect.rect.height
        //        );

        //        float sizeX = brushSize / renderTex.width;
        //        float sizeY = brushSize / renderTex.height;

        //        Vector4[] brushMultiPos = new Vector4[10];
        //        //brushMultiPos[0] = new Vector4(uv.x, uv.y, sizeX, sizeY); // �u�]�w�Ĥ@��
        //        //brushMultiPos[1] = new Vector4(1 - uv.x, 1 - uv.y, sizeX, sizeY); // �u�]�w�ĤG��
        //        //brushMultiPos[1] = new Vector4(uv.x, 1 - uv.y, sizeX, sizeY); // �u�]�w�ĤG��
        //        //brushMultiPos[1] = new Vector4(1 - uv.x, uv.y, sizeX, sizeY); // �u�]�w�ĤG��
        //        for (int i = 0; i < brushMultiPos.Length; i++)
        //        {
        //            brushMultiPos[i] = new Vector4(uv.x + 0.01f * i, uv.y + 0.01f * i, sizeX, sizeY); // �u�]�w�Ĥ@��
        //        }

        //        eraseMaterial.SetTexture("_MainTex", renderTex);
        //        eraseMaterial.SetTexture("_BrushTex", brushTexture);
        //        eraseMaterial.SetFloat("_AlphaDecayFactor", 0.1f);
        //        eraseMaterial.SetVectorArray("_BrushMultiPos", brushMultiPos);

        //        RenderTexture tempRT = RenderTexture.GetTemporary(renderTex.width, renderTex.height, 0, RenderTextureFormat.ARGB32);
        //        Graphics.Blit(renderTex, tempRT, eraseMaterial); // shader ��X�� temp
        //        Graphics.Blit(tempRT, renderTex);                // �A�ƻs�^��
        //        RenderTexture.ReleaseTemporary(tempRT);

        //        rawImage.texture = renderTex;

        //        Debug.Log($"[Manual Blit] uv: {uv}, size: ({sizeX}, {sizeY})");
        //    }
        //}
        //if (Input.GetMouseButton(0))
        //{
        //    if (rect == null) rect = rawImage.rectTransform;

        //    Vector2 screenPos = Input.mousePosition;
        //    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out Vector2 localPos))
        //    {
        //        Vector2 baseUV = new Vector2(
        //            (localPos.x + rect.rect.width * 0.5f) / rect.rect.width,
        //            (localPos.y + rect.rect.height * 0.5f) / rect.rect.height
        //        );

        //        List<Vector2> uvList = new List<Vector2>();

        //        for (int i = 0; i < maxUvPerFrame; i++)
        //        {
        //            uvList.Add(new Vector2(
        //                baseUV.x + 0.005f * i,
        //                baseUV.y + 0.005f * i
        //            ));
        //        }

        //        DrawTransparentAt(uvList);

        //        Debug.Log($"[Test] �ǤJ {uvList.Count} �� UV �� DrawTransparentAt");
        //    }
        //}

        {//�@�V�B�̦h��uv
            List<Vector2> uvBatch = new List<Vector2>();

            while (uvQueue.Count > 0 && uvBatch.Count < maxUvPerFrame)
            {
                uvBatch.Add(uvQueue.Dequeue());
            }

            if (uvBatch.Count > 0)
            {
                Debug.Log($"[Debug] ���V�B�z���ơG{uvBatch.Count}");
                DrawTransparentAt(uvBatch); // �ϥΧA����@���h������
            }
        }
    }
    public void SetMask(Sprite newMask)
    {
        maskImage = newMask;
    }
}

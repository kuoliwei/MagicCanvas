using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ScratchCard : MonoBehaviour, IPointerDownHandler
{
    private Sprite maskImage;
    public RawImage rawImage;
    private Texture brushTexture;
    private Material eraseMaterial;
    private float brushSize = 64f;

    private RenderTexture renderTex;
    private RectTransform rect;
    private Texture2D croppedTex;
    private bool isFullyRevealed = false;

    public System.Action OnFullyRevealed;
    private readonly Queue<Vector2> uvQueue = new();
    [SerializeField] int maxUvPerFrame = 32;
    private void Start()
    {

    }
    public void Init()
    {
        rect = rawImage.rectTransform;
        renderTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        renderTex.Create();
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

        //if (!isFullyRevealed && GetClearedRatio() >= clearThreshold)
        //{
        //    ShowFullImage();
        //}
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

            //if (!isFullyRevealed && GetClearedRatio() >= clearThreshold)
            //{
            //    ShowFullImage();
            //}
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
            if (pixels[i].a < 8) cleared++;
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

    public IEnumerator SmoothRestoreMask(float restoreSpeed)
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

    public void EraseAtNormalizedUV(Vector2 uv)
    {
        if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
        {
            uvQueue.Enqueue(uv);
        }
    }

    public void EraseAtScreenPosition(Vector2 screenPos)
    {
        if (rect == null) rect = rawImage.rectTransform;

        //if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out Vector2 localPos))
        // [���] �� World Space Canvas ���Ѧ��� camera�A�קK��m�ഫ����
        Camera eventCam = rawImage.canvas.renderMode == RenderMode.WorldSpace ? rawImage.canvas.worldCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, eventCam, out Vector2 localPos))
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
    private void Update()
    {
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
    public void SetBrush(Texture brush, Material material, float size)
    {
        this.brushTexture = brush;
        this.eraseMaterial = material;
        this.brushSize = size;
    }
}

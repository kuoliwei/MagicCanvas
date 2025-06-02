using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;


public class ScratchManager : MonoBehaviour
{
    public List<Sprite> maskImages;     // 遮罩圖片（共用）
    public List<Texture> backgroundImages; // 底圖（會顯示在 RawImage 背後）
    public RawImage backgroundRenderer; // 顯示底圖的 RawImage
    public ScratchCard scratchCard;

    public float clearThreshold = 0.6f; // 觸發立即揭曉的百分比
    public float revealHoldTime = 15f;  // 顯示後等待恢復的秒數

    private int currentIndex = 0;
    private bool imageFullyRevealed = false;
    private Coroutine restoreRoutine;

    void Start()
    {
        scratchCard.OnFullyRevealed += OnScratchCleared; // 建立事件連結
        ShowImageAt(0);
    }

    public void ShowImageAt(int index)
    {
        if (index >= backgroundImages.Count) index = 0;
        currentIndex = index;
        imageFullyRevealed = false;

        backgroundRenderer.texture = backgroundImages[index];
        scratchCard.SetMask(maskImages[index]);
        scratchCard.ResetScratch();
    }

    public void OnScratchCleared() // 由 ScratchCard 呼叫
    {
        if (!imageFullyRevealed)
        {
            imageFullyRevealed = true;
            scratchCard.ShowFullImage();
            restoreRoutine = StartCoroutine(AutoRestoreAfterDelay());
        }
    }

    private IEnumerator AutoRestoreAfterDelay()
    {
        yield return new WaitForSeconds(revealHoldTime);
        yield return scratchCard.SmoothRestoreMask();
        ShowImageAt(currentIndex + 1);
    }

    public void CancelRestore()
    {
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }
    }
}

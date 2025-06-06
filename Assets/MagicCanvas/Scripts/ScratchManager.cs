using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;


public class ScratchManager : MonoBehaviour
{
    public List<Sprite> maskImages;     // 遮罩圖片（共用）
    public List<Texture> backgroundImages; // 底圖（會顯示在 RawImage 背後）
    public List<RawImage> backgroundRenderers; // 顯示底圖的 RawImage
    public List<ScratchCard> scratchCards; // [修改] 改為多個 ScratchCard

    public float clearThreshold = 0.6f; // 觸發立即揭曉的百分比
    public float revealHoldTime = 15f;  // 顯示後等待恢復的秒數
    public float restoreSpeed = 1f; // [新增] 從 ScratchCard 移過來，由 Manager 控制還原速度

    public Texture brushTexture;        // 新增：由 Manager 統一提供筆刷
    public Material eraseMaterial;      // 新增：由 Manager 統一提供刮除材質
    public float brushSize = 64f;       // 新增：由 Manager 控一律設定筆刷大小

    private int currentIndex = 0;
    private bool imageFullyRevealed = false;
    private Coroutine restoreRoutine;

    void Start()
    {
        foreach (var card in scratchCards)
        {
            card.Init();
            card.SetBrush(brushTexture, eraseMaterial, brushSize); // [新增] 初始化筆刷參數
            card.OnFullyRevealed += () => OnScratchCleared(card);
        }
        ShowImageAt(0);
    }

    public void ShowImageAt(int index)
    {
        if (index >= backgroundImages.Count) index = 0;
        currentIndex = index;
        imageFullyRevealed = false;
        foreach (var renderer in backgroundRenderers)
        {
            renderer.texture = backgroundImages[index];
        }
        // 每張卡片個別設定遮罩
        foreach (var card in scratchCards)
        {
            card.SetMask(maskImages[index]);
            card.ResetScratch();
        }
    }
    // 修改：只處理呼叫者，不統一處理所有卡片
    public void OnScratchCleared(ScratchCard caller)
    {
        if (!imageFullyRevealed)
        {
            imageFullyRevealed = true;
            caller.ShowFullImage();
            restoreRoutine = StartCoroutine(AutoRestoreAfterDelay(caller));
        }
    }
    //public void OnScratchCleared() // 由 ScratchCard 呼叫
    //{
    //    if (!imageFullyRevealed)
    //    {
    //        imageFullyRevealed = true;
    //        scratchCard.ShowFullImage();
    //        restoreRoutine = StartCoroutine(AutoRestoreAfterDelay());
    //    }
    //}

    private IEnumerator AutoRestoreAfterDelay(ScratchCard target)
    {
        yield return new WaitForSeconds(revealHoldTime);
        yield return target.SmoothRestoreMask(restoreSpeed);
        ShowImageAt(currentIndex + 1);
    }
    //private IEnumerator AutoRestoreAfterDelay()
    //{
    //    yield return new WaitForSeconds(revealHoldTime);
    //    yield return scratchCard.SmoothRestoreMask();
    //    ShowImageAt(currentIndex + 1);
    //}

    public void CancelRestore()
    {
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }
    }
    private void Update()
    {
        if (!imageFullyRevealed)
        {
            foreach (var card in scratchCards)
            {
                if (card.GetClearedRatio() >= clearThreshold)
                {
                    imageFullyRevealed = true;
                    card.ShowFullImage();
                    restoreRoutine = StartCoroutine(AutoRestoreAfterDelay(card));
                    break;
                }
            }
        }
    }
}

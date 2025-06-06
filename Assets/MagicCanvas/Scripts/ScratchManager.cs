using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;


public class ScratchManager : MonoBehaviour
{
    public List<Sprite> maskImages;     // �B�n�Ϥ��]�@�Ρ^
    public List<Texture> backgroundImages; // ���ϡ]�|��ܦb RawImage �I��^
    public List<RawImage> backgroundRenderers; // ��ܩ��Ϫ� RawImage
    public List<ScratchCard> scratchCards; // [�ק�] �אּ�h�� ScratchCard

    public float clearThreshold = 0.6f; // Ĳ�o�ߧY���媺�ʤ���
    public float revealHoldTime = 15f;  // ��ܫᵥ�ݫ�_�����
    public float restoreSpeed = 1f; // [�s�W] �q ScratchCard ���L�ӡA�� Manager �����٭�t��

    public Texture brushTexture;        // �s�W�G�� Manager �Τ@���ѵ���
    public Material eraseMaterial;      // �s�W�G�� Manager �Τ@���Ѩ�����
    public float brushSize = 64f;       // �s�W�G�� Manager ���@�߳]�w����j�p

    private int currentIndex = 0;
    private bool imageFullyRevealed = false;
    private Coroutine restoreRoutine;

    void Start()
    {
        foreach (var card in scratchCards)
        {
            card.Init();
            card.SetBrush(brushTexture, eraseMaterial, brushSize); // [�s�W] ��l�Ƶ���Ѽ�
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
        // �C�i�d���ӧO�]�w�B�n
        foreach (var card in scratchCards)
        {
            card.SetMask(maskImages[index]);
            card.ResetScratch();
        }
    }
    // �ק�G�u�B�z�I�s�̡A���Τ@�B�z�Ҧ��d��
    public void OnScratchCleared(ScratchCard caller)
    {
        if (!imageFullyRevealed)
        {
            imageFullyRevealed = true;
            caller.ShowFullImage();
            restoreRoutine = StartCoroutine(AutoRestoreAfterDelay(caller));
        }
    }
    //public void OnScratchCleared() // �� ScratchCard �I�s
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

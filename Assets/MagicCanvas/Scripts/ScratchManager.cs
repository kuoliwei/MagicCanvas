using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;


public class ScratchManager : MonoBehaviour
{
    public List<Sprite> maskImages;     // �B�n�Ϥ��]�@�Ρ^
    public List<Texture> backgroundImages; // ���ϡ]�|��ܦb RawImage �I��^
    public RawImage backgroundRenderer; // ��ܩ��Ϫ� RawImage
    public ScratchCard scratchCard;

    public float clearThreshold = 0.6f; // Ĳ�o�ߧY���媺�ʤ���
    public float revealHoldTime = 15f;  // ��ܫᵥ�ݫ�_�����

    private int currentIndex = 0;
    private bool imageFullyRevealed = false;
    private Coroutine restoreRoutine;

    void Start()
    {
        scratchCard.OnFullyRevealed += OnScratchCleared; // �إߨƥ�s��
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

    public void OnScratchCleared() // �� ScratchCard �I�s
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

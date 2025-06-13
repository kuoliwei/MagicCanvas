using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandParticleEffectSpawner : MonoBehaviour
{
    public GameObject particlePrefab;
    public RectTransform canvasRect;

    private List<GameObject> currentEffects = new List<GameObject>();
    private float noUpdateTimer = 0f;
    private float noUpdateThreshold = 0.5f; // 0.5��L��s�N�۰ʲM��
    /// <summary>
    /// ���~���ǤJ�h�ծy�СA�C�ծy�з|���@�ӹ����S��
    /// </summary>
    public void SyncParticlesToScreenPositions(List<Vector2> screenPosList)
    {
        noUpdateTimer = 0f; // [�s�W] �u�n����s�N�k�s
        // 1. �s�y�Ф�{���S�Ħh�A�n Instantiate �ɨ�
        while (currentEffects.Count < screenPosList.Count)
        {
            var go = Instantiate(particlePrefab, canvasRect);
            // [�s�W] �j��]�w�̤j�ɤl�ƶq
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.maxParticles = 1000;
            }

            currentEffects.Add(go);
        }

        // 2. �s�y�Ф�{���S�Ĥ֡A�n Destroy �h��
        while (currentEffects.Count > screenPosList.Count)
        {
            int last = currentEffects.Count - 1;
            Destroy(currentEffects[last]);
            currentEffects.RemoveAt(last);
        }

        // 3. ���ʲ{���S�Ĩ�s�y��
        for (int i = 0; i < screenPosList.Count; i++)
        {
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosList[i], canvasRect.GetComponentInParent<Canvas>().worldCamera, out localPos))
            {
                Vector3 finalPos = new Vector3(localPos.x, localPos.y, -500f); // [�T�wZ�b]
                currentEffects[i].transform.localPosition = finalPos;
            }
        }
    }

    /// <summary>
    /// �S�����󦳮Įy�ЮɩI�s�A�����^��
    /// </summary>
    public void ClearAll()
    {
        foreach (var go in currentEffects)
        {
            // [�s�W] �Ұʨ�{���橵��P��
            StartCoroutine(DelayedParticleDestroy(go));
        }
        currentEffects.Clear();
    }
    private IEnumerator DelayedParticleDestroy(GameObject go)
    {
        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // �]�w�̤j�ɤl�Ƭ�0
            var main = ps.main;
            main.maxParticles = 0;
        }
        // ��1��
        yield return new WaitForSeconds(3f);

        Destroy(go);
    }
    void Update()
    {
        noUpdateTimer += Time.deltaTime;

        if (noUpdateTimer > noUpdateThreshold && currentEffects.Count > 0)
        {
            ClearAll();
            // �i��GDebug.Log("�W�L0.5��S����WebSocket�A�����P���S��");
        }
    }
}

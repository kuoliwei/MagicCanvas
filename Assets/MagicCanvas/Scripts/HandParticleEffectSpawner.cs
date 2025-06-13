using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandParticleEffectSpawner : MonoBehaviour
{
    public GameObject particlePrefab;
    public RectTransform canvasRect;

    private List<GameObject> currentEffects = new List<GameObject>();
    private float noUpdateTimer = 0f;
    private float noUpdateThreshold = 0.5f; // 0.5秒無更新就自動清空
    /// <summary>
    /// 讓外部傳入多組座標，每組座標會有一個對應特效
    /// </summary>
    public void SyncParticlesToScreenPositions(List<Vector2> screenPosList)
    {
        noUpdateTimer = 0f; // [新增] 只要有更新就歸零
        // 1. 新座標比現有特效多，要 Instantiate 補足
        while (currentEffects.Count < screenPosList.Count)
        {
            var go = Instantiate(particlePrefab, canvasRect);
            // [新增] 強制設定最大粒子數量
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.maxParticles = 1000;
            }

            currentEffects.Add(go);
        }

        // 2. 新座標比現有特效少，要 Destroy 多的
        while (currentEffects.Count > screenPosList.Count)
        {
            int last = currentEffects.Count - 1;
            Destroy(currentEffects[last]);
            currentEffects.RemoveAt(last);
        }

        // 3. 移動現有特效到新座標
        for (int i = 0; i < screenPosList.Count; i++)
        {
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosList[i], canvasRect.GetComponentInParent<Canvas>().worldCamera, out localPos))
            {
                Vector3 finalPos = new Vector3(localPos.x, localPos.y, -500f); // [固定Z軸]
                currentEffects[i].transform.localPosition = finalPos;
            }
        }
    }

    /// <summary>
    /// 沒有任何有效座標時呼叫，全部回收
    /// </summary>
    public void ClearAll()
    {
        foreach (var go in currentEffects)
        {
            // [新增] 啟動協程執行延遲銷毀
            StartCoroutine(DelayedParticleDestroy(go));
        }
        currentEffects.Clear();
    }
    private IEnumerator DelayedParticleDestroy(GameObject go)
    {
        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 設定最大粒子數為0
            var main = ps.main;
            main.maxParticles = 0;
        }
        // 等1秒
        yield return new WaitForSeconds(3f);

        Destroy(go);
    }
    void Update()
    {
        noUpdateTimer += Time.deltaTime;

        if (noUpdateTimer > noUpdateThreshold && currentEffects.Count > 0)
        {
            ClearAll();
            // 可選：Debug.Log("超過0.5秒沒收到WebSocket，全部銷毀特效");
        }
    }
}

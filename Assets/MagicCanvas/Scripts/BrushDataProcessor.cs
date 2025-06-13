using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BrushDataProcessor : MonoBehaviour
{
    [SerializeField] private List<ScratchCard> scratchCards;
    [SerializeField] HandParticleEffectSpawner spawner;
    public void HandleBrushData(List<BrushData> dataList)
    {
        //List<Vector2> screenPosList = new List<Vector2>();

        //foreach (var data in dataList)
        //{
        //    if (data.point == null || data.point.Length < 2)
        //        continue;
        //    int x = (int)(Screen.width * data.point[0]);
        //    int y = (int)(Screen.height * (1 - data.point[1]));
        //    //Debug.Log("y :" + y);
        //    screenPosList.Add(new Vector2(x, y));
        //}

        //if (screenPosList.Count > 0)
        //{
        //    foreach(var data in screenPosList)
        //    {
        //        scratchCard.EraseAtScreenPosition(data);
        //    }
        //}
        // [修改] 改為直接使用 UV，不再轉換為螢幕座標

        //foreach (var data in dataList)
        //{
        //    if (data.point == null || data.point.Length < 2)
        //        continue;

        //    // JSON 傳來為左上為原點，需轉為左下為原點
        //    float x = data.point[0];
        //    float y = 1f - data.point[1];
        //    Vector2 uv = new Vector2(x, y);
        //    // 粒子特效新增：將 UV 轉為螢幕座標（0~1 → 0~畫面寬高）
        //    Vector2 screenPos = new Vector2(x * Screen.width, y * Screen.height);
        //    if (spawner != null)
        //        spawner.SpawnAtScreenPosition(screenPos);
        //    // 改用 UV 版本的 API
        //    //scratchCard.EraseAtNormalizedUV(uv);
        //    foreach (var card in scratchCards)
        //    {
        //        card.EraseAtNormalizedUV(uv); // 每一個卡片都處理這個刮除點
        //    }
        //}

        List<Vector2> screenPosList = new List<Vector2>(); // [新增] 收集所有手座標的螢幕位置

        foreach (var data in dataList)
        {
            if (data.point == null || data.point.Length < 2)
                continue;

            // JSON 傳來為左上為原點，需轉為左下為原點
            float x = data.point[0];
            float y = 1f - data.point[1];
            Vector2 uv = new Vector2(x, y);

            screenPosList.Add(new Vector2(x * Screen.width, y * Screen.height)); // [新增] 加入螢幕座標清單

            foreach (var card in scratchCards)
            {
                card.EraseAtNormalizedUV(uv); // 每一個卡片都處理這個刮除點
            }
        }

        // [新增] 控制多個特效物件
        if (screenPosList.Count > 0)
            spawner.SyncParticlesToScreenPositions(screenPosList); // 多個手就會有多個特效
        else
            spawner.ClearAll(); // 沒人時全部銷毀
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BrushDataProcessor : MonoBehaviour
{
    [SerializeField] private List<ScratchCard> scratchCards;
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
        foreach (var data in dataList)
        {
            if (data.point == null || data.point.Length < 2)
                continue;

            // JSON 傳來為左上為原點，需轉為左下為原點
            float x = data.point[0];
            float y = 1f - data.point[1];
            Vector2 uv = new Vector2(x, y);

            // 改用 UV 版本的 API
            //scratchCard.EraseAtNormalizedUV(uv);
            foreach (var card in scratchCards)
            {
                card.EraseAtNormalizedUV(uv); // 每一個卡片都處理這個刮除點
            }
        }
    }
}

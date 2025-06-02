using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BrushDataProcessor : MonoBehaviour
{
    [SerializeField] private ScratchCard scratchCard;
    public void HandleBrushData(List<BrushData> dataList)
    {
        List<Vector2> screenPosList = new List<Vector2>();

        foreach (var data in dataList)
        {
            if (data.point == null || data.point.Length < 2)
                continue;
            int x = (int)(Screen.width * data.point[0]);
            int y = (int)(Screen.height * (1 - data.point[1]));
            //Debug.Log("y :" + y);
            screenPosList.Add(new Vector2(x, y));
        }

        if (screenPosList.Count > 0)
        {
            foreach(var data in screenPosList)
            {
                scratchCard.EraseAtScreenPosition(data);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BrushPositionJson
{
    public List<BrushData> data;
}

[System.Serializable]
public class BrushData
{
    public int roller_id;
    public float[] point; // �w���榡�� [x, y]�A0~1 �۹�y��
}

[System.Serializable]
public class BrushUvData
{
    public List<Vector2> uvList = new List<Vector2>();
}

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
        // [�ק�] �אּ�����ϥ� UV�A���A�ഫ���ù��y��

        //foreach (var data in dataList)
        //{
        //    if (data.point == null || data.point.Length < 2)
        //        continue;

        //    // JSON �ǨӬ����W�����I�A���ର���U�����I
        //    float x = data.point[0];
        //    float y = 1f - data.point[1];
        //    Vector2 uv = new Vector2(x, y);
        //    // �ɤl�S�ķs�W�G�N UV �ର�ù��y�С]0~1 �� 0~�e���e���^
        //    Vector2 screenPos = new Vector2(x * Screen.width, y * Screen.height);
        //    if (spawner != null)
        //        spawner.SpawnAtScreenPosition(screenPos);
        //    // ��� UV ������ API
        //    //scratchCard.EraseAtNormalizedUV(uv);
        //    foreach (var card in scratchCards)
        //    {
        //        card.EraseAtNormalizedUV(uv); // �C�@�ӥd�����B�z�o�Ө��I
        //    }
        //}

        List<Vector2> screenPosList = new List<Vector2>(); // [�s�W] �����Ҧ���y�Ъ��ù���m

        foreach (var data in dataList)
        {
            if (data.point == null || data.point.Length < 2)
                continue;

            // JSON �ǨӬ����W�����I�A���ର���U�����I
            float x = data.point[0];
            float y = 1f - data.point[1];
            Vector2 uv = new Vector2(x, y);

            screenPosList.Add(new Vector2(x * Screen.width, y * Screen.height)); // [�s�W] �[�J�ù��y�вM��

            foreach (var card in scratchCards)
            {
                card.EraseAtNormalizedUV(uv); // �C�@�ӥd�����B�z�o�Ө��I
            }
        }

        // [�s�W] ����h�ӯS�Ī���
        if (screenPosList.Count > 0)
            spawner.SyncParticlesToScreenPositions(screenPosList); // �h�Ӥ�N�|���h�ӯS��
        else
            spawner.ClearAll(); // �S�H�ɥ����P��
    }
}

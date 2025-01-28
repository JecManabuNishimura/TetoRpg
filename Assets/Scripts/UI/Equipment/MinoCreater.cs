using System.Collections.Generic;
using System;
using MyMethods;
using UnityEngine;
using UnityEngine.UI;

public class MinoCreater : MonoBehaviour
{
    [SerializeField] private GameObject minoImage;
    [SerializeField] private GameObject ItemParent;
    private EquipmentUniqueData equipmentData;
    public float centerX = 0;
    public float centerY = 0;
    private Color normalColor;
    private void Start()
    {
        //CreateMino();
        normalColor = GetComponent<Image>().color;
    }

    public void UpdateId(EquipmentUniqueData data)
    {
        equipmentData = data;
        /*
        if (GameManager.Instance.player.belongingsMino.Contains(id))
        {
            //GetComponent<Image>().color = Color.green;
        }*/
    }

    public EquipmentUniqueData GetMinoId() => equipmentData;
    public void CreateMino()
    {
        var mino = MinoFactory.GetMinoData(equipmentData.WeaponId.toInt());
        centerX = 0;
        centerY = 0;
        // 0以外の位置をリストアップ
        List<Tuple<int, int>> positions = new List<Tuple<int, int>>();

        for (int i = 0; i < mino.GetLength(0); i++)
        {
            for (int j = 0; j < mino.GetLength(1); j++)
            {
                if (mino[i, j] != 0)
                {
                    positions.Add(Tuple.Create(i, j));  // (x, y)の位置を格納
                }
            }
        }
        if (positions.Count > 0)
        {
            // x座標、y座標の平均を計算
            foreach (var position in positions)
            {
                centerX += position.Item2;
                centerY += position.Item1;
            }

            centerX /= positions.Count;
            centerY /= positions.Count;
        }
        else
        {
            Debug.LogError("0以外の値は存在しません");
        }

        // ミノの作成
        for (int y = 0; y < mino.GetLength(0); y++)
        {
            for(int x = 0; x < mino.GetLength(1); x++)
            {
                if (mino[y,x] != 0)
                {
                    GameObject obj = Instantiate(minoImage, ItemParent.transform, true);
                    obj.transform.localPosition = new Vector3((x*40) - (centerX * 40), -((y*40) -(centerY * 40)), 0);
                }
            }
        }
    }
}

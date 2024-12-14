using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MinoData", menuName = "Scriptable Objects/MinoData")]
public class MinoData : ScriptableObject
{
    private static MinoData _entity;
    public static MinoData Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                string assetPath = "Master/MinosData";
                _entity = Resources.Load<MinoData>(assetPath);
                
                if (_entity == null)
                {
                    Debug.LogError(nameof(EquipmentDatabase) + " not found");
                }
            }

            return _entity;
        }
    }
    public List<MinoParameter> Parameters = new();
    
    public int TetoMinoLength => Parameters.Count;

    public int[,] GetMinoData(int index)
    {
        return ConvertToNestedList(Parameters[index].minos,Parameters[index].rows,Parameters[index].cols);
    }

    public List<MinoEffect> GetMinoEffect(EquipmentUniqueData data)
    {
        return Parameters[int.Parse(data.WeaponId)].selectedGroupOptions[data.groupID].effects;
    }
    int[,] ConvertToNestedList(List<int> flatList, int rows, int cols)
    {
        int[,] nestedList = new int[rows,cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                nestedList[i, j] = flatList[i * cols + j];
            }
        }
        return nestedList;
    }
}

[Serializable]
public class MinoParameter
{
    public List<int> minos; // フラットなリスト
    public int rows;        // 行数
    public int cols;        // 列数
    public List<MinoEffectGroup> selectedGroupOptions = new ();
    //public List<string> selectedGroupOptions;
    
}
[Serializable]
public class MinoEffectGroup
{
    public MinoEffectGroup(int id, List<MinoEffect> effects)
    {
        this.id = id;
        this.effects = effects;
    }
    public int id;
    public List<MinoEffect> effects;
}

[Serializable]
public class MinoEffect
{
    public MinoEffect(string effectStatus, int value)
    {
        effect = effectStatus;
        this.value = value;
    }
    public string effect;
    public int value;
}
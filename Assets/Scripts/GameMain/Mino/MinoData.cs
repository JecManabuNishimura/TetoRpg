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
                string assetPath = "Assets/Data/MinosData.asset";
                _entity = AssetDatabase.LoadAssetAtPath<MinoData>(assetPath);
                
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

    public List<string> GetMinoEffect(int index)
    {
        return Parameters[index].selectedGroupOptions;
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
    public List<string> selectedGroupOptions;
    
}
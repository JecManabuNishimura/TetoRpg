using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MinoData", menuName = "Scriptable Objects/MinoData")]
public class MinoData : ScriptableObject
{
    public List<MinoParameter> Parameters = new();
}

[Serializable]
public struct MinoParameter
{
    public int[,] minos;
    public List<string> selectedGroupOptions;
    
}
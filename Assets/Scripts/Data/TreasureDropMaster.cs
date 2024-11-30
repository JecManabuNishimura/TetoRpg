using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "TreasureDropMaster", menuName = "Scriptable Objects/TreasureDropMaster")]
public class TreasureDropMaster : ScriptableObject
{
    public List<ItemDropData> itemDropData = new List<ItemDropData>();
    
    public string GetItemDataId()
    {
        float totalRate = 0f;
        foreach (var item in itemDropData)
        {
            totalRate += item.dropRate;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalRate);

        float cumulativeRate = 0f;
        foreach (var item in itemDropData)
        {
            cumulativeRate += item.dropRate;
            if (randomValue <= cumulativeRate)
            {
                return item.id;
            }
        }

        return null;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(TreasureDropMaster))]
public class TreasureDropMasterEditor : Editor
{
    private List<string> equipmentIds = new List<string>();

    public override void OnInspectorGUI()
    {
        // TreasureDropMasterインスタンスを取得
        TreasureDropMaster master = (TreasureDropMaster)target;

        // EquipmentMasterをロード
        EquipmentMaster equipmentMaster = Resources.Load<EquipmentMaster>("Master/EquipmentMaster");

        if (equipmentMaster == null || equipmentMaster.equipData.Count == 0)
        {
            EditorGUILayout.HelpBox("EquipmentMasterが見つからない、または装備データが空です。", MessageType.Warning);
            return;
        }

        // EquipmentDataのIDリストを準備
        equipmentIds.Clear();
        foreach (var equip in equipmentMaster.equipData)
        {
            equipmentIds.Add(equip.id);
        }

        // ItemDropDataリストの編集
        EditorGUILayout.LabelField("Item Drop Data List", EditorStyles.boldLabel);

        for (int i = 0; i < master.itemDropData.Count; i++)
        {
            var item = master.itemDropData[i];

            EditorGUILayout.BeginHorizontal();

            // ドロップダウンでIDを選択
            int selectedIndex = equipmentIds.IndexOf(item.id);
            if (selectedIndex == -1) selectedIndex = 0; // デフォルト値に戻す

            selectedIndex = EditorGUILayout.Popup("", selectedIndex, equipmentIds.ToArray(), GUILayout.Width(150));
            item.id = equipmentIds[selectedIndex];

            // Drop Rateの編集
            item.dropRate = EditorGUILayout.FloatField("", item.dropRate, GUILayout.Width(100));

            // 削除ボタン
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                master.itemDropData.RemoveAt(i);
                i--; // インデックス調整
            }

            EditorGUILayout.EndHorizontal();
        }

        // 新しい要素を追加
        if (GUILayout.Button("Add Item"))
        {
            master.itemDropData.Add(new ItemDropData { id = equipmentIds[0], dropRate = 0f });
        }

        // 変更があれば保存
        if (GUI.changed)
        {
            EditorUtility.SetDirty(master);
        }
    }
}
#endif
[Serializable]
public class ItemDropData
{
    public string id;
    public float dropRate;
} 
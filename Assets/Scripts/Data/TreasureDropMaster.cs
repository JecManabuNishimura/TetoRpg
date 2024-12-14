using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "TreasureDropMaster", menuName = "Scriptable Objects/TreasureDropMaster")]
public class TreasureDropMaster : ScriptableObject
{
    public List<ItemDropData> itemDropData = new List<ItemDropData>();
    public List<ItemDropData> minoDropData = new List<ItemDropData>();
    public EquipmentUniqueData GetItemDataId()
    {
        var allData = itemDropData.Concat(minoDropData).ToList();
        float totalRate = 0f;
        foreach (var item in allData)
        {
            totalRate += item.dropRate;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalRate);

        float cumulativeRate = 0f;
        foreach (var item in allData)
        {
            cumulativeRate += item.dropRate;
            if (randomValue <= cumulativeRate)
            {
                return new EquipmentUniqueData(item.id,item.groupId);
            }
        }

        return new EquipmentUniqueData("0",0);
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
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("WeaponId",GUILayout.Width(60));
            EditorGUILayout.LabelField("GroupId",GUILayout.Width(60));
            EditorGUILayout.LabelField("Drop%",GUILayout.Width(60));
        }
        for (int i = 0; i < master.itemDropData.Count; i++)
        {
            var item = master.itemDropData[i];

            EditorGUILayout.BeginHorizontal();

            // ドロップダウンでIDを選択
            int selectedIndex = equipmentIds.IndexOf(item.id);
            if (selectedIndex == -1) selectedIndex = 0; // デフォルト値に戻す

            selectedIndex = EditorGUILayout.Popup( selectedIndex, equipmentIds.ToArray(), GUILayout.Width(60));
            item.id = equipmentIds[selectedIndex];
            item.groupId = EditorGUILayout.IntField(item.groupId, GUILayout.Width(60));
            // Drop Rateの編集
            item.dropRate = EditorGUILayout.FloatField( item.dropRate, GUILayout.Width(60));

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
        
        EditorGUILayout.LabelField("Mino Drop Data List", EditorStyles.boldLabel);
        for (int i = 0; i < master.minoDropData.Count; i++)
        {
            var item = master.minoDropData[i];
            using (new EditorGUILayout.HorizontalScope())
            {
                item.id = EditorGUILayout.TextField(item.id,  GUILayout.Width(60));
                
                item.dropRate = EditorGUILayout.FloatField("", item.dropRate, GUILayout.Width(50));
                // 削除ボタン
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    master.minoDropData.RemoveAt(i);
                    i--; // インデックス調整
                }
            }
        }

        if (GUILayout.Button("Add Mino"))
        {
            master.minoDropData.Add(new ItemDropData { id = "0",groupId = 0, dropRate = 0f });
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
    public int groupId;
    public float dropRate;
} 
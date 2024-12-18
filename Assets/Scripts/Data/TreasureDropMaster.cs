using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "TreasureDropMaster", menuName = "Scriptable Objects/TreasureDropMaster")]
public class TreasureDropMaster : ScriptableObject
{
    public List<ItemDropData> itemDropData = new List<ItemDropData>();
    public List<ItemDropData> minoDropData = new List<ItemDropData>();
    public EquipmentUniqueData GetItemDataId()
    {
        int totalcount = minoDropData.Count + itemDropData.Count;
        List<ItemDropData> item;
        /*
        if (Random.Range(0, totalcount) < minoDropData.Count)
        {
            item = minoDropData;
        }
        else
        {
            item = itemDropData;
        }
        */
        item = minoDropData;
        //var allData = itemDropData.Concat(minoDropData).ToList();
        int rate = Random.Range(0, 100);
        Rarity rarity = Rarity.D;
        if (rate <= 1)
        {
            rarity = Rarity.SSS;
        }
        else if (rate < 5)
        {
            rarity = Rarity.SS;
        }
        else if (rate < 10)
        {
            rarity = Rarity.S;
        }
        else if (rate < 20)
        {
            rarity = Rarity.A;
        }
        else if (rate < 40)
        {
            rarity = Rarity.B;
        }
        else if (rate < 60)
        {
            rarity = Rarity.C;
        }
        else 
        {
            rarity = Rarity.D;
        }
        
        var items = item.Where(x => x.dropRarity == rarity).ToList();
        while (true)
        {
            if (items.Count == 0)
            {
                int current = (int)rarity;
                rarity = (Rarity)(current + 1);
                items = item.Where(x => x.dropRarity == rarity).ToList();
            }
            else
            {
                break;
            }
        }

        var selectItem = items[Random.Range(0, items.Count)];
        return new EquipmentUniqueData(selectItem.id,selectItem.groupId);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(TreasureDropMaster))]
public class TreasureDropMasterEditor : Editor
{
    private ReorderableList itemDropList;
    private ReorderableList minoDropList;
    
    private List<string> equipmentIds = new List<string>();

    private void OnEnable()
    {
        TreasureDropMaster master = (TreasureDropMaster)target;

        EquipmentMaster equipmentMaster = Resources.Load<EquipmentMaster>("Master/EquipmentMaster");
        if (equipmentMaster != null && equipmentMaster.equipData.Count > 0)
        {
            equipmentIds = equipmentMaster.equipData.Select(equip => equip.id).ToList();
        }
        else
        {
            equipmentIds.Add("0"); // デフォルト値
        }

        // itemDropDataのReorderableList設定
        itemDropList = CreateReorderableList("Item Drop Data", master.itemDropData, true);

        // minoDropDataのReorderableList設定
        minoDropList = CreateReorderableList("Mino Drop Data", master.minoDropData, false);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        itemDropList.DoLayoutList();
        minoDropList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    private ReorderableList CreateReorderableList(string title, List<ItemDropData> dataList, bool useDropdown)
    {
        var list = new ReorderableList(dataList, typeof(ItemDropData), true, true, true, true);

        // ヘッダー描画
        list.drawHeaderCallback = (Rect rect) =>
        {
            float width = rect.width / 3;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, width, rect.height), "WeaponId");
            EditorGUI.LabelField(new Rect(rect.x + width, rect.y, width, rect.height), "GroupId");
            EditorGUI.LabelField(new Rect(rect.x + width * 2, rect.y, width, rect.height), "Rarity");
        };

        // 各要素の描画
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var item = dataList[index];
            float width = rect.width / 3 - 5;

            if (useDropdown)
            {
                // ドロップダウンメニューでID選択
                int selectedIndex = equipmentIds.IndexOf(item.id);
                if (selectedIndex == -1) selectedIndex = 0;

                selectedIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                    selectedIndex, equipmentIds.ToArray());
                item.id = equipmentIds[selectedIndex];
            }
            else
            {
                // テキストフィールドでID編集
                item.id = EditorGUI.TextField(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight), item.id);
            }

            // GroupIdとDropRateの描画
            item.groupId = EditorGUI.IntField(new Rect(rect.x + width + 5, rect.y, width, EditorGUIUtility.singleLineHeight), item.groupId);
            item.dropRarity = (Rarity)EditorGUI.EnumPopup(new Rect(rect.x + (width * 2) + 10, rect.y, width, EditorGUIUtility.singleLineHeight), item.dropRarity);
        };

        // 要素の削除
        list.onRemoveCallback = (list) =>
        {
            if (list.index >= 0 && list.index < dataList.Count)
            {
                dataList.RemoveAt(list.index);
            }
        };

        // 要素の追加
        list.onAddCallback = (list) =>
        {
            // 新しいアイテムのIDを直前のアイテムに設定
            string newId = equipmentIds[0]; // デフォルトは最初のID

            // もしdataListにアイテムがあれば、その最後のIDを次のアイテムに設定
            if (dataList.Count > 0)
            {
                var lastItem = dataList[dataList.Count - 1];
                newId = lastItem.id; // 直前のIDを新しいアイテムのIDとして設定
            }

            dataList.Add(new ItemDropData { id = newId, groupId = 0, dropRarity = Rarity.D });
        };

        return list;
    }
   
}
#endif
[Serializable]
public class ItemDropData
{
    public string id;
    public int groupId;
    public Rarity dropRarity;
}

public enum Rarity
{
    SSS,
    SS,
    S,
    A,
    B,
    C,
    D
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WeaponEffectMaster", menuName = "Scriptable Objects/WeaponEffectMaster")]
public class WeaponEffectMaster : ScriptableObject
{
    private static WeaponEffectMaster _entity;

    public static WeaponEffectMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<WeaponEffectMaster>("Master/WeaponEffectMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(WeaponEffectMaster) + " not found");
                }
            }

            return _entity;
        }
    }

    [SerializeField] public  SerializableDictionary<string, List<WeaponEffectGroup>> weaponEffectGroups;
}

[Serializable]
public class WeaponEffectGroup
{
    public int id;
    public List<WeaponEffect> effects;
}

[Serializable]
public class WeaponEffect
{
    public EffectStatus effect;
    public int value;
}

#if UNITY_EDITOR
[CustomEditor(typeof(WeaponEffectMaster))]
public class WeaponEffectMasterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WeaponEffectMaster example = (WeaponEffectMaster)target;

        if (example.weaponEffectGroups == null)
        {
            example.weaponEffectGroups = new SerializableDictionary<string, List<WeaponEffectGroup>>();
        }
        // equipmentEffectDatas の ID 配列を作成
        string[] effectIds = new string[EquipmentMaster.Entity.equipData.Count];
        for (int i = 0; i < EquipmentMaster.Entity.equipData.Count; i++)
        {
            effectIds[i] = EquipmentMaster.Entity.equipData[i].id;
        }

        // キーをリストに変換して表示
        foreach (var key in example.weaponEffectGroups.Keys.ToList())
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Key編集
                EditorGUILayout.LabelField("Key", GUILayout.Width(50));
                // 現在のインデックスを取得
                int currentIndex = System.Array.IndexOf(effectIds, key);
                // インデックスが-1の場合、デフォルトで最初のアイテムを選択
                if (currentIndex == -1) currentIndex = 0;
                currentIndex = EditorGUILayout.Popup( currentIndex, effectIds, GUILayout.Width(100));
                // 選択された ID をキーにセット（key を更新）
                string newKey = effectIds[currentIndex];
                if (newKey != key)
                {
                    // 既存のキーを削除し、新しいキーで追加
                    var group = example.weaponEffectGroups[key];
                    example.weaponEffectGroups.Remove(key);
                    example.weaponEffectGroups[newKey] = group;
                }
                if (GUILayout.Button("Add Group"))
                {
                    example.weaponEffectGroups[key].Add(new WeaponEffectGroup()
                    {
                        id = example.weaponEffectGroups[key].Count,
                        effects = new List<WeaponEffect>(),
                    });
                }
                
            }
            EditorGUI.indentLevel++;
            GUIStyle coloredBoxStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };
            using (new EditorGUILayout.VerticalScope(coloredBoxStyle))
            {
                foreach (var effect in example.weaponEffectGroups[key])
                {
                    using (new EditorGUILayout.HorizontalScope("Box"))
                    {
                        EditorGUILayout.LabelField(effect.id.ToString(), GUILayout.Width(40));
                        if (GUILayout.Button("Add Effect"))
                        {
                            effect.effects.Add(new WeaponEffect());
                        }
                    }
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < effect.effects.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope("Box"))
                        {
                            var data = effect.effects[i];
                            data.effect =
                                (EffectStatus)EditorGUILayout.EnumPopup(data.effect,GUILayout.Width(100));
                            data.value =
                                EditorGUILayout.IntField(data.value,GUILayout.Width(60));
                            if (GUILayout.Button("Remove"))
                            {
                                effect.effects.RemoveAt(i);
                            }
                        }
                    }

                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }
        // 新しいアイテムを追加するためのボタン
        if (GUILayout.Button("Add New"))
        {
            // EquipmentMaster.Entity.equipData のデータをすべて weaponEffectGroups に追加
            foreach (var equipData in EquipmentMaster.Entity.equipData)
            {
                string newKey = equipData.id; // ID をキーとして使用

                // 新しい WeaponEffectGroup を作成
                List<WeaponEffectGroup> newGroup = new List<WeaponEffectGroup>();

                // weaponEffectGroups に追加
                if (!example.weaponEffectGroups.ContainsKey(newKey))
                {
                    example.weaponEffectGroups[newKey] = newGroup;
                }
            }
        }
    }
}
#endif

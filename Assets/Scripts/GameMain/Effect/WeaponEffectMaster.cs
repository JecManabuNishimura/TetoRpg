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

    [SerializeField] public  SerializableDictionary<string, WeaponEffectGroup> weaponEffectGroups;
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
            example.weaponEffectGroups = new SerializableDictionary<string, WeaponEffectGroup>();
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
                
                // Removeボタン
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    example.weaponEffectGroups.Remove(newKey);  // newKey を使用して削除
                }
            }

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Group ID", example.weaponEffectGroups[key].id.ToString());
                foreach (var data in example.weaponEffectGroups[key].effects.ToList())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // 各 WeaponEffect のフィールドを表示
                        data.value =
                            (EffectStatus)EditorGUILayout.EnumPopup(data.value);
                        effectGroup.effects[i].value =
                            EditorGUILayout.IntField(effectGroup.effects[i].value);

                        // WeaponEffect の削除ボタン
                        if (GUILayout.Button("Remove Effect"))
                        {
                            effectGroup.effects.RemoveAt(i);
                        }
                    }
                }
            }
            /*
            // WeaponEffectGroup のリスト（effects）を表示
            WeaponEffectGroup effectGroup = example.weaponEffectGroups[key];
            if (effectGroup.effects != null)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    for (int i = 0; i < effectGroup.effects.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // 各 WeaponEffect のフィールドを表示
                            effectGroup.effects[i].effect =
                                (EffectStatus)EditorGUILayout.EnumPopup(effectGroup.effects[i].effect);
                            effectGroup.effects[i].value =
                                EditorGUILayout.IntField(effectGroup.effects[i].value);

                            // WeaponEffect の削除ボタン
                            if (GUILayout.Button("Remove Effect"))
                            {
                                effectGroup.effects.RemoveAt(i);
                            }
                        }
                    }
                }
                */
            }

            // Effect の追加ボタン
            if (GUILayout.Button("Add Effect"))
            {
                effectGroup.effects.Add(new WeaponEffect());
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
                WeaponEffectGroup newGroup = new WeaponEffectGroup
                {
                    effects = new List<WeaponEffect>() // 空の effects リストを作成
                };

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

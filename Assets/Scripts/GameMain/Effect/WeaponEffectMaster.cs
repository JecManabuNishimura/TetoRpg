using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

//[CreateAssetMenu(fileName = "WeaponEffectMaster", menuName = "Scriptable Objects/WeaponEffectMaster")]
public class WeaponEffectMaster : MonoBehaviour //: ScriptableObject
{
    public static WeaponEffectMaster Instance;
    

    public  Dictionary<string, List<WeaponEffectGroup>> weaponEffectGroups = new ();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        
        ReadCSV("WeaponEffectList");
    }
    
    public void ReadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(',');
                string name = data[0];
                int groupId = int.Parse(data[1]);
                string effect = data[2];
                int value = int.Parse(data[3]);
                Enum.TryParse(effect, out EffectStatus state);

                if (weaponEffectGroups.ContainsKey(name))
                {
                    var group = weaponEffectGroups[name].FirstOrDefault(x => x.id == groupId);
                    if (group != null)
                    {
                        group.effects.Add(new WeaponEffect(state, value));
                    }
                    else
                    {
                        weaponEffectGroups[name].Add(new WeaponEffectGroup(groupId, new List<WeaponEffect>()
                        {
                            new WeaponEffect(state, value)
                        }));
                    }
                }
                else
                {
                    weaponEffectGroups.Add(name, new List<WeaponEffectGroup>());
                    weaponEffectGroups[name].Add(new WeaponEffectGroup(groupId, new List<WeaponEffect>()
                    {
                        new WeaponEffect(state, value)
                    }));
                }


            }
        }
        else
        {
            Debug.LogError(fileName + "が見つかりません。");
        }
        AssetDatabase.SaveAssets();  
        AssetDatabase.Refresh();
    }
}

[Serializable]
public class WeaponEffectGroup
{
    public WeaponEffectGroup(int id, List<WeaponEffect> effects)
    {
        this.id = id;
        this.effects = effects;
    }
    public int id;
    public List<WeaponEffect> effects;
}

[Serializable]
public class WeaponEffect
{
    public WeaponEffect(EffectStatus effectStatus, int value)
    {
        effect = effectStatus;
        this.value = value;
    }
    public EffectStatus effect;
    public int value;
}
/*
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
        
        // CSVの読み込みボタン
        if (GUILayout.Button("ReadCSV"))
        {
            example.weaponEffectGroups.Clear();
            example.ReadCSV("WeaponEffectList");
            EditorUtility.SetDirty(example);
            AssetDatabase.SaveAssets();  
        }
        
        // 各キーのグループを折りたたみ可能に表示
        foreach (var key in example.weaponEffectGroups.Keys.ToList())
        {
            // Foldoutでグループを折りたたみ
            bool isExpanded = EditorPrefs.GetBool("WeaponEffect_" + key, true); // 折りたたみ状態を保存する
            isExpanded = EditorGUILayout.Foldout(isExpanded, key); // Foldoutでキー表示

            // 状態を保存
            EditorPrefs.SetBool("WeaponEffect_" + key, isExpanded);

            if (isExpanded)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Key編集
                    EditorGUILayout.LabelField("Key", GUILayout.Width(50));

                    // 現在のインデックスを取得
                    int currentIndex = System.Array.IndexOf(effectIds, key);
                    if (currentIndex == -1) currentIndex = 0;
                    currentIndex = EditorGUILayout.Popup(currentIndex, effectIds, GUILayout.Width(100));

                    // 新しいIDを設定
                    string newKey = effectIds[currentIndex];
                    if (newKey != key)
                    {
                        var group = example.weaponEffectGroups[key];
                        example.weaponEffectGroups.Remove(key);
                        example.weaponEffectGroups[newKey] = group;
                    }
                }
                EditorGUI.indentLevel++;

                GUIStyle coloredBoxStyle = new GUIStyle(GUI.skin.box)
                {
                    border = new RectOffset(1, 1, 1, 1),
                    margin = new RectOffset(5, 5, 5, 5),
                    padding = new RectOffset(10, 10, 10, 10)
                };

                // グループ内容の表示
                using (new EditorGUILayout.VerticalScope(coloredBoxStyle))
                {
                    foreach (var effect in example.weaponEffectGroups[key])
                    {
                        using (new EditorGUILayout.HorizontalScope("Box"))
                        {
                            EditorGUILayout.LabelField(effect.id.ToString(), GUILayout.Width(40));
                        }
                        EditorGUI.indentLevel++;

                        for (int i = 0; i < effect.effects.Count; i++)
                        {
                            using (new EditorGUILayout.HorizontalScope("Box"))
                            {
                                var data = effect.effects[i];
                                data.effect = (EffectStatus)EditorGUILayout.EnumPopup(data.effect, GUILayout.Width(100));
                                data.value = EditorGUILayout.IntField(data.value, GUILayout.Width(60));
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
    }
    // 線を引くメソッド
    private void DrawLine(Color color, float height = 1f)
    {
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(height));
        EditorGUI.DrawRect(rect, color);  // 指定した色で矩形（線）を描画
    }
}
#endif*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class StageLoader : MonoBehaviour
{
    public SerializableDictionary<Stage, TreasureDropMaster> dropData = new();

    public TreasureDropMaster GetDropData()
    {
        return dropData[GameManager.nowStage];
    }
    private void Start()
    {
        GameManager.stageLoader = this;
    }
}
[CustomEditor(typeof(StageLoader))]
public class StageLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StageLoader example = (StageLoader)target;

        if (example.dropData == null)
        {
            example.dropData = new SerializableDictionary<Stage, TreasureDropMaster>();
        }

        // キーをリストに変換して表示
        foreach (var key in example.dropData.Keys.ToList())
        {
            EditorGUILayout.BeginHorizontal();

            // Key編集
            EditorGUILayout.LabelField("Key", GUILayout.Width(50));
            Stage newKey = (Stage)EditorGUILayout.EnumPopup(key, GUILayout.Width(100));

            // Value編集
            EditorGUILayout.LabelField("Value", GUILayout.Width(50));
            example.dropData[key] = (TreasureDropMaster)EditorGUILayout.ObjectField(example.dropData[key], typeof(TreasureDropMaster), true, GUILayout.Width(200));

            // Removeボタン
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                example.dropData.Remove(key);
            }

            // キーが変更された場合、Dictionaryを更新
            if (newKey != key)
            {
                TreasureDropMaster value = example.dropData[key];
                example.dropData.Remove(key);
                example.dropData.Add(newKey, value);
            }

            EditorGUILayout.EndHorizontal();
        }

        // 新しい要素を追加
        if (GUILayout.Button("Add New"))
        {
            example.dropData.Add(Stage.None, null);
        }

        EditorUtility.SetDirty(target); // ScriptableObjectの変更を保存
    }
}
[Serializable]
public enum Stage
{
    None,
    Stage1,
    Stage2,
    Stage3,
    Stage4,
}



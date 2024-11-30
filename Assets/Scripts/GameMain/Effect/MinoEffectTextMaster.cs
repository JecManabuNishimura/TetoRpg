using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MinoEffectTextMaster", menuName = "Scriptable Objects/MinoEffectTextMaster")]
public class MinoEffectTextMaster : ScriptableObject
{
    private static MinoEffectTextMaster _entity;

    public static MinoEffectTextMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<MinoEffectTextMaster>("Master/MinoEffectTextMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(MinoEffectTextMaster) + " not found");
                }
            }

            return _entity;
        }
    }

    public SerializableDictionary<string, string> effectExplanation = new();

    public string GetExplanationText(string key)
    {
        // Dictionaryから値を取得し、キーが存在しない場合はnullを返す
        if (effectExplanation.TryGetValue(key, out string explanation))
        {
            return explanation;  // キーが存在する場合、値を返す
        }

        return "";
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(MinoEffectTextMaster))]
public class MinoEffectTextMasterEditor : Editor
{
    private SerializedProperty effectExplanationProperty;

    private void OnEnable()
    {
        // SerializedObject を使って Dictionary を取得
        effectExplanationProperty = serializedObject.FindProperty("effectExplanation");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // SerializedObject のデータを最新に更新

        MinoEffectTextMaster example = (MinoEffectTextMaster)target;

        if (example.effectExplanation == null)
        {
            example.effectExplanation = new SerializableDictionary<string, string>();
        }

        // MinoEffectStatusMaster からキーを取得
        var statusMaster = MinoEffectStatusMaster.Entity;
        var statusKeys = statusMaster != null ? statusMaster.MinoEffectStatus : new System.Collections.Generic.List<string>();

        // effectExplanation のリストを表示
        foreach (var key in example.effectExplanation.Keys.ToList())
        {
            EditorGUILayout.BeginHorizontal();

            // Key編集（ドロップダウンで表示）
            int selectedIndex = Mathf.Max(0, statusKeys.IndexOf(key));
            int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, statusKeys.ToArray(), GUILayout.Width(100));

            string newKey = statusKeys[newSelectedIndex];

            // Value編集
            example.effectExplanation[key] = EditorGUILayout.TextArea(example.effectExplanation[key], GUILayout.Width(200));

            // Removeボタン
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                example.effectExplanation.Remove(key);
            }

            // キーが変更された場合、Dictionaryを更新
            if (newKey != key)
            {
                string value = example.effectExplanation[key];
                example.effectExplanation.Remove(key);
                example.effectExplanation.Add(newKey, value);
            }

            EditorGUILayout.EndHorizontal();
        }

        // 新しい要素を追加
        if (GUILayout.Button("Add New"))
        {
            // 新しいキーをドロップダウンリストから追加
            string newKey = statusKeys.FirstOrDefault(); // 最初のキーをデフォルトにする
            example.effectExplanation.Add(newKey, "");
        }

        EditorUtility.SetDirty(target); // ScriptableObjectの変更を保存

        serializedObject.ApplyModifiedProperties(); // 最後に変更を保存
    }
}
#endif
[Serializable]
public class MinoEffectExplanation
{
    public string key;
    public string value;
}
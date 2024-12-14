using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectTextMaster", menuName = "Scriptable Objects/EffectTextMaster")]
public class EffectTextMaster : ScriptableObject
{
    private static EffectTextMaster _entity;

    public static EffectTextMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<EffectTextMaster>("Master/EffectTextMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(EffectTextMaster) + " not found");
                }
            }

            return _entity;
        }
    }

    public SerializableDictionary<EffectStatus, string> effectExplanation = new();
}
#if UNITY_EDITOR
[CustomEditor(typeof(EffectTextMaster))]
public class ExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EffectTextMaster example = (EffectTextMaster)target;

        if (example.effectExplanation == null)
        {
            example.effectExplanation = new SerializableDictionary<EffectStatus, string>();
        }

        // キーをリストに変換して表示
        foreach (var key in example.effectExplanation.Keys.ToList())
        {
            EditorGUILayout.BeginHorizontal();

            // Key編集
            EditorGUILayout.LabelField("Key", GUILayout.Width(50));
            EffectStatus newKey = (EffectStatus)EditorGUILayout.EnumPopup(key, GUILayout.Width(100));

            // Value編集
            EditorGUILayout.LabelField("Value", GUILayout.Width(50));
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
            example.effectExplanation.Add(EffectStatus.None, "");
        }

        EditorUtility.SetDirty(target); // ScriptableObjectの変更を保存
    }
}
#endif
[Serializable]
public class EffectExplanation
{
    public EffectStatus key;
    public string value;
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            dictionary.Add(keys[i], values[i]);
        }
    }

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => dictionary[key] = value;
    }

    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
    }

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public void Clear() => dictionary.Clear();

    public Dictionary<TKey, TValue>.KeyCollection Keys => dictionary.Keys;
}
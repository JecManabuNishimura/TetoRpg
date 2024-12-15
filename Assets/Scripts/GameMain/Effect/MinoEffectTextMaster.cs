using System;
using System.Collections.Generic;
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

    // スクリプタブルオブジェクトで自作クラスを使う場合は必ず、自作のDictionaryとメンバーはSerializeしなければならない
    public SerializableDictionary<string, EffectTextData> effectExplanation = new();

    [Serializable]
    public class EffectTextData
    {
        [SerializeField]public string Text;
        [SerializeField]public bool Negative;

        public EffectTextData(string text, bool negative)
        {
            Text = text;
            Negative = negative;
        }
    }
    
    public EffectTextData GetExplanationText(string key)
    {
        // Dictionaryから値を取得し、キーが存在しない場合はnullを返す
        if (effectExplanation.TryGetValue(key, out EffectTextData explanation))
        {
            return explanation;  // キーが存在する場合、値を返す
        }

        return null;
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
            example.effectExplanation = new SerializableDictionary<string, MinoEffectTextMaster.EffectTextData>();
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
            var effectTextData = example.effectExplanation[key];
            effectTextData.Text = EditorGUILayout.TextArea(effectTextData.Text, GUILayout.Width(200));
            effectTextData.Negative = EditorGUILayout.Toggle(effectTextData.Negative, GUILayout.Width(30));

            // Removeボタン
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                example.effectExplanation.Remove(key);
            }

            // キーが変更された場合、Dictionaryを更新
            if (newKey != key)
            {
                MinoEffectTextMaster.EffectTextData value = effectTextData;
                example.effectExplanation.Remove(key);
                example.effectExplanation.Add(newKey, value);
            }

            EditorGUILayout.EndHorizontal();
        }

        // 新しい要素を追加
        if (GUILayout.Button("ReadEffect"))
        {
            var Keys = statusMaster != null ? statusMaster.MinoEffectStatus : new System.Collections.Generic.List<string>();
            foreach (var key in Keys)
            {
                if (!example.effectExplanation.ContainsKey(key))
                {

                    example.effectExplanation.Add(key,new MinoEffectTextMaster.EffectTextData("",false));
                }
            }
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
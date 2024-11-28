using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MinoEffectStatusMaster", menuName = "Scriptable Objects/MinoEffectStatusMaster")]
public class MinoEffectStatusMaster : ScriptableObject
{
    private static MinoEffectStatusMaster _entity;

    public static MinoEffectStatusMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<MinoEffectStatusMaster>("Master/MinoEffectStatusMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(MinoEffectStatusMaster) + " not found");
                }
            }

            return _entity;
        }
    }

    public List<string> MinoEffectStatus = new();
}
[CustomEditor(typeof(MinoEffectStatusMaster))]
public class MinoEffectStatusMasterEditor : Editor
{
    private SerializedProperty minoEffectStatusProperty;

    private void OnEnable()
    {
        // SerializedObject を使って List のデータを反映させる
        minoEffectStatusProperty = serializedObject.FindProperty("MinoEffectStatus");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // SerializedObject のデータを最新に更新

        MinoEffectStatusMaster example = (MinoEffectStatusMaster)target;

        // MinoEffectStatus のリストを表示
        EditorGUILayout.PropertyField(minoEffectStatusProperty, true);

        // 新しい要素を追加
        if (GUILayout.Button("Add New"))
        {
            example.MinoEffectStatus.Add("");
            serializedObject.ApplyModifiedProperties(); // 変更を反映
        }

        // ScriptableObject の変更を Unity に反映させる
        EditorUtility.SetDirty(target);

        serializedObject.ApplyModifiedProperties(); // 最後に変更を保存
    }
}

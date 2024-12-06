using UnityEditor;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    // DebugScriptがアタッチされているゲームオブジェクトの子要素を削除するメソッド
    public void RemoveAllChildren()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DebugScript))]
public class DebugScriptEditor : Editor
{
    private DebugScript debugScript;

    private void OnEnable()
    {
        // インスペクタが表示される際にターゲットのScriptを取得
        debugScript = (DebugScript)target;
    }

    public override void OnInspectorGUI()
    {
        // 親クラスのGUI描画
        base.OnInspectorGUI();

        // ボタンをインスペクタに追加
        if (GUILayout.Button("Remove All Children"))
        {
            // ボタンが押されたときに子要素を削除
            debugScript.RemoveAllChildren();
        }
    }
}
#endif
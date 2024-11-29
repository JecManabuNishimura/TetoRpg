using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MinoEffectData", menuName = "Scriptable Objects/MinoEffectData")]
public class MinoEffectData : ScriptableObject
{
    private static MinoEffectData _entity;

    public static MinoEffectData Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                string assetPath = "Assets/Data/MinoEffectData.asset";
                _entity = AssetDatabase.LoadAssetAtPath<MinoEffectData>(assetPath);

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(MinoEffectData) + " not found");
                }
                AssetDatabase.SaveAssets();  // ここで保存
            }
            else
            {
                // 必要であれば変更を保存
                EditorUtility.SetDirty(_entity);
                AssetDatabase.SaveAssets();  // 変更を保存
            }

            return _entity;
        }
    }
    // 番号のリスト
    public List<MinoEffect> MinoEffects;

    public List<string> GetMinoEffect(int index)
    {
        return MinoEffects.First(_ => _.numbers == index).selectedGroupOptions;
    }
}


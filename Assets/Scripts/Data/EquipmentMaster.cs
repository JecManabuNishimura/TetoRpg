using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipmentMaster", menuName = "Scriptable Objects/EquipmentMaster")]
public class EquipmentMaster : ScriptableObject
{
    public List<EquipmentData> equipData = new List<EquipmentData>();
    private static EquipmentMaster _entity;

    public static EquipmentMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<EquipmentMaster>("Master/EquipmentMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(EquipmentMaster) + " not found");
                }
            }

            return _entity;
        }
    }

    public EquipmentData GetEquipmentData(string index)
    {
        if (index == null) return null;
        foreach (var d in equipData)
        {
            if (d.id == index)
            {
                return d;
            }
        }

        Debug.LogError("防具マスターに登録されていない番号:" + index);
        return null;
    }
    public void ReadCSV(string csvFileName)
    {
        // CSVファイルのフルパスを取得
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);

        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split('\n');

            // 最初の行（ヘッダー）はスキップ
            for (int i = 1; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(',');

                // 空行をスキップ
                if (data.Length < 5) continue;

                string id = data[0];
                string name = data[1];
                int hp = int.Parse(data[2]);
                int atk = int.Parse(data[3]);
                int def = int.Parse(data[4]);
                
                List<EffectUpStatus> upState = new ();
                equipData.Add(new EquipmentData(id,
                                                name,
                                                new Status(hp, atk, def,0)));
            }
        }
        else
        {
            Debug.LogError(csvFileName + "がResourcesフォルダに見つかりません。");
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(EquipmentMaster))]
public class EquipmentMasterEditor : Editor
{
    // EquipmentMasterオブジェクトの参照を保持
    private EquipmentMaster equipmentMaster;

    private void OnEnable()
    {
        // インスペクタが表示される際に、ターゲットのScriptableObjectを取得
        equipmentMaster = (EquipmentMaster)target;
    }

    public override void OnInspectorGUI()
    {
        // デフォルトのインスペクタUIを描画
        DrawDefaultInspector();

        // CSVファイル名をインスペクタに表示するフィールドを追加（オプション）
        GUILayout.Space(10);
        GUILayout.Label("CSVファイルの読み込み", EditorStyles.boldLabel);

        if (GUILayout.Button("CSVを読み込む"))
        {
            equipmentMaster.equipData.Clear();
            // CSVの読み込み処理を呼び出す
            equipmentMaster.ReadCSV("EquipmentData");
        }
    }
}
#endif
[Serializable]
public class EquipmentData
{
    public string id;
    public string name;
    public int gourpId;
    public Status status;

    public EquipmentData(string id, string name, Status status)
    {
        this.id = id;
        this.name = name;
        this.status = status;
    }

    public Status GetTotalStatus()
    {
        Status state = status;
        Dictionary<EffectStatus, Action<int>> effectActions = new Dictionary<EffectStatus, Action<int>>()
        {
            { EffectStatus.AtkUp, value => state.atk += value },
            { EffectStatus.HpUp, value => state.hp += value },
            { EffectStatus.AtkDown, value => state.atk -= value },
            { EffectStatus.DefDown, value => state.def -= value },
            { EffectStatus.None, value => {} } // 何もしない
        };
        try
        {
            WeaponEffectMaster.Entity.GetWeaponEffect(id, gourpId).effects
                .Where(e => effectActions.ContainsKey(e.effect))
                .ToList()
                .ForEach(e => effectActions[e.effect](e.value));
        }
        catch (Exception e)
        {
            Debug.Log(id + ":" + gourpId);
            throw;
        }
        
        return state;

    }
}

[Serializable]
public struct EquipmentEffectData
{
    public int id;
    public string weaponName;

    public EquipmentEffectData(int id, string weaponName)
    {
        this.id = id;
        this.weaponName = weaponName;
    }
}

[Serializable]
public enum EffectStatus
{
    None,
    HpUp,
    HpDown,                 // Hp量ダウン
    AtkUp,
    AtkDown,                // 攻撃量ダウン
    DefUp,
    DefDown,                // 防御力ダウン
    CriticalUp,
    HealDropUp,
    HealPowerUp,            // 回復量Up
    AllAttackDropUp,        // 全体攻撃出現率アップ    
    DamageSuctionHp,        // 与えたダメージをHpとして吸収
    DamageReduction,        // ダメージ量軽減
    PenetratingDamage,      // 貫通ダメージ
    DoubleAttack,           // 2回攻撃
    HealEveryTurn,          // 毎ターン回復
    
    
}

[Serializable]
public class EquipmentUniqueData
{
    public EquipmentUniqueData(string weaponId, int groupID)
    {
        WeaponId = weaponId;
        this.groupID = groupID;
    }

    public string WeaponId;
    public int groupID;
    public static bool operator == (EquipmentUniqueData a, EquipmentUniqueData b)
    {
        // 両方が null の場合は等しいとみなす
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            return true;

        // 一方が null の場合、もう一方は null でないので異なるとみなす
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            return false;

        // それ以外の場合は通常の比較
        return a.WeaponId == b.WeaponId && a.groupID == b.groupID;
    }
    
    public static bool operator != (EquipmentUniqueData a, EquipmentUniqueData b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        if (obj is EquipmentUniqueData other)
        {
            return WeaponId == other.WeaponId && groupID == other.groupID;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(WeaponId, groupID);
    }
}
[Serializable]
public struct EffectUpStatus
{
    public EffectStatus effect;
    public int upState;

    public EffectUpStatus(EffectStatus effect, int upState)
    {
        this.effect = effect;
        this.upState = upState;
    }
}

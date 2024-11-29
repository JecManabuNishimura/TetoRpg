using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipmentMaster", menuName = "Scriptable Objects/EquipmentMaster")]
public class EquipmentMaster : ScriptableObject
{
    public List<EquipmentData> equipData = new List<EquipmentData>();
    public List<EquipmentEffectData> equipmentEffectDatas = new List<EquipmentEffectData>();
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
    public void ReadCSV(string csvFileName,string effectCsvFileName)
    {
        // CSVファイルのフルパスを取得
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        TextAsset effectCsvFile = Resources.Load<TextAsset>(effectCsvFileName);
        if (effectCsvFile != null)
        {
            string[] lines = effectCsvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            // 最初の行（ヘッダー）はスキップ
            for (int i = 1; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(',');
                int id = int.Parse(data[0]);
                string WeaponId = data[1];
                string EffectName = data[2];
                int value = int.Parse(data[3]);
                if (!Enum.TryParse(EffectName, true, out EffectStatus Type))
                {
                    Type = EffectStatus.None;
                }
                equipmentEffectDatas.Add(new EquipmentEffectData(id,WeaponId, Type,value));
            }
        }
        else
        {
            Debug.LogError(effectCsvFileName + "がResourcesフォルダに見つかりません。");
        }
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

                // データをリストに追加
                var effectdata =  equipmentEffectDatas.Where(x => x.weaponName == id).ToList();
                List<EffectUpStatus> upState = new ();
                foreach (var ed in effectdata)
                {
                    upState.Add(new EffectUpStatus(ed.effect,ed.value));
                }
                equipData.Add(new EquipmentData(id,
                                                name,
                                                new Status(hp, atk, def),
                                                upState.ToArray()));
            }
        }
        else
        {
            Debug.LogError(csvFileName + "がResourcesフォルダに見つかりません。");
        }
    }
}
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
            equipmentMaster.equipmentEffectDatas.Clear();
            equipmentMaster.equipData.Clear();
            // CSVの読み込み処理を呼び出す
            equipmentMaster.ReadCSV("EquipmentData", "EffectData");
        }
    }
}

[Serializable]
public class EquipmentData
{
    public string id;
    public string name;
    public Status status;
    public EffectUpStatus[] effect;

    public EquipmentData(string id, string name, Status status, EffectUpStatus[] effect)
    {
        this.id = id;
        this.name = name;
        this.status = status;
        this.effect = effect;
    }

    public Status GetTotalStatus()
    {
        Status state = status;
        foreach(var e in effect)
        {
            switch (e.effect)
            {
                case EffectStatus.AtkUp:
                    state.atk += e.upState;
                    break;
                case EffectStatus.HpUp:
                    state.hp += e.upState;
                    break;
                case EffectStatus.None:
                    break;
            }
        }
        return state;

    }
}

[Serializable]
public struct EquipmentEffectData
{
    public int id;
    public string weaponName;
    public EffectStatus effect;
    public int value;

    public EquipmentEffectData(int id, string weaponName, EffectStatus effect,int value)
    {
        this.id = id;
        this.weaponName = weaponName;
        this.effect = effect;
        this.value = value;
    }
}

[Serializable]
public enum EffectStatus
{
    None,
    HpUp,
    AtkUp,
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

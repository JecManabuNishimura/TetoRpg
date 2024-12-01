using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "Equipment/New Equipment Database")]
public class EquipmentDatabase : ScriptableObject
{
    public List<EquipmentSpriteData> weaponData;
    public List<EquipmentSpriteData> shieldData;
    public List<EquipmentSpriteData> helmetData;
    public List<EquipmentSpriteData> armorData;
    private static EquipmentDatabase _entity;

    public static EquipmentDatabase Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                string assetPath = "Master/EquipmentDatabase";
                _entity = Resources.Load<EquipmentDatabase>(assetPath);

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(EquipmentDatabase) + " not found");
                }
            }

            return _entity;
        }
    }
    public EquipmentSpriteData GetEquipmentSpriteData(string id)
    {
        switch (id.Substring(0, 2))
        {
            case "We":
                foreach (var data in weaponData.Where(data => data.weaponId == id))
                {
                    return data;
                }
                break;
            case "Sh":
                foreach (var data in shieldData.Where(data => data.weaponId == id))
                {
                    return data;
                }
                break;
            case "He":
                foreach (var data in helmetData.Where(data => data.weaponId == id))
                {
                    return data;
                }
                break;
            case "Ar":
                foreach (var data in armorData.Where(data => data.weaponId == id))
                {
                    return data;
                }
                break;
        }
        return null;
    }
}
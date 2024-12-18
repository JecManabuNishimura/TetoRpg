using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class Player : CharactorData, ICharactor
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private ParticleSystem healingEffect;
    [SerializeField] private TextMeshPro damageText;
    [SerializeField] private Slider slider;

    //[SerializeField] private MinoCreater[] minoCreaters;
    public Status totalStatus;
    public BelongingsEquipment belongingsEquipment = new ();
    
    // 装備しているMino
    public List<EquipmentUniqueData> belongingsMino = new()
    {
        new EquipmentUniqueData("0",0), 
        new EquipmentUniqueData("1",0),
        new EquipmentUniqueData("2",0),
        new EquipmentUniqueData("3",0),
        new EquipmentUniqueData("4",0),
        new EquipmentUniqueData("5",0),
        new EquipmentUniqueData("6",0),
        
    };
    
    // 所持しているMino
    public List<EquipmentUniqueData> haveMinoList = new ()
    {
        new EquipmentUniqueData("0",0), 
        new EquipmentUniqueData("1",0),
        new EquipmentUniqueData("2",0),
        new EquipmentUniqueData("3",0),
        new EquipmentUniqueData("4",0),
        new EquipmentUniqueData("5",0),
        new EquipmentUniqueData("6",0),
    };

    public List<EquipmentUniqueData> haveWeaponList = new ()
    {
        new EquipmentUniqueData("We01",0),
    };
    public List<EquipmentUniqueData> haveShieldList = new ()
    {
        new EquipmentUniqueData("Sh01",0)
    };
    public List<EquipmentUniqueData> haveHelmetList = new ()
    {
        new EquipmentUniqueData("He01",0)
    };
    public List<EquipmentUniqueData> haveArmorList = new ()
    {
        new EquipmentUniqueData("Ar01",0)
    };

    public List<EquipmentUniqueData> HaveMinoList => haveMinoList;
    public List<EquipmentUniqueData> BelongingsMino => belongingsMino;

    public Dictionary<string,int > BelongingsMinoEffect = new ();

    void Awake()
    {
        GameManager.player = this;
        damageText.enabled = false;
    }


    public void Initialize()
    {
        slider.value = 1;
        
        foreach (var state in MinoEffectStatusMaster.Entity.MinoEffectStatus)
        {
            BelongingsMinoEffect.Add(state,0);    
        }
        
        UpdateStatus();
        SetBelongingsMinoEffect();
        totalStatus.hp = totalStatus.maxHp;
        hpText.text = totalStatus.hp.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.menuFlag = true;
            MenuManager.Instance.OpenMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuManager.Instance.CloseMenu();
        }
    }

    public void SetBelongingsMinoEffect()
    {
        var keys = new List<string>(BelongingsMinoEffect.Keys); // 既存のすべてのキーを取得

        foreach (var key in keys)
        {
            BelongingsMinoEffect[key] = 0; // 値をリセット
        }
        foreach (var val in belongingsMino)
        {
            var data = MinoData.Entity.GetMinoEffect(val);
            foreach (var d in data)
            {
                BelongingsMinoEffect[d.effect] += d.value;
            }
        }
    }
    
    public void SetEquipment(EqupmentPart part,EquipmentUniqueData data)
    {
        switch(part)
        {
            case EqupmentPart.Weapon:
                // 同じものを装備する場合は外す
                belongingsEquipment.weapon = belongingsEquipment.weapon == data ? null : data;
                break;
            case EqupmentPart.Shield:
                belongingsEquipment.shield = belongingsEquipment.shield == data ? null : data;
                break;
            case EqupmentPart.Helmet:
                belongingsEquipment.helmet = belongingsEquipment.helmet == data ? null : data;
                break;
            case EqupmentPart.Armor:
                belongingsEquipment.armor = belongingsEquipment.armor == data ? null : data;
                break;
        }

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        totalStatus = status;

        // 各装備データを取得し、null チェックを行う
        if (belongingsEquipment.weapon != null)
        {
            var weaponData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.weapon.WeaponId);
            weaponData.groupId = belongingsEquipment.weapon.groupID;
            totalStatus += weaponData.GetTotalStatus();
        }

        if (belongingsEquipment.shield != null)
        {
            var shieldData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.shield.WeaponId);
            shieldData.groupId = belongingsEquipment.shield.groupID;
            totalStatus +=  shieldData.GetTotalStatus();
        }
        
        if (belongingsEquipment.helmet != null)
        {
            var helmetData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.helmet.WeaponId);
            helmetData.groupId = belongingsEquipment.helmet.groupID;
            totalStatus += helmetData.GetTotalStatus();
        }

        if (belongingsEquipment.armor != null)
        {
            var armorData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.armor.WeaponId);
            armorData.groupId = belongingsEquipment.armor.groupID;
            totalStatus += armorData.GetTotalStatus();
        }
    }
    
    public void UpdateHp()
    {
        hpText.text = totalStatus.hp.ToString();
    }

    public bool AcquisitionMino(EquipmentUniqueData data)
    {
        if (!haveMinoList.Contains(data))
        {
            haveMinoList.Add(data);
            return true;
        }

        return false;
    }
    public bool AcquisitionItem(EquipmentUniqueData data)
    {
        var type = data.WeaponId.Substring(0, 2);
        switch (type)
        {
            case "We":
                if (!haveWeaponList.Contains(data))
                {
                    haveWeaponList.Add(data);
                    return true;
                }
                break;
            case "Sh":
                if (!haveShieldList.Contains(data))
                {
                    haveShieldList.Add(data);
                    return true;
                }
                break;
            case "He":
                if (!haveHelmetList.Contains(data))
                {
                    haveHelmetList.Add(data);
                    return true;
                }
                break;
            case "Ar":
                if (!haveArmorList.Contains(data))
                {
                    haveArmorList.Add(data);
                    return true;
                }
                break;
        }

        return false;
    }

    public EquipmentUniqueData GetBelongingsMino(int index)
    { 
        if (index >= belongingsMino.Count)
        {
            Debug.LogError("所持容量より多いです:Player");
            return null;
        }
        return belongingsMino[index];
    }
    
    public void Healing()
    {
        totalStatus.hp += GameManager.healingPoint;
        if (totalStatus.hp >= totalStatus.maxHp)
        {
            totalStatus.hp = totalStatus.maxHp;
        }
        var part = Instantiate(healingEffect);
        part.transform.position = transform.position;
        UpdateHp();
        slider.value = (float)totalStatus.hp / (float)totalStatus.maxHp;
    }

    public void Damage(int damage)
    {
        int newDamage = (damage / 2) - (totalStatus.def / 4);
        if (newDamage < 0)
        {
            newDamage = 0;
        }
        damageText.enabled = true;
        damageText.text = newDamage.ToString();
        damageText.transform.GetComponent<Animator>().Play("DamageText",0,0);
        totalStatus.hp -= newDamage;
        UpdateHp();
        slider.value = (float)totalStatus.hp / (float)totalStatus.maxHp;
    }

    [Serializable]
    public class BelongingsEquipment
    {
        public EquipmentUniqueData weapon;
        public EquipmentUniqueData shield;
        public EquipmentUniqueData helmet;
        public EquipmentUniqueData armor;
    }

    
}

public enum EqupmentPart
{
    Weapon,
    Shield,
    Helmet,
    Armor,
}
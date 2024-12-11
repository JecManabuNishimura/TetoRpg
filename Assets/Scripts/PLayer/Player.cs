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
    public List<int> belongingsMino = new()
    {
        0,1,2,3,4,5,6,
    };
    
    // 所持しているMino
    public List<int> haveMinoList = new List<int>()
    {
        0,1,2,3,4,5,6,
    };

    public List<string> haveWeaponList = new List<string>()
    {
        "We01",
    };
    public List<string> haveShieldList = new List<string>()
    {
        "Sh01",
    };
    public List<string> haveHelmetList = new List<string>()
    {
        "He01",
    };
    public List<string> haveArmorList = new List<string>()
    {
        "Ar01",
    };

    public List<int> HaveMinoList => haveMinoList;
    public List<int> BelongingsMino => belongingsMino;

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
                BelongingsMinoEffect[d]++;
            }
        }
    }
    
    public void SetEquipment(EqupmentPart part,string id)
    {
        switch(part)
        {
            case EqupmentPart.Weapon:
                // 同じものを装備する場合は外す
                belongingsEquipment.weaponId = belongingsEquipment.weaponId == id ? null : id;
                break;
            case EqupmentPart.Shield:
                belongingsEquipment.shieldId = belongingsEquipment.shieldId == id ? null : id;
                break;
            case EqupmentPart.Helmet:
                belongingsEquipment.helmetId = belongingsEquipment.helmetId == id ? null : id;;
                break;
            case EqupmentPart.Armor:
                belongingsEquipment.armorId = belongingsEquipment.armorId == id ? null : id;;
                break;
        }

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        // ミノ効果追加
        totalStatus = status;

        // 各装備データを取得し、null チェックを行う
        var weaponData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.weaponId);
        if (weaponData != null)
        {
            totalStatus += weaponData.GetTotalStatus();
        }

        var shieldData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.shieldId);
        if (shieldData != null)
        {
            totalStatus += shieldData.GetTotalStatus();
        }

        var helmetData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.helmetId);
        if (helmetData != null)
        {
            totalStatus += helmetData.GetTotalStatus();
        }

        var armorData = EquipmentMaster.Entity.GetEquipmentData(belongingsEquipment.armorId);
        if (armorData != null)
        {
            totalStatus += armorData.GetTotalStatus();
        }
    }
    
    public void UpdateHp()
    {
        hpText.text = totalStatus.hp.ToString();
    }

    public void AcquisitionMino(int id)
    {
        if (!haveMinoList.Contains(id))
        {
            haveMinoList.Add(id);    
        }
    }
    public void AcquisitionItem(string itemId)
    {
        var type = itemId.Substring(0, 2);
        switch (type)
        {
            case "We":
                if (!haveWeaponList.Contains(itemId))
                {
                    haveWeaponList.Add(itemId);
                }
                break;
            case "Sh":
                if (!haveShieldList.Contains(itemId))
                {
                    haveShieldList.Add(itemId);
                }
                break;
            case "He":
                if (!haveHelmetList.Contains(itemId))
                {
                    haveHelmetList.Add(itemId);
                }
                break;
            case "Ar":
                if (!haveArmorList.Contains(itemId))
                {
                    haveArmorList.Add(itemId);
                }
                break;
            
        }
    }

    public int GetBelongingsMino(int index)
    { 
        if (index >= belongingsMino.Count)
        {
            Debug.LogError("所持容量より多いです:Player");
            return -1;
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
        public string weaponId;
        public string shieldId;
        public string helmetId;
        public string armorId;
    }
}

public enum EqupmentPart
{
    Weapon,
    Shield,
    Helmet,
    Armor,
}
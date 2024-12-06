using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    
    public GameObject equipmentObj;
    public UiController uiController;
    
    public GameObject minoObj;
    public GameObject armorObj;

    public TabData tabData;
    public MinoViewData minoData;
    public ArmorData armorData;
    
    public int selectIndex;
    private bool openFlag;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        openFlag = false;
        equipmentObj.SetActive(false);
        minoObj.SetActive(false);
        armorObj.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            uiController.Horizontal(false);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            uiController.Horizontal(true);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            uiController.Vertical(true);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            uiController.Vertical(false);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            uiController.SelectMenu();
        }
    }

    public void OpenMenu()
    {
        if (openFlag) return;
        openFlag = true;
        equipmentObj.SetActive(true);
        armorData.hpText.text = GameManager.player.status.hp.ToString();
        armorData.atkText.text = GameManager.player.status.atk.ToString();
        armorData.defText.text = GameManager.player.status.def.ToString();
        minoObj.SetActive(true);
        uiController.ChangeMenu(SelectView.TabSelect);
    }

    public void CloseMenu()
    {
        uiController.CloseMenu();
        openFlag = false;
    }
/*
    public void GroupChildReset(Transform tran)
    {
        for (int i = tran.childCount - 1; i >= 0; i--)
        {
            Destroy(tran.GetChild(i).gameObject);
        }
    }*/
}

[Serializable]
public class TabData
{
    public Image[] image;
    public Sprite ActiveSprite;
    public Sprite NonActiveSprite;
    public GameObject hideBlockGroundImg;
}

[Serializable]
public class MinoViewData
{
    public ScrollRect scrollRect;
    public SimpleCircleLayoutGroup circleLayoutGroup;
    public GameObject haveContent;
    public GameObject itemObj;
    public GridLayoutGroup gridLayoutGroup;
    public VerticalLayoutGroup belongingsEffectGroup;
    public VerticalLayoutGroup haveEffectGroup;
    public GameObject minoEffectObj;
    public GameObject CursorObj;
}

[Serializable]
public class ArmorData
{
    public GameObject effectObj;
    public GameObject HaveItemObj;
    public GameObject CursorIcon;
    public HorizontalLayoutGroup BelongingsGroup;
    public VerticalLayoutGroup EffectGroup;
    public GridLayoutGroup HaveItemGroup;
    public VerticalLayoutGroup ExplantionGroup;
    public VerticalLayoutGroup ExplantionEffectGroup;
    public List<SpriteSetting> BelongingsSprite;
    public GameObject itemContent;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    
}
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyMethods;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UiController : MonoBehaviour
{
    private MenuContext context;
    
    void Start()
    {
        context = new MenuContext();
        context.Initialize(this);
    }

    private void Update() => context.Update();
    public void SelectMenu() => context.SelectMenu();
    public void ChangeMenu(SelectView next) => context.ChangeState(next);
    public void CloseMenu() => context.CloseMenu();
    public void Horizontal(bool right) => context.Horizontal(right);
    public void Vertical(bool up) => context.Vertical(up);
}

public class MenuContext
{
    private Dictionary<SelectView, IMenu> stateTable;
    private IMenu currentState;
    private IMenu previousState;
    public void Initialize(UiController menu)
    {
        Dictionary<SelectView, IMenu> table = new()
        {
            { SelectView.None ,new None()},
            { SelectView.TabSelect ,new TabSelect(menu,MenuManager.Instance)},
            { SelectView.Armor ,new ArmorView(menu,MenuManager.Instance)},
            { SelectView.HaveMino ,new HaveMinoView(menu)},
        };
        stateTable = table;
        currentState = stateTable[SelectView.None];
        ChangeState(SelectView.None);
    }

    public void ChangeState(SelectView next)
    {
        if (stateTable is null) return;
        if (currentState is null || currentState.State == next)
        {
            return;
        }

        var nextState = stateTable[next];
        previousState = currentState;
        previousState?.Exit();
        currentState = nextState;
        currentState.Entry();
    }

    
    public void Update() => currentState?.Update();
    public void Horizontal(bool right) => currentState?.Horizontal(right);
    public void Vertical(bool right) => currentState?.Vertical(right);
    public void SelectMenu() => currentState?.SelectMenu();
    public void CloseMenu() => currentState?.CloseMenu();
}

public class None : IMenu
{
    public SelectView State => SelectView.None;
    public void Horizontal(bool right)
    {
        
    }

    public void Vertical(bool up)
    {
        
    }
    public void Exit()
    {
        MenuManager.Instance.minoObj.SetActive(true);
        MenuManager.Instance.armorObj.SetActive(false);
    }

    public void Entry()
    {
        MenuManager.Instance.equipmentObj.SetActive(false);
        GameManager.menuFlag = false;
    }

    public void Update()
    {
    }

    public void SelectMenu()
    {
    }

    public void CloseMenu()
    {
    }
}
public class TabSelect : EquipmentDataCreate, IMenu
{
    public SelectView State => SelectView.TabSelect;
    private UiController menu;

    public TabSelect(UiController menu,MenuManager menuManager)
    {
        this.menu = menu;
        tabData = menuManager.tabData;
        minoObj = menuManager.minoObj;
        armorObj = menuManager.armorObj;
        armorData = menuManager.armorData;
        minoData = menuManager.minoData;
    }

    private int selectTabNumber;
    
     public void Horizontal(bool right)
    {
        selectTabNumber += right ? 1 : -1;
        selectTabNumber = Mathf.Clamp(selectTabNumber, 0, tabData.image.Length - 1);

        foreach (var t in tabData.image)
        {
            t.sprite = tabData.NonActiveSprite;
        }
        tabData.image[selectTabNumber].sprite = tabData.ActiveSprite;

        switch (selectTabNumber)
        {
            case 0:
                minoObj.SetActive(true);
                armorObj.SetActive(false);
                break;
            case 1:
                minoObj.SetActive(false);
                armorObj.SetActive(true);
                armorData.CursorIcon.SetActive(false);
                break;
        }
    }

    public void Vertical(bool up) { }

    public void Exit()
    {
        tabData.hideBlockGroundImg.SetActive(false);
    }

    public void Entry()
    {
        tabData.hideBlockGroundImg.SetActive(true);
        Initialize();
    }

    public void Update() { }

    public void SelectMenu()
    {
        switch (selectTabNumber)
        {
            case 0:
                menu.ChangeMenu(SelectView.HaveMino);
                break;
            case 1:
                menu.ChangeMenu(SelectView.Armor);
                break;
        }
    }

    public void CloseMenu()
    {
        menu.ChangeMenu(SelectView.None);
    }

    void Initialize()
    {
        
        foreach (var t in tabData.image)
        {
            t.sprite = tabData.NonActiveSprite;
        }
        tabData.image[selectTabNumber].sprite = tabData.ActiveSprite;
        UpdateBelongingsMinoImage();
        UpdateBelongingsWeaponSprite();
        OpenEquipmentEffectArmor();
        OpenEquipmentMino();
        CreateHaveArmorData(0);
        UpdateArmorData();
        armorData.ExplantionGroup.gameObject.SetActive(false);
        armorData.ExplantionEffectGroup.gameObject.SetActive(false);
        MenuManager.Instance.minoData.belongingsEffectGroup.transform.ChildClear();
        MenuManager.Instance.minoData.haveEffectGroup.transform.ChildClear();
    }

    void UpdateBelongingsMinoImage()
    {
        minoData.circleLayoutGroup.transform.ChildClear();;
        foreach (var mino in GameManager.player.BelongingsMino)
        {
            var data = GameObject.Instantiate(minoData.itemObj, minoData.circleLayoutGroup.transform, true);
            data.GetComponent<MinoCreater>().UpdateId(mino);
            data.GetComponent<MinoCreater>().CreateMino();
        }
    }
    void OpenEquipmentMino()
    {
        minoData.gridLayoutGroup.transform.ChildClear();;
        foreach (var mino in GameManager.player.HaveMinoList)
        {
            var data = GameObject.Instantiate( minoData.itemObj, minoData.haveContent.transform, true);
            data.GetComponent<MinoCreater>().UpdateId(mino);
            data.GetComponent<MinoCreater>().CreateMino();
        }
    }
}
//=========================================================================
// ミノ装備画面
//=========================================================================
public class HaveMinoView:EquipmentDataCreate,IMenu
{
    private enum NowMode
    {
        BelongingsSelect,
        MinoSelect,
    }
    private UiController menu;
    public HaveMinoView(UiController menu) => this.menu = menu;
    private RectTransform[] gridItems;    // Gridの子要素
    private int currentIndex = 0;
    private Color defaultColor;
    private int columns = 3;
    private Tweener shakeTweener;
    private Vector3 shakeInitPosition;

    private NowMode nowMode = NowMode.BelongingsSelect;

    public SelectView State => SelectView.HaveMino;
    
    public void Entry()
    {
        // GridLayoutGroupの子オブジェクトを取得
        int childCount = MenuManager.Instance.minoData.gridLayoutGroup.transform.childCount;
        gridItems = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            gridItems[i] = MenuManager.Instance.minoData.gridLayoutGroup.transform.GetChild(i).GetComponent<RectTransform>();
        }

        BelongingMinoExplanation();


        defaultColor = gridItems[0].GetComponent<Image>().color;
    }

    private void BelongingMinoExplanation()
    {
        MenuManager.Instance.minoData.belongingsEffectGroup.transform.ChildClear();
        var id = GameManager.player.BelongingsMino[MenuManager.Instance.minoData.circleLayoutGroup.GetIndex()];
        var val = MinoData.Entity.GetMinoEffect(id);
        foreach (var data in val)
        {
            var obj = GameObject.Instantiate(MenuManager.Instance.minoData.minoEffectObj,
                MenuManager.Instance.minoData.belongingsEffectGroup.transform);
            obj.GetComponent<TextMeshProUGUI>().text = MinoEffectTextMaster.Entity.GetExplanationText(data);
        }
    }
    private void HaveMinoExplanation()
    {
        MenuManager.Instance.minoData.haveEffectGroup.transform.ChildClear();
        var id = GameManager.player.haveMinoList[currentIndex];
        var val = MinoData.Entity.GetMinoEffect(id);
        foreach (var data in val)
        {
            var obj = GameObject.Instantiate(MenuManager.Instance.minoData.minoEffectObj,
                MenuManager.Instance.minoData.haveEffectGroup.transform);
            obj.GetComponent<TextMeshProUGUI>().text = MinoEffectTextMaster.Entity.GetExplanationText(data);
        }
    }

    public void SelectMenu()
    {
        if (nowMode == NowMode.BelongingsSelect)
        {
            nowMode = NowMode.MinoSelect;
            
            currentIndex = 0;
            
            UpdateCursor();
            HaveMinoExplanation();
        }
        else
        {
            int minoNum = gridItems[currentIndex].GetComponent<MinoCreater>().GetMinoId();
            shakeInitPosition = gridItems[currentIndex].position;
            if (!GameManager.player.belongingsMino.Contains(minoNum))
            {
                ChangeBelongingMino(MenuManager.Instance.minoData.circleLayoutGroup.GetIndex(), minoNum);
                nowMode = NowMode.BelongingsSelect;
                ColorReset();
            }
            else
            {
                StartShake(gridItems[currentIndex].gameObject,0.5f, 5.5f, 30, 90, false);
            }
            MenuManager.Instance.minoData.haveEffectGroup.transform.ChildClear();
        }
        
        BelongingMinoExplanation();
    }
    void ChangeBelongingMino(int selectIndex, int minoNumber)
    {
        GameManager.player.BelongingsMino[selectIndex] = minoNumber;
        // 対象の親オブジェクト（Transform）を取得
        Transform parentTransform = 
            MenuManager.Instance.minoData.circleLayoutGroup.GetSelectObj();

        // 親オブジェクトの子要素をすべて削除
        foreach (Transform child in parentTransform)
        {
            Object.Destroy(child.gameObject); // 子オブジェクトを削除
        }
        MenuManager.Instance.minoData.circleLayoutGroup.GetSelectObj().GetComponent<MinoCreater>().UpdateId(minoNumber);
        MenuManager.Instance.minoData.circleLayoutGroup.GetSelectObj().GetComponent<MinoCreater>().CreateMino();
    }

    public void CloseMenu()
    {
        if (nowMode == NowMode.BelongingsSelect)
        {
            menu.ChangeMenu(SelectView.TabSelect);
            
        }
        else if (nowMode == NowMode.MinoSelect)
        {
            nowMode = NowMode.BelongingsSelect;
            currentIndex = 0;
            ColorReset();
        }
    }
    private void StartShake(GameObject obj, float duration, float strength, int vibrato, float randomness, bool fadeOut)
    {
        // 前回の処理が残っていれば停止して初期位置に戻す
        if (shakeTweener != null)
        {
            shakeTweener.Kill();
            obj.transform.position = shakeInitPosition;
        }
        // 揺れ開始
        shakeTweener = obj.transform.DOShakePosition(duration, strength, vibrato, randomness, fadeOut);
    }
    public void Update()
    {

    }
    public void Horizontal(bool right)
    {
        if (nowMode == NowMode.MinoSelect)
        {
            if (right)
            {
                currentIndex = Mathf.Min(gridItems.Length - 1, currentIndex + 1);
            }
            else
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
            }

            HaveMinoExplanation();
            UpdateCursor();
        }
        else if(nowMode == NowMode.BelongingsSelect)
        {
            MenuManager.Instance.minoData.circleLayoutGroup.AddAngle(right);
            BelongingMinoExplanation();
        }
    }

    public void Vertical(bool up)
    {
        if(nowMode == NowMode.MinoSelect)
        {
            if (up)
            {
                currentIndex = Mathf.Max(0, currentIndex - columns);    
            }
            else
            {
                currentIndex = Mathf.Min(gridItems.Length - 1, currentIndex + columns);
            }
            HaveMinoExplanation();
            UpdateCursor();
        }
    }

    public void Exit()
    {
        
    }

    void UpdateCursor()
    {
        foreach (var val in gridItems)
        {
            val.GetComponent<Image>().color = defaultColor;
        }
        if (currentIndex >= 0 && currentIndex < gridItems.Length)
        {
            // カーソルの位置を選択されたアイテムに一致させる
            gridItems[currentIndex].GetComponent<Image>().color = Color.red;
        }
    }

    void ColorReset()
    {
        foreach (var val in gridItems)
        {
            val.GetComponent<Image>().color = defaultColor;
        }   
    }
}

//=========================================================================
// 武器防具装備画面
//=========================================================================
public class ArmorView : EquipmentDataCreate, IMenu
{
    public SelectView State => SelectView.Armor;
    private UiController menu;
    private int selectTabNumber;
    
    private RectTransform[] gridItems;    // Gridの子要素
    private int cursorPos;
    private int haveCursorPos;
    private bool haveItemSelectViewFlag;
    
    public ArmorView(UiController menu,MenuManager menuManager)
    {
        this.menu = menu;
        tabData = menuManager.tabData;
        minoObj = menuManager.minoObj;
        armorObj = menuManager.armorObj;
        armorData = menuManager.armorData;
        minoData = menuManager.minoData;
    }
    public void Horizontal(bool right)
    {
        if (!haveItemSelectViewFlag)
        {
            cursorPos += right ? 1 : -1;
            cursorPos = Mathf.Clamp(cursorPos, 0, gridItems.Length - 1);
            armorData.CursorIcon.transform.position = gridItems[cursorPos].position;
            CreateHaveArmorData(cursorPos);
        }
        else
        {
            haveCursorPos = right 
                ? Mathf.Min(gridItems.Length - 1, haveCursorPos + 1) 
                : Mathf.Max(0, haveCursorPos - 1);
            armorData.CursorIcon.transform.position = gridItems[haveCursorPos].position;
            SetExplantionParameterSetting();
        }
    }

    public void Vertical(bool up)
    {
        if (haveItemSelectViewFlag)
        {
            haveCursorPos = up 
                ? Mathf.Max(0, haveCursorPos - 4) 
                : Mathf.Min(gridItems.Length - 1, haveCursorPos + 4);   
            armorData.CursorIcon.transform.position = gridItems[haveCursorPos].position; 
        }
        
    }
    public void Exit()
    {
        armorData.CursorIcon.SetActive(false);
    }

    public void Entry()
    {
        cursorPos = 0;
        armorData.CursorIcon.SetActive(true);
        gridItems = GetGroupChildData(armorData.BelongingsGroup.transform);
        armorData.CursorIcon.transform.position = gridItems[cursorPos].position;
        UpdateArmorData();
        CreateHaveArmorData(cursorPos);
    }

    public void Update()
    {
    }

    void SetExplantionParameterSetting()
    {
        Transform tran = armorData.ExplantionGroup.transform;
        Status s = EquipmentMaster.Entity.GetEquipmentData(
                            SelectHaveList(cursorPos)[haveCursorPos]
                        ).status;
        for (int i = 0; i < tran.childCount ; i++)
        {
            int data = i switch
            {
                0 => s.hp,
                1 => s.atk,
                2 => s.def,
            };
            tran.GetChild(i).GetComponent<ParameterSetting>()
                .SetText(data.ToString());
        }

        armorData.ExplantionEffectGroup.transform.ChildClear();;

        // 選択中の効果の表示
        var eName = SelectHaveList(cursorPos);
        if (eName != null)
        {
            EquipmentData eData = EquipmentMaster.Entity.GetEquipmentData(eName[haveCursorPos]);
            foreach (var status in eData.effect)
            {
                var data = GameObject.Instantiate(
                    armorData.effectObj,
                    armorData.ExplantionEffectGroup.transform);

                data.GetComponent<EffectCreater>().SetText(
                    EffectTextMaster.Entity.effectExplanation[status.effect],
                    status.upState.ToString()
                );
            }
        }
    }

    public void SelectMenu()
    {
        if (!haveItemSelectViewFlag)
        {
            // 持ち物選択に進むとき
            var item  = GetGroupChildData(armorData.HaveItemGroup.transform);
            if (item == null) return;
            gridItems = item;
            haveItemSelectViewFlag = true;
            armorData.CursorIcon.transform.position = gridItems[haveCursorPos].position;
            armorData.ExplantionGroup.gameObject.SetActive(true);
            armorData.ExplantionEffectGroup.gameObject.SetActive(true);
            SetExplantionParameterSetting();
        }
        else
        {
            // 装備選択に進むとき
            (EqupmentPart,List<string>) esData = cursorPos switch
            {
                0 => (EqupmentPart.Weapon,GameManager.player.haveWeaponList),
                1 => (EqupmentPart.Shield,GameManager.player.haveShieldList),
                2 => (EqupmentPart.Helmet,GameManager.player.haveHelmetList),
                3 => (EqupmentPart.Armor,GameManager.player.haveArmorList),
            };

            GameManager.player.SetEquipment(esData.Item1,esData.Item2[haveCursorPos]);
            UpdateBelongingsWeaponSprite();
            OpenEquipmentEffectArmor();
            UpdateArmorData();
            armorData.ExplantionGroup.gameObject.SetActive(false);
            armorData.ExplantionEffectGroup.gameObject.SetActive(false);
            haveItemSelectViewFlag = false;
            gridItems = GetGroupChildData(armorData.BelongingsGroup.transform);
            armorData.CursorIcon.transform.position = gridItems[cursorPos].position;

        }
    }

    public void CloseMenu()
    {
        if (haveItemSelectViewFlag)
        {
            haveItemSelectViewFlag = false;
            gridItems = GetGroupChildData(armorData.BelongingsGroup.transform);
            armorData.CursorIcon.transform.position = gridItems[cursorPos].position;
            armorData.ExplantionGroup.gameObject.SetActive(false);
            armorData.ExplantionEffectGroup.gameObject.SetActive(false);
        }
        else
        {
            menu.ChangeMenu(SelectView.TabSelect);    
        }
    }

 
}

public class EquipmentDataCreate
{
    protected TabData tabData;
    protected GameObject minoObj;
    protected GameObject armorObj;
    protected ArmorData armorData;
    protected MinoViewData minoData;

    protected Func<int, string> SelectEquipment = (i) =>
    {
        return i switch
        {
            0 => GameManager.player.belongingsEquipment?.weaponId,
            1 => GameManager.player.belongingsEquipment?.shildId,
            2 => GameManager.player.belongingsEquipment?.helmetId,
            3 => GameManager.player.belongingsEquipment?.armorId,
        };
    };

    protected Func<int, List<string>> SelectHaveList => (i) =>
    {
        return i switch
        {
            0 => GameManager.player.haveWeaponList,
            1 => GameManager.player.haveShieldList,
            2 => GameManager.player.haveHelmetList,
            3 => GameManager.player.haveArmorList,
        };
    };

    // 持ち物リストの更新
    protected void CreateHaveArmorData(int cursorPos)
    {
        //MenuManager.Instance.GroupChildReset(MenuManager.Instance.armorData.HaveItemGroup.transform);
        MenuManager.Instance.armorData.HaveItemGroup.transform.ChildClear();
        List<string> esData = SelectHaveList(cursorPos);
        foreach (var list in esData)
        {
            var obj = GameObject.Instantiate(
                MenuManager.Instance.armorData.HaveItemObj,
                MenuManager.Instance.armorData.HaveItemGroup.transform);
            obj.GetComponent<Image>().sprite =
                EquipmentDatabase.Entity.GetEquipmentSpriteData(list).sprite;
        }
    }
    
    // 装備中の画像更新
    protected void UpdateBelongingsWeaponSprite()
    {
        

        for (int i = 0; i < 4; i++)
        {
            var eName = SelectEquipment(i);
            if (eName != null)
            {
                List<EquipmentSpriteData> esData = i switch
                {
                    0 => EquipmentDatabase.Entity.weaponData,
                    1 => EquipmentDatabase.Entity.shieldData,
                    2 => EquipmentDatabase.Entity.helmetData,
                    3 => EquipmentDatabase.Entity.armorData,
                };
                var data = esData.First(_ => _.weaponId == eName);
                if (data != null)
                {
                    MenuManager.Instance.armorData.BelongingsSprite[i].SetImage(data.sprite);
                    MenuManager.Instance.armorData.BelongingsSprite[i].SetActive(true);
                    
                }
            }
            else
            {
                MenuManager.Instance.armorData.BelongingsSprite[i].SetNoSettingMode();
            }
        }
    }
    
    // 効果の表示更新
    protected void OpenEquipmentEffectArmor()
    {
        armorData.EffectGroup.transform.ChildClear();;
        
        // 装備中の効果の表示
        for(int i = 0;i < 4;i++)
        {
            var eName = SelectEquipment(i);
            if (eName != null)
            {
                EquipmentData eData = EquipmentMaster.Entity.GetEquipmentData(eName);
                foreach (var status in eData.effect)
                {
                    var data = GameObject.Instantiate(
                        armorData.effectObj,
                        armorData.EffectGroup.transform);

                    data.GetComponent<EffectCreater>().SetText(
                        EffectTextMaster.Entity.effectExplanation[status.effect],
                        status.upState.ToString()
                    );
                }
            }
        }
    }
    protected void UpdateArmorData()
    {
        armorData.hpText.text = GameManager.player.totalStatus.maxHp.ToString();
        armorData.atkText.text = GameManager.player.totalStatus.atk.ToString();
        armorData.defText.text = GameManager.player.totalStatus.def.ToString();
    }
    protected RectTransform[] GetGroupChildData(Transform groupTransform)
    {
        int childCount = groupTransform.childCount;
        if (childCount == 0) return null;
        var items = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            items[i] = groupTransform.GetChild(i)
                .GetComponent<RectTransform>();
        }

        return items;
    }
}
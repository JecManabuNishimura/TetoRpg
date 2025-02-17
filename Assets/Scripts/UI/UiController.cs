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
            { SelectView.HaveMino ,new HaveMinoView(menu,MenuManager.Instance)},
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

        GameManager.Instance.menuFlag = false;
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
        foreach (var mino in GameManager.Instance.player.BelongingsMino)
        {
            var data = GameObject.Instantiate(minoData.itemObj, minoData.circleLayoutGroup.transform, true);
            data.GetComponent<MinoCreater>().UpdateId(mino);
            data.GetComponent<MinoCreater>().CreateMino();
        }
    }
    void OpenEquipmentMino()
    {
        minoData.gridLayoutGroup.transform.ChildClear();;
        foreach (var mino in GameManager.Instance.player.HaveMinoList)
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
    public HaveMinoView(UiController menu,MenuManager menuManager)
    {
        this.menu = menu;
        minoData = menuManager.minoData;
    }

    private RectTransform[] gridItems;    // Gridの子要素
    private int currentIndex = 0;
    private Color defaultColor;
    private int columns = 3;
    private Tweener shakeTweener;
    private Vector3 shakeInitPosition;
    private int ypos;

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

        gridCursorMove = new GridCursorMove(childCount, minoData.scrollRect, minoData.gridLayoutGroup);
        BelongingMinoExplanation();
        
        //defaultColor = new Color(0.69f, 0.65f, 0.5f);
    }

    private void BelongingMinoExplanation()
    {
        MenuManager.Instance.minoData.belongingsEffectGroup.transform.ChildClear();
        foreach (var mino in GameManager.Instance.player.BelongingsMino)
        {
            var val = MinoData.Entity.GetMinoEffect(mino);
            foreach (var data in val)
            {
                if (data.effect != "None")
                {
                    var obj = GameObject.Instantiate(MenuManager.Instance.minoData.minoEffectObj,
                        MenuManager.Instance.minoData.belongingsEffectGroup.transform);
                    obj.GetComponent<TextMeshProUGUI>().text =
                        MinoEffectTextMaster.Entity.GetExplanationText(data.effect).Text;
                }
            }
        }
    }
    private void HaveMinoExplanation()
    {
        MenuManager.Instance.minoData.haveEffectGroup.transform.ChildClear();
        var id = GameManager.Instance.player.haveMinoList[currentIndex];
        var val = MinoData.Entity.GetMinoEffect(id);
        foreach (var data in val)
        {
            var obj = GameObject.Instantiate(MenuManager.Instance.minoData.minoEffectObj,
                MenuManager.Instance.minoData.haveEffectGroup.transform);
            obj.GetComponent<TextMeshProUGUI>().text = MinoEffectTextMaster.Entity.GetExplanationText(data.effect).Text;
        }
    }

    public void SelectMenu()
    {
        if (nowMode == NowMode.BelongingsSelect)
        {
            nowMode = NowMode.MinoSelect;
            
            //currentIndex = 0;
            minoData.CursorObj.SetActive(true);
            UpdateCursor();
            HaveMinoExplanation();
        }
        else
        {
            
            EquipmentUniqueData minoNum = gridItems[currentIndex].GetComponent<MinoCreater>().GetMinoId();
            shakeInitPosition = gridItems[currentIndex].position;
            if (!GameManager.Instance.player.belongingsMino.Contains(minoNum))
            {
                minoData.CursorObj.SetActive(false);
                ChangeBelongingMino(MenuManager.Instance.minoData.circleLayoutGroup.GetIndex(), minoNum);
                nowMode = NowMode.BelongingsSelect;
                //ColorReset();
                GameManager.Instance.player.SetBelongingsMinoEffect();
            }
            else
            {
                StartShake(gridItems[currentIndex].gameObject,0.5f, 5.5f, 30, 90, false);
            }
            MenuManager.Instance.minoData.haveEffectGroup.transform.ChildClear();
            //NextUpGauge.Instance.ReCount(GameManager.Instance.NowNextCount);
        }
        
        BelongingMinoExplanation();
    }
    void ChangeBelongingMino(int selectIndex, EquipmentUniqueData data)
    {
        // ミノ装備
        GameManager.Instance.player.BelongingsMino[selectIndex] = data;
        // 対象の親オブジェクト（Transform）を取得
        Transform parentTransform = 
            MenuManager.Instance.minoData.circleLayoutGroup.GetSelectObj();

        // 親オブジェクトの子要素をすべて削除
        foreach (Transform child in parentTransform)
        {
            Object.Destroy(child.gameObject); // 子オブジェクトを削除
        }
        MenuManager.Instance.minoData.circleLayoutGroup.GetSelectObj().GetComponent<MinoCreater>().UpdateId(data);
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
            currentIndex = gridCursorMove.MoveHorizontal(right);

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
            currentIndex = gridCursorMove.MoveVertical(up);
            HaveMinoExplanation();
            UpdateCursor();
        }
    }
   

    public void Exit()
    {
        
    }

    void UpdateCursor()
    {
        if (currentIndex >= 0 && currentIndex < gridItems.Length)
        {
            // カーソルの位置を選択されたアイテムに一致させる
            minoData.CursorObj.transform.position = gridItems[currentIndex].position;
            minoData.CursorObj.GetComponent<CursorController>().PlayAnim();
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
            UpdateCursor(cursorPos);
            CreateHaveArmorData(cursorPos);
        }
        else
        {
            haveCursorPos = gridCursorMove.MoveHorizontal(right);
            UpdateCursor(haveCursorPos);
            SetExplantionParameterSetting();
        }
    }

    public void Vertical(bool up)
    {
        if (haveItemSelectViewFlag)
        {
            haveCursorPos = gridCursorMove.MoveVertical(up);
            UpdateCursor(haveCursorPos);
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
        UpdateCursor(cursorPos);
        
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
                            SelectHaveList(cursorPos)[haveCursorPos].WeaponId
                        ).status;
        for (int i = 0; i < tran.childCount ; i++)
        {
            if (i == 0)
            {
                tran.GetChild(i).GetComponent<ParameterSetting>()
                    .SetText(EquipmentMaster.Entity.GetEquipmentData(
                        SelectHaveList(cursorPos)[haveCursorPos].WeaponId
                    ).name);
            }
            else
            {
                int data = i switch
                {
                    1 => s.hp,
                    2 => s.atk,
                    3 => s.def,
                };
                tran.GetChild(i).GetComponent<ParameterSetting>()
                    .SetText(data.ToString());
            }
            
            
        }

        armorData.ExplantionEffectGroup.transform.ChildClear();

        // 選択中の効果の表示
        var eName = SelectHaveList(cursorPos);
        if (eName != null)
        {
            var eData = eName.FirstOrDefault(d => d.WeaponId == eName[haveCursorPos].WeaponId && d.groupID == eName[haveCursorPos].groupID);
            WeaponEffectMaster.Entity.GetWeaponEffect(eData.WeaponId, eData.groupID).effects
                .Where(e => e.effect != EffectStatus.None)
                .ToList()
                .ForEach(status =>
                {
                    var data = GameObject.Instantiate(
                        armorData.effectObj,
                        armorData.ExplantionEffectGroup.transform);

                    data.GetComponent<EffectCreater>().SetText(
                        EffectTextMaster.Entity.effectExplanation[status.effect],
                        status.value.ToString());
                });
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
            haveCursorPos = 0;
            gridCursorMove = new GridCursorMove(
                MenuManager.Instance.armorData.HaveItemGroup.transform.childCount,
                armorData.scrollRect,
                armorData.HaveItemGroup);
            
            UpdateCursor(haveCursorPos);
            armorData.ExplantionGroup.gameObject.SetActive(true);
            armorData.ExplantionEffectGroup.gameObject.SetActive(true);

            SetExplantionParameterSetting();
        }
        else
        {
            // 装備選択に進むとき
            (EqupmentPart,List<EquipmentUniqueData>) esData = cursorPos switch
            {
                0 => (EqupmentPart.Weapon,GameManager.Instance.player.haveWeaponList),
                1 => (EqupmentPart.Shield,GameManager.Instance.player.haveShieldList),
                2 => (EqupmentPart.Helmet,GameManager.Instance.player.haveHelmetList),
                3 => (EqupmentPart.Armor,GameManager.Instance.player.haveArmorList),
            };

            GameManager.Instance.player.SetEquipment(esData.Item1,esData.Item2[haveCursorPos]);
            UpdateBelongingsWeaponSprite();
            OpenEquipmentEffectArmor();
            UpdateArmorData();
            armorData.ExplantionGroup.gameObject.SetActive(false);
            armorData.ExplantionEffectGroup.gameObject.SetActive(false);
            haveItemSelectViewFlag = false;
            gridItems = GetGroupChildData(armorData.BelongingsGroup.transform);
            UpdateCursor(cursorPos);

        }
    }

    public void CloseMenu()
    {
        if (haveItemSelectViewFlag)
        {
            haveItemSelectViewFlag = false;
            gridItems = GetGroupChildData(armorData.BelongingsGroup.transform);
            UpdateCursor(cursorPos);
            armorData.ExplantionGroup.gameObject.SetActive(false);
            armorData.ExplantionEffectGroup.gameObject.SetActive(false);
        }
        else
        {
            menu.ChangeMenu(SelectView.TabSelect);    
        }
    }
    void UpdateCursor(int index)
    {
        // カーソルの位置を選択されたアイテムdに一致させる
        armorData.CursorIcon.transform.position = gridItems[index].position;
        armorData.CursorIcon.GetComponent<CursorController>().PlayAnim();
    }
 
}

public class EquipmentDataCreate
{
    protected GridCursorMove gridCursorMove;
    protected TabData tabData;
    protected GameObject minoObj;
    protected GameObject armorObj;
    protected ArmorData armorData;
    protected MinoViewData minoData;

    protected Func<int, EquipmentUniqueData> SelectEquipment = (i) =>
    {
        return i switch
        {
            0 => GameManager.Instance.player.belongingsEquipment?.weapon,
            1 => GameManager.Instance.player.belongingsEquipment?.shield,
            2 => GameManager.Instance.player.belongingsEquipment?.helmet,
            3 => GameManager.Instance.player.belongingsEquipment?.armor,
        };
    };

    protected Func<int, List<EquipmentUniqueData>> SelectHaveList => (i) =>
    {
        return i switch
        {
            0 => GameManager.Instance.player.haveWeaponList,
            1 => GameManager.Instance.player.haveShieldList,
            2 => GameManager.Instance.player.haveHelmetList,
            3 => GameManager.Instance.player.haveArmorList,
        };
    };

    // 持ち物リストの更新
    protected void CreateHaveArmorData(int cursorPos)
    {
        //MenuManager.Instance.GroupChildReset(MenuManager.Instance.armorData.HaveItemGroup.transform);
        MenuManager.Instance.armorData.HaveItemGroup.transform.ChildClear();
        // カーソルの位置によって取得内容を変更
        var esData = SelectHaveList(cursorPos);
        foreach (var list in esData)
        {
            var obj = GameObject.Instantiate(
                MenuManager.Instance.armorData.HaveItemObj,
                MenuManager.Instance.armorData.HaveItemGroup.transform);
            obj.GetComponent<Image>().sprite =
                EquipmentDatabase.Entity.GetEquipmentSpriteData(list.WeaponId).sprite;
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
                var data = esData.FirstOrDefault(_ => _.weaponId == eName.WeaponId);
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
                WeaponEffectMaster.Entity.GetWeaponEffect(eName.WeaponId, eName.groupID).effects
                    .Where(e => e.effect != EffectStatus.None)
                    .ToList()
                    .ForEach(status =>
                    {
                        var data = GameObject.Instantiate(
                            armorData.effectObj,
                            armorData.EffectGroup.transform);

                        data.GetComponent<EffectCreater>().SetText(
                            EffectTextMaster.Entity.effectExplanation[status.effect],
                            status.value.ToString());
                    });
            }
        }
    }
    protected void UpdateArmorData()
    {
        armorData.hpText.text = GameManager.Instance.player.totalStatus.maxHp.ToString();
        armorData.atkText.text = GameManager.Instance.player.totalStatus.atk.ToString();
        armorData.defText.text = GameManager.Instance.player.totalStatus.def.ToString();
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
public class GridCursorMove
{
    private int ItemCount;
    private ScrollRect scrollRect;
    private GridLayoutGroup layoutGroup;
    private int currentIndex;
    private int ypos;
    private int rowCount;
    private int columns;
    public GridCursorMove(int itemCount, ScrollRect scrollRect, GridLayoutGroup layoutGroup)
    {
        ItemCount = itemCount;
        this.scrollRect = scrollRect;
        this.layoutGroup = layoutGroup;
        scrollRect.content.anchoredPosition = new Vector2(0, 0);
        rowCount = (int)(scrollRect.viewport.rect.height / layoutGroup.cellSize.y);
        columns = (int)(scrollRect.viewport.rect.width / layoutGroup.cellSize.x);
    }

    public int MoveHorizontal(bool right)
    {
        if (right)
        {
            if (currentIndex % columns < columns - 1 && currentIndex < ItemCount - 1)
            {
                currentIndex ++;
            }
            //currentIndex = Mathf.Min(gridItems.Length - 1, currentIndex + 1);
        }
        else
        {
            if (currentIndex% columns > 0)
            {
                currentIndex--;
            }
            //currentIndex = Mathf.Max(0, currentIndex - 1);
        }

        return currentIndex;
    }
    public int MoveVertical(bool up)
    {
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        
        float maxScrollPosition = contentHeight - viewportHeight;
        float itemHeight = layoutGroup.cellSize.y + layoutGroup.spacing.y;


        if (up)
        {
            if (currentIndex - columns >= 0)
            {
                currentIndex -= columns;
                ypos = Mathf.Clamp(ypos - 1, -1, Mathf.Min(rowCount, currentIndex));
            }
        }
        else
        {
            currentIndex = Mathf.Min(currentIndex + columns, ItemCount - 1);
            ypos = Mathf.Clamp(ypos + 1, -1, Mathf.Min(rowCount, currentIndex));
        }

        if (ypos == rowCount && !up)
        {
            var pos =scrollRect.content.anchoredPosition;
            pos.y = Mathf.Clamp(scrollRect.content.anchoredPosition.y + itemHeight, 0, (int)maxScrollPosition);

            scrollRect.content.anchoredPosition = pos;
            ypos--;
        }
        else if (ypos == -1 && up )
        {
            var pos =scrollRect.content.anchoredPosition;
            pos.y = Mathf.Clamp(scrollRect.content.anchoredPosition.y - itemHeight, 0, maxScrollPosition);

            scrollRect.content.anchoredPosition = pos;
            ypos++;
        }

        return currentIndex;
    }
}
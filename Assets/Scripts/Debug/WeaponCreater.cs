using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class WeaponCreater : EditorWindow
{
    public class WeaponData
    {
        public EffectStatus effect;
        public int ToValue;
        public int FromValue;
    }

    private string weaponId;
    private int createCount;
    private int effectCount;

    private ReorderableList effectList;
    private List<WeaponData> effects = new();
    private List<CreateWeaponData> weapons = new();
    private SerializedObject serializedObject;
    private Vector2 scrollPosition;

    [MenuItem("Window/Custom/WeaponFactoryWindow")]
    public static void ShowWindow()
    {
        GetWindow<WeaponCreater>("WeaponFactoryWindow");
    }
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        effectList = CreateReorderableList("Item Drop Data", effects, true);
    }
    
    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope("Box"))
        {
            if (GUILayout.Button("Create", GUILayout.Width(150), GUILayout.Height(20)))
            {
                weapons.Clear();
                if (weaponId == "")
                {
                    Debug.Log("WeaponIdを指定してください");
                    return;
                }

                if (createCount <= 0)
                {
                    Debug.Log("createCountを1以上にしてください");
                    return;
                }

                if (effectCount <= 0)
                {
                    Debug.Log("effectCountを1以上にしてください");
                    return;
                }

                for (int i = 0; i < createCount; i++)
                {
                    CreateWeaponData weaponData = new CreateWeaponData();
                    weaponData.weaponId = weaponId;
                    weaponData.groupData = new(i, new List<WeaponEffect>());
                    List<int> numberList = new List<int>();
                    for (int c = 0; c < effects.Count; c++)
                    {
                        numberList.Add(c);
                    }

                    int count = UnityEngine.Random.Range(1, effectCount + 1);
                    for (int j = 0; j < count; j++)
                    {
                        int randomNum = UnityEngine.Random.Range(0, numberList.Count);
                        var rand = effects[numberList[randomNum]];
                        numberList.RemoveAt(randomNum);
                        weaponData.groupData.effects.Add(
                            new WeaponEffect(
                                rand.effect,
                                UnityEngine.Random.Range(rand.ToValue, rand.FromValue + 1)));
                    }

                    weapons.Add(weaponData);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Writing", GUILayout.Width(150), GUILayout.Height(20)))
            {
                string fileName = "Master/WeaponEffectMaster";
                
                WeaponEffectMaster database = Resources.Load<WeaponEffectMaster>(fileName);

                foreach (var w in weapons)
                {
                    CreateWeaponData data = new()
                    {
                        weaponId = w.weaponId,
                        groupData = w.groupData,
                    };
                    database.WeaponDatas.Add(data);
                }
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }
        }

        using (new EditorGUILayout.VerticalScope(GUILayout.Width(400)))
        {
            using (new EditorGUILayout.HorizontalScope("Box", GUILayout.Width(200)))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("WeaponID", GUILayout.Width(100), GUILayout.Height(20), GUILayout.ExpandWidth(false));
                    EditorGUILayout.LabelField("CreateCount", GUILayout.Width(100), GUILayout.Height(20), GUILayout.ExpandWidth(false));
                    EditorGUILayout.LabelField("EffectCount", GUILayout.Width(100), GUILayout.Height(20), GUILayout.ExpandWidth(false));
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    weaponId = EditorGUILayout.TextArea(weaponId, GUILayout.Width(50), GUILayout.Height(20), GUILayout.ExpandWidth(false));
                    createCount = EditorGUILayout.IntField(createCount, GUILayout.Width(50), GUILayout.Height(20), GUILayout.ExpandWidth(false));
                    effectCount = EditorGUILayout.IntField(effectCount, GUILayout.Width(50), GUILayout.Height(20), GUILayout.ExpandWidth(false));
                }
            }
            
            effectList.DoLayoutList();
        }
        int oneWidth = 200;
        int colCount = 0;
        if (GUILayout.Button("Delete", GUILayout.Width(150), GUILayout.Height(20)))
        {
            for (int i = weapons.Count - 1; i >= 0; i--)
            {
                if (weapons[i].deleteFlag)
                {
                    weapons.RemoveAt(i);
                }
            }
        }
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
        {
            GUILayout.BeginHorizontal();
            foreach (var w in weapons)
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    using (new EditorGUILayout.HorizontalScope("Box"))
                    {
                        EditorGUILayout.LabelField(w.weaponId, GUILayout.Width(50), GUILayout.Height(20));
                        w.deleteFlag = EditorGUILayout.Toggle(w.deleteFlag);
                    }
                        
                    EditorGUILayout.IntField(w.groupData.id, GUILayout.Width(50), GUILayout.Height(20));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        foreach (var e in w.groupData.effects)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.EnumPopup(e.effect, GUILayout.Width(100), GUILayout.Height(20));
                                EditorGUILayout.IntField(e.value, GUILayout.Width(50), GUILayout.Height(20));
                            }
                        }
                    }
                }
                colCount++;
                if (oneWidth * colCount + 50 > position.width)
                {
                    colCount = 0;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            
            GUILayout.EndHorizontal();
            // 新しいスクロール位置を保存
            scrollPosition = scrollView.scrollPosition;
        }
        


        if (GUI.changed)
        {
            EditorUtility.SetDirty(this);
        }
    }


    private ReorderableList CreateReorderableList(string title, List<WeaponData> dataList, bool useDropdown)
    {
        var list = new ReorderableList(dataList, typeof(WeaponData), true, true, true, true);

        // ヘッダー描画
        list.drawHeaderCallback = (Rect rect) =>
        {
            float width = rect.width / 3;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, rect.height), "Effect");
            EditorGUI.LabelField(new Rect(rect.x + width, rect.y, 100, rect.height), "ToValue");
            EditorGUI.LabelField(new Rect(rect.x + width * 2, rect.y, 100, rect.height), "FromValue");
        };

        // 各要素の描画
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var item = dataList[index];
            float width = rect.width / 3 - 5;
            item.effect = (EffectStatus)EditorGUI.EnumPopup(new Rect(20, rect.y, 100, EditorGUIUtility.singleLineHeight), item.effect);
            item.ToValue = EditorGUI.IntField(new Rect(width + 20, rect.y, 100, EditorGUIUtility.singleLineHeight), item.ToValue);
            item.FromValue = EditorGUI.IntField(new Rect((width*2) + 20, rect.y, 100, EditorGUIUtility.singleLineHeight), item.FromValue);
        };

        // 要素の削除
        list.onRemoveCallback = (list) =>
        {
            if (list.index >= 0 && list.index < dataList.Count)
            {
                dataList.RemoveAt(list.index);
            }
        };

        // 要素の追加
        list.onAddCallback = (list) =>
        {
            dataList.Add(new WeaponData() { effect = EffectStatus.None, ToValue = 0, FromValue = 0 });
        };

        return list;
    }
}

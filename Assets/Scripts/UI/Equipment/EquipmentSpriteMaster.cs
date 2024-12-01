using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class EquipmentSpriteMaster : EditorWindow
{
    public static EquipmentSpriteMaster Instance { get; private set; }
    // 表示する名前とSpriteのリスト
    public List<EquipmentSpriteData> weaponData = new List<EquipmentSpriteData>();
    public List<EquipmentSpriteData> shieldData = new List<EquipmentSpriteData>();
    public List<EquipmentSpriteData> helmetData = new List<EquipmentSpriteData>();
    public List<EquipmentSpriteData> armorData = new List<EquipmentSpriteData>();
    private int columns = 5; // グリッドの列数
    private float cellWidth = 64f; // セルの幅
    private float previewSize = 64f; // プレビュー画像のサイズ
    private Vector2 scrollPosition; // スクロール位置
    private int selectedTab = 0; // 現在選択されているタブ
    private float padding = 15f;     // セル間のスペース
    private string dataFileName = "equipment_data.json"; // 保存するファイル名

    
    
    // ウィンドウの起動
    [MenuItem("Window/Custom Name and Sprite Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<EquipmentSpriteMaster>("Name and Sprite");
        window.LoadData();  // データを読み込む
    }

    // ウィンドウのUIを描画
    private void OnGUI()
    {
        // タブの作成
        string[] tabNames = { "Weapon", "Shield","Helmet","Armor" };
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        // タブによって異なるUIを表示
        switch (selectedTab)
        {
            case 0:
                DrawWeaponTab();
                break;
            case 1:
                DrawShieldTab();
                break;
            case 2:
                DrawHelmetTab();
                break;
            case 3:
                DrawArmorTab();
                break;
        }

    }

    private void DrawItemTab(string tabName, List<EquipmentSpriteData> itemData, string prefix)
    {
        GUILayout.Label(tabName, EditorStyles.boldLabel);

        // ウィンドウの幅に基づいて横の列数を調整
        float windowWidth = position.width;
        int columns = Mathf.FloorToInt(windowWidth / (cellWidth + padding));

        // スクロールビューの開始
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(windowWidth), GUILayout.Height(position.height - 50));

        // グリッド表示: 行数の計算
        int rowCount = Mathf.CeilToInt(itemData.Count / (float)columns);

        // 横並びにするために BeginHorizontal を追加
        EditorGUILayout.BeginHorizontal();

        // 新しい名前とSpriteのセットを追加するボタン
        if (GUILayout.Button("Add New", GUILayout.Width(150), GUILayout.Height(30)))
        {
            string newId = $"{prefix}{itemData.Count + 1:00}";
            itemData.Add(new EquipmentSpriteData() { weaponId = newId });
        }

        // アイテムを削除するボタン
        if (GUILayout.Button("Delete Selected", GUILayout.Width(150), GUILayout.Height(30)))
        {
            // アイテムを削除
            for (int i = itemData.Count - 1; i >= 0; i--)
            {
                if (itemData[i].isSelected)  // `isSelected` が true なら削除
                {
                    itemData.RemoveAt(i);  // リストから削除
                }
            }
        }
        if (GUILayout.Button("Save", GUILayout.Width(150), GUILayout.Height(30)))
        {
            SaveToScriptableObject();
        }

        EditorGUILayout.EndHorizontal(); // 横並びを終了

        // グリッド内のアイテム表示
        for (int row = 0; row < rowCount; row++)
        {
            EditorGUILayout.BeginHorizontal();  // 行ごとに横並びにする

            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col;

                // インデックスがリストのサイズを超えていない場合のみ処理
                if (index < itemData.Count)
                {
                    // 縦に並べるため、個々のセル内で1つずつ表示
                    EditorGUILayout.BeginVertical(GUILayout.Width(cellWidth));  // セル内で縦に並べる

                    // WeaponIDのテキストフィールド
                    itemData[index].weaponId = EditorGUILayout.TextField(itemData[index].weaponId);

                    // SpriteのObjectFieldを表示
                    itemData[index].sprite = (Sprite)EditorGUILayout.ObjectField(itemData[index].sprite, typeof(Sprite), false);
                    // アイテム選択用のチェックボックスを追加
                    // チェックボックス
                    itemData[index].isSelected = GUILayout.Toggle(itemData[index].isSelected, "", GUILayout.Width(40));  // チェックボックスの横幅を指定
                    

                    // プレビュー画像表示用のスペースを確保（ObjectField とプレビュー画像が重ならないように）
                    GUILayout.Space(5);  // 少しスペースを開ける

                    // プレビュー画像の表示
                    if (itemData[index].sprite != null)
                    {
                        // プレビュー画像を表示
                        Rect rect = GUILayoutUtility.GetLastRect();  // 最後のレイアウト位置を取得
                        rect.height = previewSize;  // プレビュー画像の高さ
                        rect.width = previewSize;   // プレビュー画像の幅
                        EditorGUI.DrawPreviewTexture(rect, itemData[index].sprite.texture);  // 画像のプレビューを表示
                    }

                    GUILayout.Space(5);  // セル間の余白を調整

                    EditorGUILayout.EndVertical();  // 縦のレイアウト終了
                }
                else
                {
                    // 空のセルを追加（残りのスペース）
                    GUILayout.Space(cellWidth);
                }
            }

            EditorGUILayout.EndHorizontal();  // 行終了
        }

        EditorGUILayout.EndScrollView();  // スクロールビュー終了
    }

    private void DrawWeaponTab()
    {
        DrawItemTab("Weapon", weaponData,"We");
    }

    private void DrawShieldTab()
    {
        DrawItemTab("Shield", shieldData,"Sh");
    }
    private void DrawHelmetTab()
    {
        DrawItemTab("Helmet", helmetData,"He");
    }
    private void DrawArmorTab()
    {
        DrawItemTab("Armor", armorData,"Ar");
    }

    // データを読み込むメソッド
    private void LoadData()
    {
        string assetPath = "Master/EquipmentDatabase";
        EquipmentDatabase database = Resources.Load<EquipmentDatabase>(assetPath);
        if (database != null)
        {
            // ScriptableObjectのデータをEditorWindowにコピー
            weaponData = new List<EquipmentSpriteData>(database.weaponData);
            shieldData = new List<EquipmentSpriteData>(database.shieldData);
            helmetData = new List<EquipmentSpriteData>(database.helmetData);
            armorData = new List<EquipmentSpriteData>(database.armorData);
        }
        else
        {
            Debug.LogWarning("EquipmentDatabase not found at " + assetPath);
        }
        
    }
   
    private void SaveToScriptableObject()
    {
        string assetPath = "Master/EquipmentDatabase";
        EquipmentDatabase database = Resources.Load<EquipmentDatabase>(assetPath);
    
        // アセットが存在しない場合は新規作成
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<EquipmentDatabase>();
            AssetDatabase.CreateAsset(database, assetPath);
        }

        // データをコピー
        database.weaponData = new List<EquipmentSpriteData>(weaponData);
        database.shieldData = new List<EquipmentSpriteData>(shieldData);
        database.helmetData = new List<EquipmentSpriteData>(helmetData);
        database.armorData = new List<EquipmentSpriteData>(armorData);

        // アセットを保存
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        
    }
    
    // ウィンドウが閉じられるときにデータを保存
    private void OnDestroy()
    {
        SaveToScriptableObject();  // データを保存
    }
    
    [Serializable]
    public class EquipmentData
    {
        public List<EquipmentSpriteData> weaponData;
        public List<EquipmentSpriteData> shieldData;
        public List<EquipmentSpriteData> helmetData;
        public List<EquipmentSpriteData> armorData;
    }
}
#endif
[Serializable]
public class EquipmentSpriteData
{
    public string weaponId;
    public Sprite sprite;
    public bool isSelected;  // 選択状態を保持するフィールド
}


using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class MinoFactoryWindow : EditorWindow
{
    private class MinoSetting
    {
        public int[,] minoDataList;
        public bool select;

        public MinoSetting()
        {
            minoDataList = new int[4, 4];
            select = false;
        }
        
    }
   // 画像を格納する変数
   private Texture2D buttonFalseImage;
   private Texture2D buttonTrueImage;
   private Texture2D buttonRotImage;
   private float cellSize = 25f;  // セルのサイズ
   private float padding = 1f;    // セル間の余白
   private float gridPadding = 10f; // 配列間の余白
   private List<MinoSetting> minoSettingList = new();
   private float maxGroupHeight = 0f; // 全体の高さを管理
   private MinoEffectStatusMaster minoEffectStatusMaster;
   private List<List<int>> selectedGroupOptions = new List<List<int>>(); // 各グループに複数選択肢を保持
   private float buttonSize = 30f;
   private Rect buttonRect;
   float totalWidth = Screen.width - 20f;  // 画面幅
   
   float startX = 10f;
   float startY = 50f;
   [MenuItem("Window/Custom/MinoFactoryWindow")]
   public static void ShowWindow()
   {
      GetWindow<MinoFactoryWindow>("MinoFactoryWindow");
   }

   private void OnEnable()
   {
       minoEffectStatusMaster = MinoEffectStatusMaster.Entity;
      // 画像をエディタのアセットフォルダからロード
      buttonFalseImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Chess Studio/Puzzle Blocks Icon Pack/png/bubble.png");
      buttonTrueImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Chess Studio/Puzzle Blocks Icon Pack/png/blockBlueDimond.png");
      buttonRotImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Chess Studio/Puzzle Blocks Icon Pack/png/blockYellowDimond.png");
   }

   private void OnGUI()
   {
        // 配列の表示
        float startX = padding; // X座標の開始位置
        float startY = padding; // Y座標の開始位置
        
        float currentX = startX; // X方向の現在の位置
        float currentY = startY + 20; // Y方向の現在の位置
        // 保存ボタンを一番上に配置
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+", GUILayout.Width(60), GUILayout.Height(20))) // サイズを小さくした保存ボタン
        {
            minoSettingList.Add(new MinoSetting());
        }
        if (GUILayout.Button("-", GUILayout.Width(60), GUILayout.Height(20))) // サイズを小さくした保存ボタン
        {
            for (int i = minoSettingList.Count - 1; i >= 0; i--)
            {
                if (minoSettingList[i].select)
                {
                    minoSettingList.RemoveAt(i);
                }
            }
        }
        if (GUILayout.Button("Save", GUILayout.Width(60), GUILayout.Height(20))) // サイズを小さくした保存ボタン
        {
            SaveData();
        }
        if (GUILayout.Button("Load", GUILayout.Width(60), GUILayout.Height(20))) // サイズを小さくした保存ボタン
        {
            LoadData();
        }
        GUILayout.EndHorizontal();
        
        for (int i = 0; i < minoSettingList.Count; i++)
        {
            
            var array = minoSettingList[i].minoDataList;
            if (array == null) return;
            // 1つのグループとして縦に並べる
            EditorGUILayout.BeginVertical();
            // グループラベルとボタンを横に並べる
            GUILayout.BeginHorizontal(); // 水平配置を開始
            // 数字（i）を表示
            GUIStyle labelStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = Color.white } // ここでテキストの色を白に設定
            };

            GUI.Label(new Rect(currentX, currentY, 40, 40), i.ToString(), labelStyle);

            // ボタンの位置とサイズを計算
            Rect buttonRect = new Rect(currentX + 50, currentY, 20f, 20f); // ボタンの位置を調整
            Rect minusButtonRect = new Rect(currentX + 70, currentY, 20f, 20f); // マイナスボタンの位置を調整
            Rect selectButtonRect = new Rect(currentX + 90, currentY, 20f, 20f); // マイナスボタンの位置を調整
            // ボタンを配置
            if (GUI.Button(buttonRect, "+"))
            {
                // 新しいプルダウンメニューを追加
                if (selectedGroupOptions.Count <= i)
                {
                    selectedGroupOptions.Add(new List<int>());
                }
                selectedGroupOptions[i].Add(0); // 新しい選択肢（0）を追加
            }
            
            // マイナスボタンを配置
            if (GUI.Button(minusButtonRect, "-"))
            {
                // 選択肢を削除
                selectedGroupOptions[i].RemoveAt(selectedGroupOptions[i].Count - 1);
                maxGroupHeight = 0;
            }
 
            minoSettingList[i].select = GUI.Toggle(selectButtonRect, minoSettingList[i].select, "");
            
            GUILayout.EndHorizontal(); // 水平配置を終了
            
            for (int y = 0; y < array.GetLength(0); y++)
            {
                for (int x = 0; x < array.GetLength(1); x++)
                {
                    // 各セルの位置とサイズを計算
                    Rect rect = new Rect(currentX + x * (cellSize + padding), (currentY+30) + y  * (cellSize + padding), cellSize, cellSize);

                    // 画像を描画
                    GUI.DrawTexture(rect, GetButtonImage(array[y, x]));
                    // 画像がクリックされたか判定
                    if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        // クリックされた場合の処理
                        array[y, x] = (array[y, x] + 1) % 3;
                        // クリック処理後に描画を更新
                        Repaint();
                    }
                }
            }
            
            // グループ内のプルダウンメニューを表示（グループごとに1つ）
            if (minoEffectStatusMaster != null && minoEffectStatusMaster.MinoEffectStatus.Count > 0)
            {
                // グループごとの選択肢がリストにない場合は初期化
                if (selectedGroupOptions.Count <= i)
                {
                    selectedGroupOptions.Add(new List<int>()); // デフォルトで最初の選択肢を選択
                }
                // プルダウンメニューのリスト
                List<int> groupOptions = selectedGroupOptions[i];

                // 各プルダウンメニューを表示
                for (int j = 0; j < groupOptions.Count; j++)
                {
                    // プルダウンメニューの位置とサイズを計算
                    Rect popupRect = new Rect(currentX, (currentY + 30) + (array.GetLength(0) * (cellSize + padding)) + 10f + (j * 25f), 100f, 20f);

                    // Popupを表示
                    int selectedOption = EditorGUI.Popup(popupRect, groupOptions[j], minoEffectStatusMaster.MinoEffectStatus.ToArray());

                    // 選択肢が変わった場合、選択肢リストを更新
                    if (groupOptions[j] != selectedOption)
                    {
                        groupOptions[j] = selectedOption;
                    }

                }
                // 最も多くプルダウンメニューを追加したグループの数を記録
                if (groupOptions.Count > maxGroupHeight)
                {
                    maxGroupHeight = groupOptions.Count;
                }
            }
            EditorGUILayout.EndVertical();
            // 次のグループは横に並べる
            currentX += array.GetLength(1) * (cellSize + padding) + gridPadding;

            // 配列の幅が広くなりすぎたら、次の行に折り返す
            if (currentX > position.width - (array.GetLength(1) * (cellSize + padding)))
            {
                currentX = startX; // X座標をリセット
                currentY += array.GetLength(0) * (cellSize + padding) + gridPadding + 30f + (maxGroupHeight * 25f) ; // Y座標を更新（30f は番号のスペース）
            }
        }
   }
   
   private void LoadData()
    {
        string assetPath = "Assets/Data/MinosData.asset";
        var minoData = AssetDatabase.LoadAssetAtPath<MinoData>(assetPath);

        if (minoData == null)
        {
            // データが存在しない場合、新規に作成
            minoData = ScriptableObject.CreateInstance<MinoData>();
            AssetDatabase.CreateAsset(minoData, assetPath);
            AssetDatabase.SaveAssets();
        }
        selectedGroupOptions.Clear();
        minoSettingList.Clear();
        foreach (var data in minoData.Parameters)
        {
            MinoSetting setting = new MinoSetting();

            // minos を復元
            setting.minoDataList = ConvertToNestedList(data.minos, data.rows, data.cols);

            minoSettingList.Add(setting);

            // selectedGroupOptions の変換
            if (data.selectedGroupOptions != null)
            {
                foreach (var effect in data.selectedGroupOptions)
                {
                    List<int> groupOptions = new();
                    int effectIndex = minoEffectStatusMaster.MinoEffectStatus.IndexOf(effect);

                    // 見つからない場合はエラーチェック
                    if (effectIndex >= 0)
                    {
                        groupOptions.Add(effectIndex);
                    }

                    selectedGroupOptions.Add(groupOptions);
                }
            }
        }
    }
    
    private void SaveData()
    {
        string assetPath = "Assets/Data/MinosData.asset";
        MinoData database = AssetDatabase.LoadAssetAtPath<MinoData>(assetPath);

        if (database == null)
        {
            // データが存在しない場合、新規に作成
            database = ScriptableObject.CreateInstance<MinoData>();
            AssetDatabase.CreateAsset(database, assetPath);
        }

        // MinoEffects を一度クリア
        database.Parameters.Clear();

        int count = 0;
        foreach (var mino in minoSettingList)
        {
            MinoParameter para = new MinoParameter();

            // minos の変換
            para.minos = Flatten2DList(mino.minoDataList);
            para.rows = mino.minoDataList.GetLength(0);
            para.cols = mino.minoDataList.GetLength(1);
            
            // 各Minoの選択肢リストを初期化
            List<string> option = new List<string>();
            
            // eslectedGroupOptions の内容で MinoEffects を更新
            if (selectedGroupOptions[count].Count != 0)
            {
                // 選択肢が MinoEffectStatus に基づいて追加される
                foreach (var val in selectedGroupOptions[count])
                {
                    option.Add(minoEffectStatusMaster.MinoEffectStatus[val]);
                }
            }

            para.selectedGroupOptions = new List<string>(option);

            database.Parameters.Add(para);
            count++;
        }

        // 保存を反映
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
    }
    List<int> Flatten2DList(int[,] nestedList)
    {
        List<int> flatList = new List<int>();

        for (int i = 0; i < nestedList.GetLength(0); i++)
        {
            for (int j = 0; j < nestedList.GetLength(1); j++)
            {
                if (nestedList[i, j] == 2)
                {
                    flatList.Add(-1);
                }
                else
                {
                    flatList.Add(nestedList[i, j]);
                }
                
            }
        }

        return flatList;
    }

    int[,] ConvertToNestedList(List<int> flatList, int rows, int cols)
    {
        int[,] nestedList = new int[rows,cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (flatList[i * cols + j] == -1)
                {
                    nestedList[i, j]  = 2;
                }
                else
                {
                    nestedList[i, j] = flatList[i * cols + j];
                }
            }
        }
        return nestedList;
    }
    private void OnDestroy()
    {
        //SaveData();
    }
   // 画像取得用のメソッド
   private Texture2D GetButtonImage(int state)
   {
      switch (state)
      {
         case 0: return buttonFalseImage;
         case 1: return buttonTrueImage;
         case 2: return buttonRotImage;
         default: return buttonFalseImage;
      }
   }
}
#endif
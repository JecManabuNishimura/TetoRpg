using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "MinoEffectData", menuName = "MinoEffect/MinoEffectData")]
public class MinoEffectData : ScriptableObject
{
    private static MinoEffectData _entity;

    public static MinoEffectData Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                string assetPath = "Assets/Data/MinoEffectData.asset";
                _entity = AssetDatabase.LoadAssetAtPath<MinoEffectData>(assetPath);

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(MinoEffectData) + " not found");
                }
            }

            return _entity;
        }
    }
    // 番号のリスト
    public List<MinoEffect> MinoEffects;

    public List<string> GetMinoEffect(int index)
    {
        return MinoEffects.First(_ => _.numbers == index).selectedGroupOptions;
    }
}

[Serializable]
public struct MinoEffect
{
    public int numbers;
    public List<string> selectedGroupOptions;
}
public class MinoEffectCreater : EditorWindow
{
    private Texture2D image;
    private float cellSize = 25f;  // セルのサイズ
    private float padding = 1f;    // セル間の余白
    private float gridPadding = 10f; // 配列間の余白
    private MinoEffectStatusMaster minoEffectStatusMaster;
    private List<List<int>> selectedGroupOptions = new List<List<int>>(); // 各グループに複数選択肢を保持
    private float maxGroupHeight = 0f; // 全体の高さを管理
    private MinoEffectData minoEffectData; // 保存する ScriptableObject
    [MenuItem("Window/MinoEffectCreate")]
    public static void ShowWindow()
    {
        GetWindow<MinoEffectCreater>("MinoEffectCreate");
    }
    private void OnEnable()
    {
        // MinoEffectStatusMasterを取得
        minoEffectStatusMaster = MinoEffectStatusMaster.Entity;
        LoadData();
    }

    private void OnGUI()
    {
        // 画像を読み込む (例: Resources フォルダに画像を置く)
        if (image == null)
        {
            image = Resources.Load<Texture2D>("blockPurepleDimond"); // 画像のパスに変更
        }

        if (image == null)
        {
            GUILayout.Label("Image not found in Resources.");
            return;
        }

        // 配列の表示
        float startX = padding; // X座標の開始位置
        float startY = padding; // Y座標の開始位置
        
        float currentX = startX; // X方向の現在の位置
        float currentY = startY + 20; // Y方向の現在の位置
        // 保存ボタンを一番上に配置
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Width(60), GUILayout.Height(20))) // サイズを小さくした保存ボタン
        {
            SaveData();
        }
        GUILayout.EndHorizontal();
        
        for (int i = 0; i < MinoFactory.TetoMinoLenght; i++)
        {
            // 1つのグループとして縦に並べる
            EditorGUILayout.BeginVertical();
            var array = MinoFactory.GetMinoData(i);

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
            
            GUILayout.EndHorizontal(); // 水平配置を終了

            for (int y = 0; y < array.GetLength(0); y++)
            {
                for (int x = 0; x < array.GetLength(1); x++)
                {
                    // ゼロ以外の値に画像を表示
                    if (array[y, x] != 0)
                    {
                        // 各セルの位置とサイズを計算
                        Rect rect = new Rect(currentX + x * (cellSize + padding), (currentY+30) + y  * (cellSize + padding), cellSize, cellSize);

                        // 画像を描画
                        GUI.DrawTexture(rect, image);
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
        string assetPath = "Assets/Data/MinoEffectData.asset";
        minoEffectData = AssetDatabase.LoadAssetAtPath<MinoEffectData>(assetPath);

        if (minoEffectData == null)
        {
            // データが存在しない場合、新規に作成
            minoEffectData = ScriptableObject.CreateInstance<MinoEffectData>();
            AssetDatabase.CreateAsset(minoEffectData, assetPath);
            AssetDatabase.SaveAssets();
        }

        // selectedGroupOptions をロード
        selectedGroupOptions.Clear();
        if (minoEffectData.MinoEffects != null)
        {
            foreach (var effect in minoEffectData.MinoEffects)
            {
                List<int> groupOptions = new List<int> { effect.numbers }; // 番号のリストを格納
                selectedGroupOptions.Add(groupOptions);
            }
        }
    }
    private void SaveData()
    {
        string assetPath = "Assets/Data/MinoEffectData.asset";
        MinoEffectData database = AssetDatabase.LoadAssetAtPath<MinoEffectData>(assetPath);

        if (database == null)
        {
            // データが存在しない場合、新規に作成
            database = ScriptableObject.CreateInstance<MinoEffectData>();
            AssetDatabase.CreateAsset(database, assetPath);
        }

        // MinoEffects を一度クリア
        if (database.MinoEffects == null)
        {
            database.MinoEffects = new List<MinoEffect>();
        }
        else
        {
            database.MinoEffects.Clear();  // リストの中身をクリア
        }

        // selectedGroupOptions の内容で MinoEffects を更新
        for (int i = 0; i < selectedGroupOptions.Count; i++)
        {
            List<int> groupOptions = selectedGroupOptions[i];

            MinoEffect effect = new MinoEffect
            {
                numbers = i,  // 番号はインデックス
                selectedGroupOptions = new List<string>()
            };

            // 選択肢を selectedGroupOptions から追加
            foreach (var option in groupOptions)
            {
                effect.selectedGroupOptions.Add(minoEffectStatusMaster.MinoEffectStatus[option]);
            }

            // リストに新しい MinoEffect を追加
            database.MinoEffects.Add(effect);
        }

        // 保存を反映
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
    }
    private void OnDestroy()
    {
        SaveData();
    }
}

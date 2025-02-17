using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WeaponEffectMaster", menuName = "Scriptable Objects/WeaponEffectMaster")]
public class WeaponEffectMaster : ScriptableObject
{
    private static WeaponEffectMaster _entity;

    public static WeaponEffectMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<WeaponEffectMaster>("Master/WeaponEffectMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(WeaponEffectMaster) + " not found");
                }
            }

            return _entity;
        }
    }


    public List<CreateWeaponData> WeaponDatas = new();

    public WeaponEffectGroup GetWeaponEffect(string weaponId, int groupId)
    {
         return WeaponDatas.FirstOrDefault(x => x.weaponId == weaponId && x.groupData.id == groupId)?.groupData;
    }

    public void ResetGroupID()
    {
        string prevId = "";
        int counter = 0;
        foreach (var item in WeaponDatas)
        {
            if(prevId == item.weaponId)
            {
                item.groupData.id = counter;
                counter++;
            }
            else
            {
                prevId = item.weaponId;
                counter = 0;
                item.groupData.id = counter;
                counter++;
            }
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(WeaponEffectMaster))]
public class WeaponEffectMasterEditor : Editor
{
    private string searchQuery = "";
    private string weaponId = "";
    private List<bool> selectNumber = new ();
    private TreasureDropMaster[] drops;
    private void OnEnable()
    {
        drops = Resources.LoadAll<TreasureDropMaster>("Master/TreasureDrop");
        foreach (var VARIABLE in drops)
        {
            selectNumber.Add(false);
        }
    }

    public override void OnInspectorGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            int count = 0;
            foreach (var d in drops)
            {
                string fileName = "";
                string assetPath = AssetDatabase.GetAssetPath(d);
                fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                string[] parts = fileName.Split('_'); // "_" で分割
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(parts[1],GUILayout.Width(50));    
                    selectNumber[count] = EditorGUILayout.Toggle(selectNumber[count],GUILayout.Width(60));    
                }
                count++;
            }
        }

        // WeaponEffectMasterのインスタンスを取得
        WeaponEffectMaster weaponEffectMaster = (WeaponEffectMaster)target;
        GUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Search", GUILayout.Width(60));
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("weaponId", GUILayout.Width(60));
            weaponId = EditorGUILayout.TextField( weaponId, GUILayout.Width(100));
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Effect", GUILayout.Width(60));
            searchQuery = EditorGUILayout.TextField(searchQuery, GUILayout.Width(100));
        }
        weaponEffectMaster.ResetGroupID();
        EditorGUILayout.EndVertical();
        // WeaponDatasリストの表示
        EditorGUILayout.LabelField("WeaponDatas", EditorStyles.boldLabel);
        // WeaponDatasを検索文字列に基づいてフィルタリング
        var filteredWeaponDatas = weaponEffectMaster.WeaponDatas
            .Where(weaponData =>
                (string.IsNullOrEmpty(weaponId) || weaponData.weaponId.Contains(weaponId, System.StringComparison.OrdinalIgnoreCase)) && // weaponIdでフィルタリング
                (string.IsNullOrEmpty(searchQuery) || weaponData.groupData.effects
                    .Any(e => e.effect.ToString().Contains(searchQuery, System.StringComparison.OrdinalIgnoreCase))) // Effectでフィルタリング
            )
            .ToList();

        for (int i = 0; i < filteredWeaponDatas.Count; i++)
        {
            var weaponData = filteredWeaponDatas[i];
            if(weaponData.useStage == null)
            {
                weaponData.useStage = new List<string>();
            }
            weaponData.useStage.Clear();
            foreach (var d in drops)
            {
                if(d.itemDropData.FirstOrDefault(x => x.id == weaponData.weaponId && x.groupId == weaponData.groupData.id) != null)
                {
                    string fileName = "";
#if UNITY_EDITOR
                    // AssetDatabaseを使ってファイルパスを取得
                    string assetPath = AssetDatabase.GetAssetPath(d);  // dはTreasureDropMasterのインスタンス
                    fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath); // ファイル名（拡張子なし）を取得
                    // ファイル名から "_" 以降を取り出す
                    string[] parts = fileName.Split('_'); // "_" で分割
                    if (parts.Length > 1)
                    {
                        // "_" 以降の部分（最初の部分を除く）
                        fileName = parts[1];
                    }
                    else
                    {
                        // "_" がない場合はそのままのファイル名を使用
                        fileName = parts[0];
                    }
#endif

                    // useStageにファイル名を追加
                    weaponData.useStage.Add(fileName);
                }
            }
            // WeaponIdの表示
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField(weaponData.weaponId + "  " + weaponData.groupData.id, GUILayout.Width(60));
                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                {
                    weaponEffectMaster.WeaponDatas.Remove(weaponData);
                    return;
                }

                
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    for (int s = 0; s < selectNumber.Count; s++)
                    {
                        if (selectNumber[s])
                        {
                            drops[s].itemDropData.Add(new ItemDropData()
                            {
                                id = weaponData.weaponId,
                                groupId = weaponData.groupData.id,
                                dropRarity = Rarity.D,
                            });
                        }
                    }
                }
                if (GUILayout.Button("Remove", GUILayout.Width(50)))
                {
                    for (int s = 0; s < selectNumber.Count; s++)
                    {
                        if (selectNumber[s])
                        {
                            var item = drops[s].itemDropData
                              .Select((data, index) => new { data, index }) // インデックスも一緒に取得
                              .FirstOrDefault(x => x.data.id == weaponData.weaponId && x.data.groupId == weaponData.groupData.id);

                            drops[s].itemDropData.RemoveAt(item.index);
                        }
                    }
                }
                foreach (var s in weaponData.useStage)
                {
                    EditorGUILayout.TextArea(s, GUILayout.Width(50));
                }
            }




            EditorGUI.indentLevel++;
            int count = 0;
            foreach (var e in weaponData.groupData.effects)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
                    if (e.effect.ToString().Contains("Down"))
                    {
                        popupStyle.normal.textColor = Color.red;
                    }
                    else
                    {
                        popupStyle.normal.textColor = Color.blue;
                    }
                    
                    e.effect = (EffectStatus)EditorGUILayout.EnumPopup(e.effect, popupStyle, GUILayout.Width(150));

                    e.value = EditorGUILayout.IntField(e.value, GUILayout.Width(60));

                    if (GUILayout.Button("D", GUILayout.Width(20)))
                    {
                        weaponData.groupData.effects.RemoveAt(count);
                    }
                }
                count++;
            }
            EditorGUI.indentLevel--;
            // 区切り線を表示（オプション）
            EditorGUILayout.Space();
        }

        // 変更があった場合、インスペクタを更新
        if (GUI.changed)
        {
            EditorUtility.SetDirty(weaponEffectMaster);
        }

        // デフォルトのインスペクタを表示（必要に応じて）
        base.OnInspectorGUI();
    }
}
#endif

[Serializable]
public class WeaponEffectGroup
{
    public WeaponEffectGroup(int id, List<WeaponEffect> effects)
    {
        this.id = id;
        this.effects = effects;
    }
    [SerializeField] public int id;
    [SerializeField] public List<WeaponEffect> effects;
}

[Serializable]
public class WeaponEffect
{
    public WeaponEffect(EffectStatus effectStatus, int value)
    {
        effect = effectStatus;
        this.value = value;
    }
    [SerializeField] public EffectStatus effect;
    [SerializeField] public int value;
}

[Serializable]
public class CreateWeaponData
{
    [SerializeField] public string weaponId;
    [SerializeField] public WeaponEffectGroup groupData;
    [SerializeField] public bool deleteFlag;
    [SerializeField] public List<string> useStage;

}

/*
[CreateAssetMenu(fileName = "WeaponEffectMaster", menuName = "Scriptable Objects/WeaponEffectMaster")]
public class WeaponEffectMaster : ScriptableObject
{
    public static WeaponEffectMaster Instance;
    

    public  SerializableDictionary<string, List<WeaponEffectGroup>> weaponEffectGroups = new ();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        
        ReadCSV("WeaponEffectList");
    }
  
    public void ReadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(',');
                string name = data[0];
                int groupId = int.Parse(data[1]);
                string effect = data[2];
                int value = int.Parse(data[3]);
                Enum.TryParse(effect, out EffectStatus state);

                if (weaponEffectGroups.ContainsKey(name))
                {
                    var group = weaponEffectGroups[name].FirstOrDefault(x => x.id == groupId);
                    if (group != null)
                    {
                        group.effects.Add(new WeaponEffect(state, value));
                    }
                    else
                    {
                        weaponEffectGroups[name].Add(new WeaponEffectGroup(groupId, new List<WeaponEffect>()
                        {
                            new WeaponEffect(state, value)
                        }));
                    }
                }
                else
                {
                    weaponEffectGroups.Add(name, new List<WeaponEffectGroup>());
                    weaponEffectGroups[name].Add(new WeaponEffectGroup(groupId, new List<WeaponEffect>()
                    {
                        new WeaponEffect(state, value)
                    }));
                }


            }
        }
        else
        {
            Debug.LogError(fileName + "が見つかりません。");
        }
        AssetDatabase.SaveAssets();  
        AssetDatabase.Refresh();
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(WeaponEffectMaster))]
public class WeaponEffectMasterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WeaponEffectMaster example = (WeaponEffectMaster)target;

        if (example.weaponEffectGroups == null)
        {
            example.weaponEffectGroups = new SerializableDictionary<string, List<WeaponEffectGroup>>();
        }
        
        // equipmentEffectDatas の ID 配列を作成
        string[] effectIds = new string[EquipmentMaster.Entity.equipData.Count];
        for (int i = 0; i < EquipmentMaster.Entity.equipData.Count; i++)
        {
            effectIds[i] = EquipmentMaster.Entity.equipData[i].id;
        }
        
        // CSVの読み込みボタン
        if (GUILayout.Button("ReadCSV"))
        {
            example.weaponEffectGroups.Clear();
            example.ReadCSV("WeaponEffectList");
            EditorUtility.SetDirty(example);
            AssetDatabase.SaveAssets();  
        }
        
        // 各キーのグループを折りたたみ可能に表示
        foreach (var key in example.weaponEffectGroups.Keys.ToList())
        {
            // Foldoutでグループを折りたたみ
            bool isExpanded = EditorPrefs.GetBool("WeaponEffect_" + key, true); // 折りたたみ状態を保存する
            isExpanded = EditorGUILayout.Foldout(isExpanded, key); // Foldoutでキー表示

            // 状態を保存
            EditorPrefs.SetBool("WeaponEffect_" + key, isExpanded);

            if (isExpanded)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Key編集
                    EditorGUILayout.LabelField("Key", GUILayout.Width(50));

                    // 現在のインデックスを取得
                    int currentIndex = System.Array.IndexOf(effectIds, key);
                    if (currentIndex == -1) currentIndex = 0;
                    currentIndex = EditorGUILayout.Popup(currentIndex, effectIds, GUILayout.Width(100));

                    // 新しいIDを設定
                    string newKey = effectIds[currentIndex];
                    if (newKey != key)
                    {
                        var group = example.weaponEffectGroups[key];
                        example.weaponEffectGroups.Remove(key);
                        example.weaponEffectGroups[newKey] = group;
                    }
                }
                EditorGUI.indentLevel++;

                GUIStyle coloredBoxStyle = new GUIStyle(GUI.skin.box)
                {
                    border = new RectOffset(1, 1, 1, 1),
                    margin = new RectOffset(5, 5, 5, 5),
                    padding = new RectOffset(10, 10, 10, 10)
                };

                // グループ内容の表示
                using (new EditorGUILayout.VerticalScope(coloredBoxStyle))
                {
                    foreach (var effect in example.weaponEffectGroups[key])
                    {
                        using (new EditorGUILayout.HorizontalScope("Box"))
                        {
                            EditorGUILayout.LabelField(effect.id.ToString(), GUILayout.Width(40));
                        }
                        EditorGUI.indentLevel++;

                        for (int i = 0; i < effect.effects.Count; i++)
                        {
                            using (new EditorGUILayout.HorizontalScope("Box"))
                            {
                                var data = effect.effects[i];
                                data.effect = (EffectStatus)EditorGUILayout.EnumPopup(data.effect, GUILayout.Width(100));
                                data.value = EditorGUILayout.IntField(data.value, GUILayout.Width(60));
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
    }
    // 線を引くメソッド
    private void DrawLine(Color color, float height = 1f)
    {
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(height));
        EditorGUI.DrawRect(rect, color);  // 指定した色で矩形（線）を描画
    }
}
#endif
*/
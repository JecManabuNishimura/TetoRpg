using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.SubsystemsImplementation;
using static UnityEngine.EventSystems.EventTrigger;
using Object = System.Object;


public class GameManager : MonoBehaviour
{
    public static Player player;
    public static Enemy.Charactor enemy;
    public static int boardWidth = 8;
    public static int boardHeight = 20;
    
    public static event Func<UniTask> EnemyAttack;
    public static event Action DownMino;
    public static event Func<Task> FallingMino;
    public static event Func<Task> CreateLineBlock;
    public static event Func<Task> CreateBlock;
    public static event Action ClearBlock;
    public static event Func<Task> StartBattle;
    public static event Func<Task> ChangeFallCount;
    public static event Action StageFlomSelectStage;
    public static Action StageClearAnim;
    

    public static Action BackGroundEmmision_Start;
    public static Action BackGroundEmmision_Stop;

    public static int healingPoint = 1;
    public static int DeleteLine;
    public static int DeleteMino;

    public static int playerDamage => (DeleteLine +(DeleteMino / 8)) * player.totalStatus.atk;

    // 操作用フラグ
    public static bool playerPut;
    public static bool DownFlag;
    public static bool BombFlag;
    public static bool maxPutposFlag;
    public static bool EnemyAttackFlag;
    public static bool LineCreateFlag;
    public static bool menuFlag;
    public static bool cameraFlag;
    public static bool EnemyHpVisibleFlag;
    public static bool EnemyDown;

    public static StageLoader stageLoader;
    public static Stage nowStage = Stage.None;
    public static StageData stageData;

    public static CameraMove cameraMove;
    public static Transform enemyPos;

    public static GameObject trantision;

    public static int NextUpCountAmount = 1;

    public static List<GameObject> dontdestoryObj = new List<GameObject>();

    private static bool StageClearFlag = false;
    //public static int NowNextCount => stageLoader.NextCount + GameManager.player.BelongingsMinoEffect["NextGaugeUp"] * 2 - GameManager.player.BelongingsMinoEffect["NextGaugeDown"] * 2;

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BoardManager.Instance.ClearBoard();
            ClearBlock?.Invoke();
            enemy.EnemyDeath();
            enemy = null;
            MapManager.Instance.EndBattle();
            nowStage = GetNextEnumValue(nowStage);
            stageLoader.SetStageStatus();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            await StageClear();
        }

    }

    public static async Task PlayerMove()
    {
        while (true)
        {
            if(playerPut)
            {
                if (!maxPutposFlag)
                {
                    enemy.CountDown();    
                }
                
                if (DownFlag)
                {
                    await Task.Delay(1000);
                    DownMino?.Invoke();
                    // ボムで消されている場合
                    if (BombFlag)
                    {
                        await FallingMino?.Invoke()!;
                    }
                    await Task.Delay(500); 
                }
                if (EnemyDown)
                {
                    //EnemyDown = false;
                    BoardManager.Instance.ClearBoard();
                    ClearBlock?.Invoke();
                    enemy.EnemyDeath();
                    enemy = null;
                    if (MapManager.Instance.EndBattle())
                    {
                        StageClearFlag = true;
                    }
                    else
                    {
                        nowStage = GetNextEnumValue(nowStage);
                        stageLoader.SetStageStatus();    
                    }
                    
                }
                else
                {
                    enemy.CheckputPos();
                    if (EnemyAttackFlag)
                    {
                        if (EnemyAttack != null)
                        {
                            Debug.Log("攻撃始まり    ");
                            await EnemyAttack.Invoke();
                        }

                        await Task.Delay(500);

                        DownMino?.Invoke();
                        await Task.Delay(1000);
                    }
                    

                    if (LineCreateFlag)
                    {
                        await CreateLineBlock?.Invoke();
                    }
                }
            }
            // 攻撃終了を待つ
            if (!EnemyAttackFlag && !maxPutposFlag &&
                !DownFlag && !LineCreateFlag)
            {
                if (enemy != null)
                {
                    await ChangeFallCount?.Invoke();    
                }
                
                if (maxPutposFlag)
                {
                    continue;
                }
                playerPut = false;
                break;
            }
            await Task.Yield(); // フレームの待機
        }
    }

    void Awake()
    {
        EquipmentMaster.Entity.ReadCSV("EquipmentData");
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
        string uiName = "EquipmentUI";
        string BattleName = "BattleScene";
        if(!IsSceneLoaded(uiName))
        {
            StartCoroutine(LoadSceneAdditive(uiName));
        }

        if(!IsSceneLoaded(BattleName))
        {
            StartCoroutine(LoadSceneAdditive(BattleName));
        }
    }
    private void Start()
    {
        player.Initialize();
        StageStart(Stage.Stage1);
    }
    public static void StageStart(Stage stage)
    {
        nowStage = stage;
        stageLoader.SetStageStatus();
        cameraFlag = true;
        cameraMove.MoveCamera();
    }

    public static async UniTask StageClear()
    {
        StageClearAnim?.Invoke();
        trantision.GetComponent<Transition>().StartTran();
        while (true)
        {
            if (trantision.GetComponent<PlayableDirector>().state == PlayState.Paused)
            {
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("StageSelect");

                while (asyncOperation.isDone)
                {
                    await UniTask.Yield();
                }
                UnLoad();
                await UniTask.WaitForSeconds(1.5f);

                await EndTransition();
                break;
            }
            await UniTask.Yield();
        }
    }

    public static async UniTask EndTransition ()
    {
        trantision.GetComponent<Transition>().EndTran();
        while (trantision.GetComponent<PlayableDirector>().state == PlayState.Playing)
        {
            await UniTask.Yield();
        }
        foreach (var obj in dontdestoryObj)
        {
            Destroy(obj);
        }
        dontdestoryObj.Clear();
        Destroy(trantision);
    }

    public static void UnLoad()
    {
        cameraMove = null;
    }

    public static async void Battle()
    {
        
        EnemyDown = false;
        boardWidth = stageData.boardWith;
        boardHeight = stageData.boardHeight;
        BoardManager.Instance.Initialize();
        // フィールドブロック増減
        boardWidth += player.BelongingsMinoEffect["FieldUp"];
        boardWidth -= player.BelongingsMinoEffect["FieldDown"];
        enemy = Instantiate(MapManager.Instance.GetEnemyObj, enemyPos.position, Quaternion.identity).GetComponent<Enemy.Charactor>();
        
        await CreateBlock?.Invoke();
        
        await StartBattle?.Invoke();
    }
 
    // 非同期でシーンを追加するコルーチン
    private IEnumerator LoadSceneAdditive(string name)
    {
        // シーンの読み込みを非同期で行う
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

        // シーンの読み込みが完了するまで待つ
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    // シーンがすでにロードされているかを確認するメソッド
    private bool IsSceneLoaded(string sceneName)
    {
        // すべてのロードされているシーンを確認
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName)
            {
                return true; // シーンがロード済み
            }
        }
        return false; // シーンがロードされていない
    }
    static T GetNextEnumValue<T>(T current) where T : Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, current);

        // 次の値を取得（最後の値の場合は最初に戻る）
        index = (index + 1) % values.Length;
        return values[index];
    }
}

[Serializable]
public class StageData
{
    public int boardHeight;
    public int boardWith;
    public int HealDropRate;
    public int BombDropRate;
    public int StripesDropRate;
    public int TresureDropRate;
}

namespace MyMethods
{
    public static class MyExtensions
    {
        public static void ChildClear(this Transform tran)
        {
            for (int i = tran.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(tran.GetChild(i).gameObject);
            }
        }

        public static int toInt(this string str)
        {
            return int.Parse(str);
        }
        
    }
}

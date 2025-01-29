using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameManager 
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager(); // 初回アクセス時にインスタンスを作成
            }
            return instance;
        }
    }
    public Player player;
    public Enemy.Charactor enemy;
    public int boardWidth = 8;
    public int boardHeight = 20;
    
    public event Func<UniTask> EnemyAttack;
    public event Action DownMino;
    public event Func<Task> FallingMino;
    public event Func<Task> CreateLineBlock;
    public event Func<Task> CreateBlock;
    public event Action ClearBlock;
    public event Func<Task> StartBattle;
    public event Func<Task> ChangeFallCount;
    public event Action StageFlomSelectStage;
    public Action StageClearAnim;
    

    public Action BackGroundEmmision_Start;
    public Action BackGroundEmmision_Stop;

    public int healingPoint = 1;
    public int DeleteLine;
    public int DeleteMino;

    public int playerDamage => (DeleteLine +(DeleteMino / 8)) * player.totalStatus.atk;

    // 操作用フラグ
    public bool playerPut;
    public bool DownFlag;
    public bool BombFlag;
    public bool maxPutposFlag;
    public bool EnemyAttackFlag;
    public bool LineCreateFlag;
    public bool menuFlag;
    public bool cameraFlag;
    public bool EnemyHpVisibleFlag;
    public bool EnemyDown;

    public StageLoader stageLoader;
    public Stage nowStage = Stage.None;
    public StageData stageData;

    public CameraMove cameraMove;
    public Transform enemyPos;

    public GameObject trantision;

    public int NextUpCountAmount = 1;

    public List<GameObject> dontdestoryObj = new List<GameObject>();

    private bool StageClearFlag = false;
    //public static int NowNextCount => stageLoader.NextCount + GameManager.Instance.player.BelongingsMinoEffect["NextGaugeUp"] * 2 - GameManager.Instance.player.BelongingsMinoEffect["NextGaugeDown"] * 2;
    
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
    }

    public async Task PlayerMove()
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
                        cameraMove.MoveCamera();
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

    public void StageStart(Stage stage)
    {
        nowStage = stage;
        stageLoader.SetStageStatus();
        cameraFlag = true;
        cameraMove.MoveCamera();
    }

    public async UniTask StageClear()
    {
        StageClearAnim?.Invoke();
        while (true)
        {
            if (trantision.GetComponentInChildren<PlayableDirector>().state == PlayState.Paused)
            {
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("StageSelect");
                
                while (asyncOperation.isDone)
                {
                    await UniTask.Yield();
                }
                foreach (var obj in dontdestoryObj)
                {
                    GameObject.Destroy(obj);
                }
                UnLoad();
                await UniTask.WaitForSeconds(1.5f);

                await EndTransition();
                break;
            }
            await UniTask.Yield();
        }
    }

    public async UniTask EndTransition ()
    {
        trantision.GetComponentInChildren<Transition>().Restart();
        while (trantision.GetComponentInChildren<PlayableDirector>().state == PlayState.Playing)
        {
            await UniTask.Yield();
        }

        dontdestoryObj.Clear();
        GameObject.Destroy(trantision);
    }

    public void UnLoad()
    {
        cameraMove = null;
        BoardManager.Instance.DestroyInstance();
    }

    public async void Battle()
    {
        
        EnemyDown = false;
        boardWidth = stageData.boardWith;
        boardHeight = stageData.boardHeight;
        BoardManager.Instance.Initialize();
        // フィールドブロック増減
        boardWidth += player.BelongingsMinoEffect["FieldUp"];
        boardWidth -= player.BelongingsMinoEffect["FieldDown"];
        enemy = GameObject.Instantiate(MapManager.Instance.GetEnemyObj, enemyPos.position, Quaternion.identity).GetComponent<Enemy.Charactor>();
        
        await CreateBlock?.Invoke();
        
        await StartBattle?.Invoke();
    }
 
    // 非同期でシーンを追加するコルーチン

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

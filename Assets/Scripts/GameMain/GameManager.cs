using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = System.Object;

public class GameManager : MonoBehaviour
{
    public static Player player;
    public static Enemy.Charactor enemy;
    public static int boardWidth = 10;
    public static int boardHeight = 21;
    
    public static event Func<Task> EnemyAttack;
    public static event Action DownMino;
    public static event Func<Task> FallingMino;
    public static event Func<Task> CreateLineBlock;

    public static int healingPoint = 5;
    public static int DeleteLine;

    public static int playerDamage => DeleteLine * player.status.atk;

    // 操作用フラグ
    public static bool playerPut;
    public static bool DownFlag;
    public static bool BombFlag;
    public static bool maxPutposFlag;
    public static bool EnemyAttackFlag;
    public static bool LineCreateFlag;
    public static bool menuFlag;

    public static StageLoader stageLoader;
    public static Stage nowStage = Stage.None;
    public static StageData stageData;

    public static void StageStart(Stage stage)
    {
        nowStage = stage;
        stageLoader.SetStageStatus();
    }
    public static async Task PlayerMove()
    {
        while (true)
        {
            if(playerPut)
            {
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
                if(EnemyAttackFlag)
                {
                    await EnemyAttack?.Invoke()!;
                    await Task.Delay(500); 
                    
                    DownMino?.Invoke();
                    await Task.Delay(1000);
                }

                if (LineCreateFlag)
                {
                    await CreateLineBlock?.Invoke();
                }
            }
            // 攻撃終了を待つ
            if (!EnemyAttackFlag && !maxPutposFlag &&
                !DownFlag && !LineCreateFlag)
            {
                playerPut = false;
                break;
            }
            await Task.Yield(); // フレームの待機
        }
    }

    void Awake()
    {
        BoardManager.Instance.Initialize();
    }

    private void Start()
    {
        player.Initialize();
        StageStart(Stage.Stage1);
    }
}

[Serializable]
public class StageData
{
    public int HealDropRate;
    public int BombDropRate;
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
    }
}

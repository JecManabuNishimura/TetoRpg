using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SubsystemsImplementation;
using static UnityEngine.EventSystems.EventTrigger;
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
    public static event Func<Task> CreateBlock;
    public static event Action ClearBlock;
    public static event Func<int,Task> StartBattle;

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
    public static bool cameraFlag;
    public static bool EnemyDown;

    public static StageLoader stageLoader;
    public static Stage nowStage = Stage.None;
    public static StageData stageData;

    public static CameraMove cameraMove;
   
    

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
                if (EnemyDown)
                {
                    BoardManager.Instance.ClearBoard();
                    ClearBlock?.Invoke();
                    Destroy(enemy.gameObject);
                    MapManager.Instance.EndBattle();
                    nowStage = GetNextEnumValue(nowStage);
                    stageLoader.SetStageStatus();
                }
                else
                {
                    if (EnemyAttackFlag)
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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        BoardManager.Instance.Initialize();
        string uiName = "EquipmentUI";
        string mapName = "Map";
        if(!IsSceneLoaded(uiName))
        {
            StartCoroutine(LoadSceneAdditive(uiName));
        }

        if(!IsSceneLoaded(mapName))
        {
            StartCoroutine(LoadSceneAdditive(mapName));
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

    public static async void Battle()
    {
        EnemyDown = false;
        
        enemy = Instantiate(MapManager.Instance.GetEnemyObj, stageLoader.EnemyPos, Quaternion.identity).GetComponent<Enemy.Charactor>();
        await CreateBlock?.Invoke();
        await StartBattle?.Invoke(stageLoader.NextCount);
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
    public int HealDropRate;
    public int BombDropRate;
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
    }
}

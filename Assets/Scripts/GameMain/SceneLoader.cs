using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] CameraStackManager cameraManager;
    private async void Awake()
    {
        await StartAsync();
        cameraManager.Initialize();
    }
    public async Task StartAsync()
    {
        string uiName = "EquipmentUI";
        string BattleName = "BattleScene";
        if (!IsSceneLoaded(uiName))
        {
            await LoadSceneAdditive(uiName);
        }

        if (!IsSceneLoaded(BattleName))
        {
            await LoadSceneAdditive(BattleName);
        }
    }
    private async UniTask LoadSceneAdditive(string name)
    {
        // シーンの読み込みを非同期で行う
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

        // シーンの読み込みが完了するまで待つ
        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
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

    // Update is called once per frame
    void Update()
    {
        
    }
}

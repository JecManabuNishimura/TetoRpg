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
        // �V�[���̓ǂݍ��݂�񓯊��ōs��
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

        // �V�[���̓ǂݍ��݂���������܂ő҂�
        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
        }
    }
    // �V�[�������łɃ��[�h����Ă��邩���m�F���郁�\�b�h
    private bool IsSceneLoaded(string sceneName)
    {
        // ���ׂẴ��[�h����Ă���V�[�����m�F
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName)
            {
                return true; // �V�[�������[�h�ς�
            }
        }
        return false; // �V�[�������[�h����Ă��Ȃ�
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;

public class GameInitialize : MonoBehaviour
{
    private void Awake()
    {
        EquipmentMaster.Entity.ReadCSV("EquipmentData");
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
    }
    private void Start()
    {
        GameManager.Instance.player = new Player();
        GameManager.Instance.player.status.maxHp = 100;
        GameManager.Instance.player.status.atk = 5;
        GameManager.Instance.player.status.def = 3;
        GameManager.Instance.player.Initialize();
    }
}

using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [SerializeField] private MapEncountEnemy encountEnemy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector3 GetEnemyPos
    {
        get
        {
            if(encountEnemy.GetEnemyData()  != null)
            {
                return encountEnemy.GetEnemyData().transform.position;
            }
            return Vector3.zero;
        }
    }
    public GameObject GetEnemyObj => encountEnemy.GetEnemyData().GetComponent<EncountEnemy>().GetEnemyObj;
    public void EndBattle()
    {
        encountEnemy.DeleteEnemy();
        GameManager.cameraFlag = true;
    }
}

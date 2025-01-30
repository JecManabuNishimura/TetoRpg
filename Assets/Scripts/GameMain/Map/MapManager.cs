using System;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    [SerializeField] private AudioSource Bgm;
    [SerializeField] private AudioSource Se;
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
        SoundMaster.Entity.SetBGMAudio(Bgm);
        SoundMaster.Entity.SetSEAudio(Se);
    }

    private void Start()
    {
        
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
    public bool EndBattle()
    {
        encountEnemy.DeleteEnemy();
        GameManager.Instance.cameraFlag = true;
        return encountEnemy.GetEnemyData() == null;
        
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class MapEncountEnemy : MonoBehaviour
{
    [SerializeField] private List<GameObject> EnemyList;

    public GameObject GetEnemyData()
    {
        if(EnemyList.Count != 0)
        {
            return EnemyList[0];
        }

        return null;
    }

    public void DeleteEnemy()
    {
        if(EnemyList.Count != 0)
        {
            EnemyList.RemoveAt(0);
        }
    }
}

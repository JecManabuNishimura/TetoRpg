using System;
using UnityEngine;

public class EnemyPoint : MonoBehaviour
{

    private void Start()
    {
        GameManager.Instance.enemyPos = transform;
    }
}

using System;
using UnityEngine;

public class EncountEnemy : MonoBehaviour
{
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    void OnValidate()
    {
        spriteRenderer.sprite = enemyObj.GetComponent<SpriteRenderer>().sprite;
    }

    public GameObject GetEnemyObj => enemyObj;
}

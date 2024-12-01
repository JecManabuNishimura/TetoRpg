using System;
using UnityEngine;

public class EncountEnemy : MonoBehaviour
{
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    void OnValidate()
    {
        Debug.Log("test");
        spriteRenderer.sprite = enemyObj.GetComponent<SpriteRenderer>().sprite;
    }
}

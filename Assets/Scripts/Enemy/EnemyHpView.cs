using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nowHp;
    [SerializeField] private TextMeshProUGUI maxHp;

    private void Start()
    {
        maxHp.text = GameManager.enemy.status.maxHp.ToString();
    }

    private void Update()
    {
        nowHp.text = GameManager.enemy.status.hp.ToString();
    }
}

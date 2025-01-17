using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpView : MonoBehaviour
{
    [SerializeField] private GameObject hpTextObj;
    [SerializeField] private TextMeshProUGUI nowHp;
    [SerializeField] private TextMeshProUGUI maxHp;

    private void Start()
    {
        if(GameManager.enemy != null)
        {
            maxHp.text = GameManager.enemy.status.maxHp.ToString();
        }
    }

    private void Update()
    {
        if (GameManager.enemy != null && GameManager.EnemyHpVisibleFlag)
        {
            hpTextObj.SetActive(true);
            nowHp.text = GameManager.enemy.status.hp.ToString();
            maxHp.text = GameManager.enemy.status.maxHp.ToString();
        }
        else
        {
            hpTextObj.SetActive(false);
        }
    }
}

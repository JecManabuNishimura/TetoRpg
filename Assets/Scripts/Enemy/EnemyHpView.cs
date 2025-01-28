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
        if(GameManager.Instance.enemy != null)
        {
            maxHp.text = GameManager.Instance.enemy.status.maxHp.ToString();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.enemy != null && GameManager.Instance.EnemyHpVisibleFlag)
        {
            hpTextObj.SetActive(true);
            nowHp.text = GameManager.Instance.enemy.status.hp.ToString();
            maxHp.text = GameManager.Instance.enemy.status.maxHp.ToString();
        }
        else
        {
            hpTextObj.SetActive(false);
        }
    }
}

using System;
using TMPro;
using UnityEngine;

public class CountAmount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void Update()
    {
        text.text = GameManager.Instance.NextUpCountAmount.ToString();
    }
}

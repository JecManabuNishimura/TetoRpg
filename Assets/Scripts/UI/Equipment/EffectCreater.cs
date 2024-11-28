using TMPro;
using UnityEngine;

public class EffectCreater : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI effectText;
    [SerializeField]private TextMeshProUGUI powerText;

    public void SetText(string effect, string power)
    {
        effectText.text = effect;
        powerText.text = power;
    }
}

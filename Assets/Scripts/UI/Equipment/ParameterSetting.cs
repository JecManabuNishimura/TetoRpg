using TMPro;
using UnityEngine;

public class ParameterSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void SetText(string text)
    {
        this.text.text = text;
    }

}

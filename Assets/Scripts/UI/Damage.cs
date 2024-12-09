using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Damage : MonoBehaviour
{
   [SerializeField] private TextMeshPro damageText;

    public void ChangeText(string text)
    {
        damageText.text = text;
    }

    public void DestroyObj()
    {
        Destroy(this.gameObject);
    }
}

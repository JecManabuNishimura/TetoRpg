using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FallCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fallText;
        [SerializeField] private Animator anim;
        public void ChangeText(string text)
        {
            anim.Play("CountAnimation",0,0);
            fallText.text = text;
        }
    }
}

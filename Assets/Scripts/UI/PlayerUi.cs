using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUi : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI hpText;
    [SerializeField] public ParticleSystem healingEffect;   // UI‚¶‚á‚È‚¢‚ª‚¢‚Á‚½‚ñ‰¼
    [SerializeField] public TextMeshPro damageText;
    [SerializeField] public Slider slider;
    
    void Start()
    {
        GameManager.Instance.player.ui = this;
        damageText.enabled = false;
        slider.value = 1;
        hpText.text = GameManager.Instance.player.totalStatus.hp.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        GameManager.Instance.player.ui = null;
    }
}

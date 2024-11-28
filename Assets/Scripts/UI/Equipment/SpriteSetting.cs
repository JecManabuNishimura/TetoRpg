using UnityEngine;
using UnityEngine.UI;

public class SpriteSetting : MonoBehaviour
{
    [SerializeField] public Image image;
    [SerializeField] public Sprite noSettingImage;

    public void SetImage(Sprite sprite)
    {
        image.color = Color.white;
        image.sprite = sprite;
    }

    public void SetActive(bool flag)
    {
        image.enabled = flag;
    }

    public void SetNoSettingMode()
    {
        image.sprite = noSettingImage;
        Color col = Color.black;
        col.a = 0.2f;
        image.color = col;
    }
}

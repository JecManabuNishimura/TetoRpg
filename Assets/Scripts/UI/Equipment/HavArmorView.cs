using System;
using UnityEngine;
using UnityEngine.UI;

public class HavArmorView : MonoBehaviour
{
    [SerializeField] private Image armorImage;
    [SerializeField] private Image BackGroundImage;
    
    public void ChangeArmorImage(Sprite sprite)
    {
        armorImage.sprite = sprite;
    }
}

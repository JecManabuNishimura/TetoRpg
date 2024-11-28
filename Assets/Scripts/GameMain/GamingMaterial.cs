using System;
using System.Collections;
using UnityEngine;

public class GamingMaterial : MonoBehaviour
{
    Material material = null;
    private bool startFlag = false;
    private Material myMaterial;
    private Color matColor;
    
    [Header("色変更スパン")]
    public float Chnge_Color_Time = 0.1f;

    [Header("変更の滑らかさ")]
    public float Smooth = 0.01f;

    [Header("色彩")]
    [Range(0, 1)] public float HSV_Hue = 1.0f;// 0 ~ 1

    [Header("彩度")]
    [Range(0, 1)] public float HSV_Saturation = 1.0f;// 0 ~ 1

    [Header("明度")]
    [Range(0, 1)] public float HSV_Brightness = 1.0f;// 0 ~ 1

    [Header("色彩 MAX")]
    [Range(0, 1)] public float HSV_Hue_max = 1.0f;// 0 ~ 1

    [Header("色彩 MIN")]
    [Range(0, 1)] public float HSV_Hue_min = 0.0f;// 0 ~ 1

    public int MaxCounter;

    private int counter;
    // Start is called before the first frame update
    void Start()
    {
        material =  GetComponent<Renderer>().material;
        matColor = material.color;
        HSV_Hue = HSV_Hue_min;
        counter = 0;
    }

    private void Update()
    {

    }

    public void StartChangeColor()
    {
        counter = 0;
        if (!startFlag)
        {
            startFlag = true;
            StartCoroutine(Change_Color());
        }
    }

    IEnumerator Change_Color()
    {
        if (counter == MaxCounter)
        {
            HSV_Hue = 0;
            material.color = matColor;
            startFlag = false;
            yield break;
        }

        counter++;
        HSV_Hue += Smooth;

        if (HSV_Hue >= HSV_Hue_max)
        {
            HSV_Hue = HSV_Hue_min;
        }
        if(material != null)
            material.color = Color.HSVToRGB(HSV_Hue, HSV_Saturation, HSV_Brightness);

        yield return new WaitForSeconds(Chnge_Color_Time);

        StartCoroutine("Change_Color");
    }
}

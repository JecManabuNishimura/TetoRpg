using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "NewImageDatabase", menuName = "ScriptableObjects/ImageDatabase")]
public class ImageDatabase : ScriptableObject
{
    public List<Sprite> imageList;  // 複数の画像を保持するリスト
    public List<Sprite> LifeImageList;  // 複数の画像を保持するリスト

    public Sprite bombImage;
    public Sprite stripesImage;
    public Sprite ObstacleImage;
    public Sprite SkillCancelImage;
    public Sprite ObstacleStopImage;
    public Sprite ShapeFixImage;
    public Sprite DeleteCancelImage;

}

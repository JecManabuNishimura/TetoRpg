using System;
using UnityEngine;

public class MinoBlock : MonoBehaviour
{
    [SerializeField] private ImageDatabase imgDB;
    [SerializeField] private ParticleSystem particle;
    public int index;
    public MinoType minoType;
    public int TreasureNumber;
    public bool deleteFlag = false;
    public Vector3 TreimagePos;
    

    public void SetMinoData(MinoType type, int index,int number = 0,Vector3 pos = new ())
    {
        minoType = type;
        this.index = index;
        TreasureNumber = number;
        TreimagePos = pos;
        if (type == MinoType.Treasure)
        {
            GetComponent<MeshRenderer>().enabled = false;
            return;
        }
        
        GetComponent<Renderer>().material.mainTexture =
            type switch
            {
                MinoType.Normal => imgDB.imageList[index % 10].texture,
                MinoType.Life => imgDB.LifeImageList[index% 10].texture,
                MinoType.Bomb => imgDB.bombImage.texture,
                MinoType.Stripes => imgDB.stripesImage.texture,
            };
    }

    public void CreateDownEffect()
    {
        Instantiate(particle, transform.position + Vector3.up * 3, Quaternion.Euler(-90f, 0f, 0f));
    }
}

public enum MinoType
{
    Normal,
    Life,
    Bomb,
    Stripes,
    Treasure,
}

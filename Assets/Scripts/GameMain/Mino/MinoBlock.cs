using UnityEngine;

public class MinoBlock : MonoBehaviour
{
    [SerializeField] private ImageDatabase imgDB;

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
                MinoType.Normal => imgDB.imageList[index].texture,
                MinoType.Life => imgDB.LifeImageList[index].texture,
                MinoType.Bomb => imgDB.bombImage.texture,
            };
    }
}

public enum MinoType
{
    Normal,
    Life,
    Bomb,
    Treasure,
}
using MyMethods;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;


public class testBlock : MonoBehaviour
{
    [SerializeField] private GameObject backGround;
    [SerializeField] private GameObject backGroundFream;
    public GameObject block;
    public GameObject checkBlock;
    private GameObject checkParent;
    public GameObject parent;
    
    private List<GameObject> blockList = new();
    private GameObject bgObj;
    private GameObject bgfObj;
    private Tween emissionTween;
    public int distance = 1;
    void Start()
    {
        parent.transform.ChildClear();
        GameManager.CreateLineBlock += CreateLineBlock;
        GameManager.CreateBlock += CreateBlock;
        GameManager.ClearBlock += ClearBlock;
        GameManager.BackGroundEmmision_Start += BackGroundEmission_Start;
        GameManager.BackGroundEmmision_Stop += BackGroundEmission_Stop;
        checkParent = new GameObject(){name = "check"};
        //BoardManager.Instance.SetTestBlock += SetTestBlock;
    }

    private void CreateObj(Vector3 pos)
    {
        GameObject obj =  Instantiate(block,parent.transform);
        obj.transform.localPosition = pos;

        obj.name = $"{pos.x}:{pos.y}";
        blockList.Add(obj);
    }
    private void ClearBlock()
    {
        if(emissionTween != null)
        {

            //material.SetColor("_EmissionColor", initColor);
            DOTween.To(
                () => Color.red * 100, // 現在のエミッションカラーを取得
                x => material.SetColor("_EmissionColor", x), // エミッションカラーをセット
                initColor, // 最終目標の色（強度10倍）
                1f // アニメーションの時間（1秒）
            ).SetEase(Ease.Linear);
            emissionTween.Kill();
            emissionTween = null;
        }
        parent.transform.ChildClear();
        Destroy(bgObj);
        Destroy(bgfObj);
    }

    private Material material;
    private Color initColor;
    private async Task CreateBlock()
    {
        
        for (int y = 0; y < GameManager.boardHeight -1; y++)
        {
            for (int x = 0; x < GameManager.boardWidth; x++)
            {
                CreateObj(new Vector3(x, y, 0));
            }
            await Task.Yield();
        }
        
        /*
        for (int y = 0; y < GameManager.boardHeight -1; y++)
        {
            for (int x = 0; x < (GameManager.boardWidth / 2) + 1; x++)
            {
                CreateObj(new Vector3(x, -y, 0));
                CreateObj(new Vector3(GameManager.boardWidth - x -1, -y, 0));
                await Task.Yield();
            }
        }
        */

        bgObj = Instantiate(backGround);
        material = bgObj.GetComponent<SpriteRenderer>().material;
        initColor = material.GetColor("_EmissionColor");



        bgObj.GetComponent<SpriteRenderer>().size =
            new Vector2(GameManager.boardWidth + 0.8f, GameManager.boardHeight-0.1f);
        bgObj.transform.position = new Vector3( GameManager.boardWidth / 2.0f -0.5f ,
            GameManager.boardHeight / 2.0f ,15);
        bgfObj = Instantiate(backGroundFream);
        bgfObj.GetComponent<SpriteRenderer>().size =
            new Vector2(GameManager.boardWidth + 1f, GameManager.boardHeight);
        bgfObj.transform.position = new Vector3(GameManager.boardWidth / 2.0f - 0.5f,
            GameManager.boardHeight / 2.0f, 15);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameManager.LineCreateFlag = true;
        }
    }

    void BackGroundEmission_Start()
    {
        if (emissionTween == null)
        {
            emissionTween = DOTween.To(
                () => initColor, // 現在のエミッションカラーを取得
                x => material.SetColor("_EmissionColor", x), // エミッションカラーをセット
                Color.red * 100, // 最終目標の色（強度10倍）
                1f // アニメーションの時間（1秒）
            ).SetLoops(-1, LoopType.Yoyo) // 繰り返し（往復）
            .SetEase(Ease.Linear); // イージングを線形に設定（一定速度）
        }
    }
    void BackGroundEmission_Stop()
    {
        if(emissionTween != null)
        {

            //material.SetColor("_EmissionColor", initColor);
            DOTween.To(
                () => Color.red * 100, // 現在のエミッションカラーを取得
                x => material.SetColor("_EmissionColor", x), // エミッションカラーをセット
                initColor, // 最終目標の色（強度10倍）
                1f // アニメーションの時間（1秒）
            ).SetEase(Ease.Linear);
            emissionTween.Kill();
            emissionTween = null;
        }
    }

    async Task CreateLineBlock()
    {
        int count = 0;
        List<GameObject> objlist = new();
        while (true)
        {
            
            if (count < GameManager.boardHeight-1)
            {
                GameObject obj = Instantiate(block,parent.transform);
                obj.transform.localPosition = new Vector3(GameManager.boardWidth, 1, 0);
                objlist.Add(obj);
                count++;
            }
            
            bool endflag = true;
            for (int i = 0; i < objlist.Count; i++)
            {
                if ((int)objlist[i].transform.localPosition.y != -(GameManager.boardHeight - 2) + i)
                {
                    var pos = objlist[i].transform.localPosition;
                    pos.y -= 1;
                    objlist[i].transform.localPosition = pos;
                    endflag = false;
                }
            }
            await Task.Delay(50);

            if (endflag)
            {
                break;
            }
        }

        GameManager.LineCreateFlag = false;
        GameManager.boardWidth++;
        BoardManager.Instance.ResetBoard();
    }

    void SetTestBlock()
    {
        int childCount = checkParent.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(checkParent.transform.GetChild(i).gameObject);
        }

        for (int y = 0; y < GameManager.boardHeight; y++)
        {
            for (int x = 0; x < GameManager.boardWidth; x++)
            {
                if(BoardManager.Instance.board[y,x] != 0)
                {
                    GameObject obj= Instantiate(checkBlock, new Vector3(x, y, 5), Quaternion.identity);
                    obj.transform.parent = checkParent.transform;
                }
            }
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class testBlock : MonoBehaviour
{
    public GameObject block;
    public GameObject checkBlock;
    private GameObject checkParent;
    public GameObject parent;

    private List<GameObject> blockList = new();
    
    public int distance = 1;
    void Start()
    {
        GameManager.CreateLineBlock += CreateLineBlock;
        GameManager.CreateBlock += CreateBlock;
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
    private async Task CreateBlock()
    {
        /*
        for (int y = 0; y < GameManager.boardHeight -1; y++)
        {
            for (int x = 0; x < GameManager.boardWidth; x++)
            {
                CreateObj(new Vector3(x, -y, 0));
                await Task.Yield();
            }
        }
        */
        for (int y = 0; y < GameManager.boardHeight -1; y++)
        {
            for (int x = 0; x < (GameManager.boardWidth / 2) + 1; x++)
            {
                CreateObj(new Vector3(x, -y, 0));
                CreateObj(new Vector3(GameManager.boardWidth - x -1, -y, 0));
                await Task.Yield();
            }
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameManager.LineCreateFlag = true;
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

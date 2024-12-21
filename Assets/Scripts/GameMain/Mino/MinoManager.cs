using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyMethods;
using UI;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class MinoManager : MonoBehaviour 
{
    [SerializeField] private GameObject minoObj;
    [SerializeField] private GameObject ClearMino;
    [SerializeField] private GameObject CreatePoint;
    [SerializeField] private ImageDatabase ImageDatabase;
    [SerializeField] private GameObject obstacleMino;
    [SerializeField] private GameObject TreasureObj;
    [SerializeField] private GameObject holdObj;
    //[SerializeField] private NextUpGauge nextUpGauge;
    [SerializeField] private GameObject damgeText;
    [SerializeField] private GameObject newTextObj;
    
    private GameObject SelectMino;
    private GameObject clMinoObj;
    private GameObject[,] minoDataTable;
    private List<Treasure> treasuresTable = new();
    private GameObject minoListObj;

    private GameObject treasureSpriteObj;

    private int[,] nowMinos;

    private int index;
    private int rotNum;
    private float fallTimer;
    private int downColPos;

    private int fallCount;

    private InputHandler inputHandler;
    private float fallTime = 1;

    private bool treasureFlag = false;
    private int treasureNumber = 0;
    private bool holFlag = false;

    private List<HoldMinoData> holdMino = new();
    private Vector2Int delPos;


    private void Start()
    {
        GameManager.StartBattle += StartBattle;
        
        clMinoObj = new GameObject
        {
            transform =
            {
                position = new Vector3(0, 0, 0)
            }
        };

        BoardManager.Instance.ChangeColor += Instance_ChangeColor;
        BoardManager.Instance.DeleteMino += DeleteMino;
        BoardManager.Instance.DownMino += DownMino;
        BoardManager.Instance.UpMino += UpMino;
        BoardManager.Instance.TableNullMino += SetMinoTableNull;
        BoardManager.Instance.ResetTable += ResetDataTable;
        BoardManager.Instance.CheckTreasure += CheckTreasure;
        BoardManager.Instance.ClearTable += ClearTable;
        
        inputHandler = new InputHandler
        {
            RotateMino = RotMino,
            MoveLeft = () => MoveLeftRight(true),
            MoveRight = () => MoveLeftRight(false),
            MoveDown = MoveDown,
            Fall = Fall,
            ChangeColor = () => StartCoroutine(ChangeColor(5, 10)),
            HoldMino = HoldMino,
        };
        

        minoListObj = new GameObject
        {
            name = "minoListObj"
        };
        holdObj.SetActive(GameManager.player.BelongingsMinoEffect["HoldBlock"] != 0);
    }

    private async Task StartBattle(int gaugeNum)
    {
        minoDataTable = new GameObject[GameManager.boardHeight, GameManager.boardWidth];
        NextUpGauge.Instance.CreateGauge(gaugeNum);
        await NextUpGauge.Instance.Play();
        CreateNewMino();
        downColPos = (int)SelectMino.transform.position.x;
    }

    private void ResetDataTable()
    {
        var newTable = new GameObject[GameManager.boardHeight, GameManager.boardWidth];
        for (int y = 0; y < minoDataTable.GetLength(0); y++)
        {
            for (int x = 0; x < minoDataTable.GetLength(1); x++)
            {
                newTable[y, x] = minoDataTable[y, x];
            }
        }
        minoDataTable = newTable;
    }

    private void ClearTable()
    {
        for (int y = 0; y < minoDataTable.GetLength(0); y++)
        {
            for (int x = 0; x < minoDataTable.GetLength(1); x++)
            {
                Destroy(minoDataTable[y, x]);
            }
        }
        NextUpGauge.Instance.Clear();
    }
    private void Update()
    {
        // 落下速度
        fallTime -= GameManager.player.BelongingsMinoEffect["DownSpeedUp"] * 0.2f;
        fallTime += GameManager.player.BelongingsMinoEffect["DownSpeedDown"]* 0.2f;
        // ホールド対応
        holdObj.SetActive(GameManager.player.BelongingsMinoEffect["HoldBlock"] != 0);
        // 予測ブロック対応
        clMinoObj.SetActive(GameManager.player.BelongingsMinoEffect["PredictionBlock"] != 0);
        if (!SelectMino || GameManager.menuFlag) return;


        inputHandler.HandleInput();

        if (fallTime <= 0.1f)
        {
            fallTime = 0.1f;
        }
        fallTimer += Time.deltaTime;
        if (fallTimer >= fallTime)
        {
            //MoveDown();
            fallTimer = 0;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            fallTimer = 0;
        }
    }

    private void Instance_ChangeColor(int x,int y)
    {
        delPos = new Vector2Int(x, y);
        StartCoroutine(ChangeColor(x, y));
    }

    Vector2Int[,] moveDirection = new Vector2Int[,]
    {
        {Vector2Int.zero,new Vector2Int( 1,0),new Vector2Int( 1, 1),new Vector2Int(0,-2),new Vector2Int( 1,-2) }, // N->W
        {Vector2Int.zero,new Vector2Int(-1,0),new Vector2Int(-1,-1),new Vector2Int(0, 2),new Vector2Int(-1, 2) }, // W->S
        {Vector2Int.zero,new Vector2Int(-1,0),new Vector2Int(-1, 1),new Vector2Int(0,-2),new Vector2Int(-1,-2) }, // S->E
        {Vector2Int.zero,new Vector2Int( 1,0),new Vector2Int( 1,-1),new Vector2Int(0, 2),new Vector2Int( 1, 2) }, // E->N

    };

    int HitCheck(int[,] mino , Vector2Int vec, int num = 0)
    {
        // https://www.youtube.com/watch?v=0OQ7mP97vdc　回転参考
        if (num >= 8)
        {
            return num; // 最大回数を超えたら num を返す
        }
        
        for (int y = 0; y < mino.GetLength(0); y++)
        {
            for (int x = 0; x < mino.GetLength(1); x++)
            {
                if (mino[y, x] != 0)
                {
                    Vector2Int newVec = vec;
                    switch (num)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            newVec += moveDirection[rotNum, num];
                            break;
                        case 5:
                            {
                                // Ｘ軸の確認　右端
                                int mindex = newVec.x + GetMaxIndexCol(mino);
                                int minIndex = newVec.x + GetMinIndexCol(mino);
                                int maxYindex = newVec.y - GetMaxIndexRow(mino);
                                if (maxYindex > 0 && maxYindex < GameManager.boardHeight)
                                {
                                    //飛び出している場合
                                    if (mindex > GameManager.boardWidth)
                                    {
                                        // 飛び出た分だけ戻して確認する
                                        // 調整
                                        newVec.x += mindex - GameManager.boardWidth;
                                        if (!BoardManager.Instance.HitCheck(newVec.x, newVec.y))
                                        {
                                            return HitCheck(mino, vec, 7); 
                                        }
                                    }
                                    else if (minIndex < 0)
                                    {
                                        return HitCheck(mino, vec, 6);
                                    }
                                }
                                else
                                {
                                    return HitCheck(mino, vec, 7); 
                                }
                            }
                            break;
                
                        case 6:
                            {
                                int minIndex = newVec.x + GetMinIndexCol(mino);
                                // Ｘ軸の確認　左端
                                //if (newVec.y - y > 0 && newVec.y - y < GameManager.boardHeight)
                                if (newVec.y - y > 0)
                                {
                                    if (minIndex < 0)
                                    {
                                        newVec.x += 0 - minIndex;
                                        if (newVec.y < GameManager.boardHeight)
                                        {
                                            if (!BoardManager.Instance.HitCheck(newVec.x, newVec.y))
                                            {
                                                return HitCheck(mino, vec, 7);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return HitCheck(mino, vec, 7); // 次の num で再帰
                                }
                            }
                            break;
                        case 7:
                            {
                                int maxYindex = newVec.y - GetMaxIndexRow(mino);
                                if (maxYindex <= 0 )
                                {
                                    newVec.y += 0 - maxYindex + 1;
                                    if (!BoardManager.Instance.HitCheck(newVec.x, newVec.y))
                                    {
                                        return HitCheck(mino, vec, num + 1); // 次の num で再帰
                                    }
                                }
                            }
                            break;
                    }
                    newVec.x += x;
                    newVec.y -= y;
                    if (newVec.x >= 0 && newVec.x < GameManager.boardWidth &&
                        newVec.y > 0 )
                     {
                        // 一番上は無視
                        if (newVec.y < GameManager.boardHeight)
                        {
                            if (!BoardManager.Instance.HitCheck(newVec.x, newVec.y))
                            {
                                return HitCheck(mino, vec, num + 1); // 次の num で再帰
                            }
                        }
                     }
                    else
                    {
                        if(num <=4)
                        {
                            // 外チェックに入る
                            return HitCheck(mino, vec, 5);
                        }
                    }
                }
            }
        }
        return num;
    }

    int GetMaxIndexCol(int[,] mino)
    {
        int max = 0;
        for (int y = 0; y < mino.GetLength(0); y++)
        {
            for (int x = 0; x < mino.GetLength(1); x++)
            {
                if (mino[y, x] != 0 && max < x)
                {
                    max = x;
                }
            }
        }

        return max;
    }
    int GetMinIndexCol(int[,] mino)
    {
        int min = mino.GetLength(1);
        for (int y = 0; y < mino.GetLength(0); y++)
        {
            for (int x = 0; x < mino.GetLength(1); x++)
            {
                if (mino[y, x] != 0 && min > x)
                {
                    min = x;
                }
            }
        }

        return min;
    }

    int GetMaxIndexRow(int[,] mino)
    {
        int max = 0;
        for (int y = 0; y < mino.GetLength(0); y++)
        {
            for (int x = 0; x < mino.GetLength(1); x++)
            {
                if (mino[y, x] != 0 && max < y)
                {
                    max = y;
                }
            }
        }

        return max;
    }

    private void RotMino()
    {
        if (SelectMino == null || treasureFlag) return;
        // 回転禁止
        if (GameManager.player.BelongingsMinoEffect["RotBlockCancel"] != 0) return;
        var nowPos = SelectMino.transform.position;
        rotNum = (rotNum +1) % 4;
        int[,] minoRot = MinoFactory.RotatePiece(nowMinos, rotNum,index);
        int hitNum = HitCheck(minoRot, new Vector2Int((int)nowPos.x, (int)nowPos.y));

        if (hitNum == 8)
        {
            rotNum--;
            return;
        }
        nowPos += hitNum switch
        {
            0 => (Vector2)moveDirection[rotNum, hitNum],
            1 => (Vector2)moveDirection[rotNum, hitNum],
            2 => (Vector2)moveDirection[rotNum, hitNum],
            3 => (Vector2)moveDirection[rotNum, hitNum],
            4 => (Vector2)moveDirection[rotNum, hitNum],
            5 => -new Vector3(nowPos.x + GetMaxIndexCol(minoRot) - GameManager.boardWidth + 1, 0,0),
            6 => new Vector3(0 - (nowPos.x + GetMinIndexCol(minoRot)), 0,0),
            7 => new Vector3(0,(0 -(nowPos.y - GetMaxIndexRow(minoRot)) + 1),0),
        };
        int childCount = SelectMino.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(SelectMino.transform.GetChild(i).gameObject);
            Destroy(clMinoObj.transform.GetChild(i).gameObject);
        }

        Destroy(SelectMino);

        clMinoObj.transform.position = Vector3.zero;
        CreatePiece(minoRot);
        SelectMino.transform.position = nowPos;
        CheckUnder();
    }

    void DeleteMino(int x, int y, bool AttackFlag)
    {
        if (minoDataTable[y, x] != null)
        {
            // トレジャーデータ削除
            Action TreDelete = () =>
            {
                var list = new List<MinoBlock>();
                for (int yy = 0; yy < GameManager.boardHeight; yy++)
                {
                    for (int xx = 0; xx < GameManager.boardWidth; xx++)
                    {
                        if(minoDataTable[yy, xx] != null)
                        {
                            if (minoDataTable[yy, xx].GetComponent<MinoBlock>().TreasureNumber == minoDataTable[y, x].GetComponent<MinoBlock>().TreasureNumber)
                            {
                                list.Add(minoDataTable[yy, xx].GetComponent<MinoBlock>());
                            }
                        }
                    }
                }

                list.ForEach(block => block.deleteFlag = true);
                // 宝箱の削除
                foreach (var treasure in treasuresTable.ToList())
                {
                    if (treasure.number == minoDataTable[y, x].GetComponent<MinoBlock>().TreasureNumber)
                    {
                        if (!AttackFlag)
                        {
                            var unique = GameManager.stageLoader.GetDropData().GetItemDataId();
                            if (int.TryParse(unique.WeaponId, out int result))
                            {
                                // ミノだった場合
                                var obj = CreateMiniMino(MinoData.Entity.GetMinoData(result));
                                obj.transform.parent = treasure.spriteObj.transform.GetChild(0).transform.transform;
                                obj.transform.localPosition = new Vector3(0, 0.5f, -1);
                                treasure.spriteObj.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                                treasure.spriteObj.GetComponent<PlayableDirector>().Play();
                                if(GameManager.player.AcquisitionMino(unique))
                                {
                                    var text = Instantiate(newTextObj, obj.transform);
                                    text.transform.localPosition = new Vector3(0, 3f, -1);
                                }
                            }
                            else
                            {
                                // 装備品だった場合
                                treasure.spriteObj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite
                                    = EquipmentDatabase.Entity.GetEquipmentSpriteData(unique.WeaponId)
                                        .sprite;
                                treasure.spriteObj.GetComponent<PlayableDirector>().Play();
                                if (GameManager.player.AcquisitionItem(unique))
                                {
                                    var text = Instantiate(newTextObj, treasure.spriteObj.transform.GetChild(0).transform);
                                    text.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                    text.transform.localPosition = new Vector3(0, 1f, -1);
                                    
                                }
                            }
                            
                        }
                        Destroy(treasure.spriteObj, 2f); // アニメーション終了まで
                        treasuresTable.Remove(treasure);
                    }
                }
            };

            if (!AttackFlag)
            {
                //============================================
                // 各種ブロック効果
                //============================================
                switch (minoDataTable[y, x].GetComponent<MinoBlock>().minoType)
                {
                    case MinoType.Life:
                        GameManager.player.Healing();
                        break;
                    case MinoType.Normal:
                        break;
                    case MinoType.Bomb:
                        EffectMaster.Entity.PlayEffect(EffectType.Explotion,new Vector2(x,y));
                        Destroy(minoDataTable[y, x]);
                        minoDataTable[y, x] = null;
                        BoardManager.Instance.DeleteBomb(x,y);
                        GameManager.BombFlag = true;
                        return;
                    case MinoType.Stripes:
                        Destroy(minoDataTable[y, x]);
                        minoDataTable[y, x] = null;
                        BoardManager.Instance.DeleteStripes(x,y);
                        break;
                    case MinoType.Treasure:
                        TreDelete();
                        break;
                }
            }
            else if(minoDataTable[y, x].GetComponent<MinoBlock>().minoType == MinoType.Treasure)
            {
                TreDelete();
            }

            Destroy(minoDataTable[y, x]);
            minoDataTable[y, x] = null;
        }
    }

    bool CheckTreasure(int x,int y)
    {
        if (minoDataTable[y, x].GetComponent<MinoBlock>().deleteFlag)
        {
            Destroy(minoDataTable[y, x]);
            minoDataTable[y, x] = null;
            return true;
        }
        return false;
    }

    void SetMinoTableNull(int x ,int y)
    {
        minoDataTable[y, x] = null;
    }

    

    // トレジャーボックス情報取得
    Treasure GetTreasureData(int x,int y)
    {
        foreach(var data in treasuresTable)
        {
            if(data.number == minoDataTable[y, x].GetComponent<MinoBlock>().TreasureNumber)
            {
                return data;
            }
        }
        return null;
    }
    
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
    //   ホールド対応
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
    void HoldMino()
    {
        if (GameManager.player.BelongingsMinoEffect["HoldBlock"] == 0) return;
        if (SelectMino == null || treasureFlag || holFlag) return;
        holFlag = true;
        if(holdMino.Count == 0)
        {
            HoldMinoData data = new HoldMinoData()
            {
                minoData = nowMinos,
                minoObj = CreateMiniMino(nowMinos),
                index = index
            };
            holdMino.Add(data);
            Destroy(SelectMino.gameObject);
            clMinoObj.transform.ChildClear();
            CreateNewMino();
            
        }
        else
        {
            HoldMinoData data = new HoldMinoData()
            {
                minoData = nowMinos,
                minoObj = CreateMiniMino(nowMinos),
                index = index
            };
            holdMino.Add(data);
            index = holdMino[0].index;
            Destroy(SelectMino.gameObject);
            SelectMino = null;
            rotNum = 0;
            clMinoObj.transform.ChildClear();
            nowMinos = holdMino[0].minoData;
            clMinoObj.transform.position = Vector3.zero;
            CreatePiece(holdMino[0].minoData);
            
            SelectMino.transform.position = new Vector3(GameManager.boardWidth / 2-2,GameManager.boardHeight,0);
            CheckUnder();
            Destroy(holdMino[0].minoObj);
            holdMino.RemoveAt(0);
        }
    }
    void DownMino(int x,int y)
    {
        if (minoDataTable[y + 1, x] != null)
        {
            var t = GetTreasureData(x, y + 1);
            minoDataTable[y + 1, x].transform.position += Vector3.down;
            if(t != null && minoDataTable[y + 1, x].GetComponent<MinoBlock>().TreimagePos != Vector3.zero)
            {
                t.spriteObj.transform.position = minoDataTable[y + 1, x].transform.position + minoDataTable[y + 1, x].GetComponent<MinoBlock>().TreimagePos;
            }
        }
        minoDataTable[y, x] = minoDataTable[y + 1, x];
        if (minoDataTable[y + 1, x] != null)
        {
            minoDataTable[y + 1, x] = null;
        }
    }
    void MoveLeftRight(bool isLeft)
    {
        bool hitflag = false;
        foreach (Transform child in SelectMino.transform)
        {
            Vector3 newPosition = child.position + (isLeft ? -Vector3.right : Vector3.right);
            if (newPosition.x >= GameManager.boardWidth || newPosition.x < 0)
            {
                hitflag = true;
                break;
            }
            
            child.name = ((int)newPosition.x) + " " + ((int)newPosition.y);
            if (!BoardManager.Instance.IsValidPosition(newPosition))
            {
                hitflag = true;
                break;
            }
        }
        if (!hitflag)
        {
            SelectMino.transform.position += isLeft ? -Vector3.right : Vector3.right;
        }
        downColPos = (int)SelectMino.transform.position.x;
        CheckUnder();
    }

    //========================================================================
    // 　新しいミノを生成する
    //========================================================================
    private void CreateNewMino()
    {
        //--------------------------------------------------------------------------
        //  宝箱出現率調節
        //--------------------------------------------------------------------------
        if (GameManager.stageData.TresureDropRate != 0)
        {
            treasureFlag = Random.Range(0, GameManager.stageData.TresureDropRate) == 0 ? true : false;    
        }
        
        rotNum = 0;
        if(treasureFlag)
        {
            index = 0;
        }
        else
        {
            // 所持数最大7に固定（現在）
            index = GameManager.player.GetBelongingsMino(Random.Range(0, 7)).WeaponId.toInt();
        }
        clMinoObj.transform.position = Vector3.zero;       
        nowMinos = MinoFactory.GetMinoData(index, treasureFlag);
        CreatePiece(nowMinos,true);
        SelectMino.transform.position = new Vector3(GameManager.boardWidth / 2-2,GameManager.boardHeight,0);
        CheckUnder();
    }

    GameObject CreateMiniMino(int[,] minos)
    {
        GameObject parent = new GameObject();
        parent.name = index + "mino";
        float centerX = 0;
        float centerY = 0;
        // 0以外の位置をリストアップ
        List<Tuple<int, int>> positions = new List<Tuple<int, int>>();

        for (int i = 0; i < minos.GetLength(0); i++)
        {
            for (int j = 0; j < minos.GetLength(1); j++)
            {
                if (minos[i, j] != 0)
                {
                    positions.Add(Tuple.Create(i, j));  // (x, y)の位置を格納
                }
            }
        }
        if (positions.Count > 0)
        {
            // x座標、y座標の平均を計算
            foreach (var position in positions)
            {
                centerX += position.Item2;
                centerY += position.Item1;
            }

            centerX /= positions.Count;
            centerY /= positions.Count;
        }
        else
        {
            Debug.LogError("0以外の値は存在しません");
        }
        for (int y = 0; y < minos.GetLength(0); y++)
        {
            for (int x = 0; x < minos.GetLength(1); x++)
            {
                if (minos[y, x] != 0)
                {
                    MinoType type;
                    GameObject obj = Instantiate(minoObj, parent.transform, true);
                    // 回転軸用のマイナスもあるので、ABSをつける
                    type = GetType(Mathf.Abs(minos[y, x]));
                    obj.GetComponent<MinoBlock>().SetMinoData(type, index);
                    obj.transform.localPosition = new Vector3((x) - (centerX), -((y) -(centerY )), 0);
                }
            }
        }

        parent.transform.position = holdObj.transform.position;
        parent.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        return parent;
    }

    // ミノの作成
    void CreatePiece(int[,] minos, bool FastFlag = false)
    {
        GameObject parent = new GameObject();
        parent.name = index + "mino";
        for ( int y = 0; y < minos.GetLength(0); y++)
        {
            for (int x = 0; x < minos.GetLength(1); x++)
            {
                if (minos[y, x] != 0)
                {
                    GameObject obj = Instantiate(minoObj, parent.transform, true);
                    // テクスチャの設定
                    if(FastFlag)
                    {
                        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        //  特殊ミノ出現設定
                        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        //-----------------------------------------------------------------
                        // ライフの確率
                        //-----------------------------------------------------------------
                        if ((GameManager.stageData.HealDropRate != 0) &&
                            (Random.Range(0, 
                            GameManager.stageData.HealDropRate -
                            GameManager.player.BelongingsMinoEffect["HealRateUp"]) == 0))
                        {
                            minos[y, x] *= 2;
                        }
                        
                        //-----------------------------------------------------------------
                        // ボムの確率
                        //-----------------------------------------------------------------
                        else if ((GameManager.stageData.BombDropRate != 0) &&
                        (Random.Range(0, GameManager.stageData.BombDropRate) == 0))
                        {
                            minos[y, x] *= 3;
                        }
                        //-----------------------------------------------------------------
                        // 縞々の確率
                        //-----------------------------------------------------------------
                        else if ((GameManager.stageData.StripesDropRate != 0) &&
                                 (Random.Range(0, GameManager.stageData.StripesDropRate) == 0))
                        {
                            minos[y, x] *= 4;
                        }
                    }
                    MinoType type;
                    if (!treasureFlag)
                    {
                        // 回転軸用のマイナスもあるので、ABSをつける
                        type = GetType(Mathf.Abs(minos[y, x]));
                    }
                    else
                    {
                        type = MinoType.Treasure;
                    }
                    obj.GetComponent<MinoBlock>().SetMinoData(type, index,treasureNumber);

                    GameObject clearObj = Instantiate(ClearMino, clMinoObj.transform, true);
                    obj.transform.position = new Vector3(x, -y, 0);
                    clearObj.transform.position = new Vector3(x, -y, 0);
                }
            }
        }
        if(FastFlag && treasureFlag)
        {
            
            GameObject tre = Instantiate(TreasureObj, parent.transform, true);
            tre.transform.localPosition = new Vector3(1.5f, -1.5f, 1);
            treasureSpriteObj = tre;
            treasureNumber++;
        }

        SelectMino = parent;
        CheckUnder();
    }

    MinoType GetType(int index)
    {
        return Mathf.Abs(index) switch
        {
            1 => MinoType.Normal,
            2 => MinoType.Life,
            3 => MinoType.Bomb,
            4 => MinoType.Stripes,
        };
    }
    void CheckUnder()
    {
        int maxPos = 0;
        int chilPos = 0;
        int hitMaxHeight = 0;
        int hitHeight = 0;
        foreach (Transform child in SelectMino.transform)
        {
            int posX = (int)child.position.x;
            int posY = (int)child.position.y;
            if (posX != child.position.x) continue;
            while (true)
            {
                if (posY < GameManager.boardHeight)
                {
                    if (posY <= 0 || BoardManager.Instance.board[posY, posX] != 0)
                    {
                        hitHeight = posY - (int)child.localPosition.y;
                        break;
                    }
                }
                posY--;
            }
            if (maxPos <= posY || hitHeight >= hitMaxHeight)
            {
                hitMaxHeight = hitHeight;
                maxPos = posY;
                chilPos = (int)child.localPosition.y - 1;
            }
        }

        clMinoObj.transform.position =
            new Vector3(SelectMino.transform.position.x, maxPos - chilPos, clMinoObj.transform.position.z);
    }
    private async void Fall()
    {
        while (true)
        {
            foreach (Transform child in SelectMino.transform)
            {
                Vector3 newPosition = child.position + Vector3.down;
                child.name = ((int)newPosition.x) + " " + ((int)newPosition.y);
                if (!BoardManager.Instance.IsValidPosition(newPosition))
                {
                    PlacePiece();
                    for (int i = 0; i < SelectMino.transform.childCount; i++)
                    {
                        SelectMino.transform.GetChild(i).GetComponent<MinoBlock>()?.CreateDownEffect();    
                    }
                    SelectMino = null;
                    int childCount = clMinoObj.transform.childCount;

                    for (int i = childCount - 1; i >= 0; i--)
                    {
                        Destroy(clMinoObj.transform.GetChild(i).gameObject);
                    }


                    
                    GameManager.playerPut = true;
                    
                    
                    await GameManager.PlayerMove();
                    // 敵死亡時　何もしない
                    if(GameManager.EnemyDown)
                    {
                        return;
                    }
                    await ChangeFallCount();
                    CreateNewMino();
                    
                    return;
                }
            }
            SelectMino.transform.position += Vector3.down; 
        }
        
    }
    async void MoveDown()
    {
        bool hitflag = false;
        if (SelectMino == null) return;
        foreach (Transform child in SelectMino.transform)
        {
            Vector3 newPosition = child.position + Vector3.down;
            child.name = ((int)newPosition.x) + " " + ((int)newPosition.y);
            if (!BoardManager.Instance.IsValidPosition(newPosition))
            {
                PlacePiece();
                SelectMino = null;
                int childCount = clMinoObj.transform.childCount;

                for (int i = childCount - 1; i >= 0; i--)
                {
                    Destroy(clMinoObj.transform.GetChild(i).gameObject);
                }
                hitflag = true;
                break;
            }
        }

        if (!hitflag)
        {
            SelectMino.transform.position += Vector3.down; 
        }
        else
        {
            
            GameManager.playerPut = true;

            await GameManager.PlayerMove();
            // 敵死亡時　何もしない
            if (GameManager.EnemyDown)
            {
                return;
            }
            await ChangeFallCount();
            CreateNewMino();
        }

        CheckUnder();
    }

    async Task ChangeFallCount()
    {
        NextUpGauge.Instance.DownCount();
        if (NextUpGauge.Instance.GetCount < 0)
        {
            await CreateObstacleBlock();
            NextUpGauge.Instance.ResetGauge();
        }
    }

    IEnumerator ChangeColor(int col, int row)
    {
        int preX = -1;
        int preY = -1;
        if(col == -1)
        {
            col = downColPos;
        }
        for (int dis = 0; dis < GameManager.boardHeight; dis++)
        {
            for (int i = 0; i < 360; i++)
            {
                int fX = (int)(col + Mathf.Sin(Mathf.Deg2Rad * i) * dis);
                int fY = (int)(row + Mathf.Cos(Mathf.Deg2Rad * i) * dis);

                if (fX != preX || fY != preY)
                {
                    if (fX >= 0 && fY >= 0 && fX < GameManager.boardWidth && fY < GameManager.boardHeight)
                    {
                        if (minoDataTable[fY, fX] != null)
                            minoDataTable[fY, fX].GetComponent<GamingMaterial>().StartChangeColor();
                    }
                }

                preX = fX;
                preY = fY;

            }
            yield return new WaitForSeconds(0.08f);
        }
    }
    async Task PlacePiece()
    {
        bool treFlag = false;
        foreach (Transform child in SelectMino.transform)
        {
            // 小数点があるものは宝箱の絵
            // トレジャー用 小数点があるものは宝箱
            if(child.position.x != (int)child.position.x)
            {
                child.parent = null;
                continue;
            }
            var type = child.GetComponent<MinoBlock>().minoType;
            if (child.position.y >= GameManager.boardHeight)
            {
                continue;
            }
            BoardManager.Instance.SetBoardData((int)child.position.x, (int)child.position.y,treasureFlag);
            minoDataTable[(int)child.position.y, (int)child.position.x] = Instantiate(minoObj, minoListObj.transform, true);
            var table = minoDataTable[(int)child.position.y, (int)child.position.x];
            table.transform.position = new Vector3(child.position.x, child.position.y, 0);
            var minoBlock = table.GetComponent<MinoBlock>();
            
            Vector3 vec;
            // トレジャーの場合
            if (treasureSpriteObj != null)
            {
                // 宝箱の位置を保存しておく
                vec = new Vector3(
                    treasureSpriteObj.transform.position.x - child.transform.position.x,
                    treasureSpriteObj.transform.position.y - child.transform.position.y);
            }
            else
            {
                vec = Vector3.zero;
            }
            
            minoBlock.SetMinoData(
                type, 
                child.GetComponent<MinoBlock>().index,
                child.GetComponent<MinoBlock>().TreasureNumber,
                vec
                );

            // 一件のみ保存
            if (type == MinoType.Treasure && !treFlag)
            {
                Treasure t = new Treasure();
                t.spriteObj = treasureSpriteObj;
                t.number = table.GetComponent<MinoBlock>().TreasureNumber;
                treasureSpriteObj = null;
                treasuresTable.Add(t);
                treFlag = true;
            }
        }

        Destroy(SelectMino);
        // そろっているかチェック
        GameManager.DeleteLine = 0;     //　いったん初期化
        GameManager.DeleteMino = 0;
        BoardManager.Instance.CheckLine(0);
        // ダメージテキスト表示
        if (GameManager.playerDamage != 0)
        {
            var obj = Instantiate(damgeText, new Vector3(delPos.x,delPos.y , 0), Quaternion.identity);
            // クリティカル判定
            if (Random.Range(0, 100) < GameManager.player.status.critical)
            {
                // クリティカル2倍
                obj.GetComponent<Damage>().ChangeText((GameManager.playerDamage * 2).ToString());
            }
            else
            {
                var damage = GameManager.playerDamage / 2 - GameManager.enemy.status.def / 4;
                if (damage < 0)
                {
                    damage = 0;
                }
                obj.GetComponent<Damage>().ChangeText(damage.ToString());
            }
        }
        
        await GameManager.enemy.Damage(GameManager.playerDamage);
        holFlag = false;
    }

    void UpMino(int x,int y)
    {
        if (minoDataTable[y - 1, x] != null)
        {
            var t = GetTreasureData(x, y - 1);
            minoDataTable[y - 1, x].transform.position += Vector3.up;
            if(t != null && minoDataTable[y - 1, x].GetComponent<MinoBlock>().TreimagePos != Vector3.zero)
            {
                t.spriteObj.transform.position = minoDataTable[y - 1, x].transform.position + minoDataTable[y - 1, x].GetComponent<MinoBlock>().TreimagePos;
            }
        }
        minoDataTable[y, x] = minoDataTable[y - 1, x];
        if (minoDataTable[y - 1, x] != null)
        {
            minoDataTable[y - 1, x] = null;
        }
    }
    async Task CreateObstacleBlock()
    {
        int rand = Random.Range(0, 10);
        await Task.Delay(500);
        BoardManager.Instance.UpLine();
        
        for (int i = 0; i < GameManager.boardWidth; i++)
        {
            if (rand != i)
            {
                BoardManager.Instance.SetBoardData(i,1,false);
                minoDataTable[1, i] = Instantiate(obstacleMino, new Vector3(i, 1, 0), Quaternion.identity);
                minoDataTable[1, i].transform.parent = minoListObj.transform;
            }
        }
    }

    private class Treasure
    {
        public GameObject spriteObj;
        public int number;

    }

    private class HoldMinoData
    {
        public int[,] minoData;
        public GameObject minoObj;
        public int index;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using Random = UnityEngine.Random;

public class BoardManager 
{
    static private BoardManager instance;
    public int[,] board; // ボードの状態を管理する配列

    public event Action<int,int> ChangeColor;
    public event Action<int,int,bool,bool> DeleteMino;
    public event Action<int,int> DownMino;
    public event Action<int,int> UpMino;
    public event Action<int,int> TableNullMino;
    public event Func<int,int,bool> CheckTreasure;
    public event Action ResetTable;
    public event Action ClearTable;
    public event Func<Vector2Int, Vector2Int,UniTask> AlignmentMino;
    public event Func<(int, int), List<Vector2Int>> GetTreasurePos;
    public event Action<List<Vector2Int>,List<Vector2Int>,Vector2> MoveTreasurePos;
    public event Action<int,int> CreateObstacleBlock;
    public event Action MinoEffectStart;
    public event Func<MinoType, int, int, bool> CheckBlockType;
    public event Action<int, int> SetAttackBlock;

    public event Action SetTestBlock;
    private List<int> deleteLineRow = new();
    private List<Vector2Int> bombCol = new();
    public bool ObstacleSkillFlag = false;
    private bool stripeFlag = false;
    public static BoardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BoardManager(); // インスタンスがまだ生成されていない場合に生成
            }
            return instance;
        }
    }
    public void DestroyInstance()
    {
        GameManager.Instance.DownMino -= AllDownLineMino;
        GameManager.Instance.FallingMino -= FallingMino;
        instance = null;  // インスタンスを削除（nullにする）
    }

    public void Initialize()
    {
        board = new int[GameManager.Instance.boardHeight, GameManager.Instance.boardWidth];
        GameManager.Instance.DownMino += AllDownLineMino;
        GameManager.Instance.FallingMino += FallingMino;
    }

    public void ResetBoard()
    {
        var newBoard = new int[GameManager.Instance.boardHeight, GameManager.Instance.boardWidth];
        for (int y = 0; y < board.GetLength(0); y++)
        {
            for (int x = 0; x < board.GetLength(1); x++)
            {
                newBoard[y, x] = board[y,x];
            }
        }

        board = newBoard;
        ResetTable?.Invoke();
    }

    public void ClearBoard()
    {
        for (int y = 0; y < board.GetLength(0); y++)
        {
            for (int x = 0; x < board.GetLength(1); x++)
            {
                board[y, x] = 0;
            }
        }
        ClearTable?.Invoke();
    }

    public bool IsValidPosition(Vector3 position)
    {
        var boardX = (int)(position.x);
        var boardY = (int)(position.y);
        if (boardY >= GameManager.Instance.boardHeight)
        {
            return true;
        }
        if (boardX < 0 || boardX >= GameManager.Instance.boardWidth || boardY <= 0)
        {
            return false;
        }

        return board[boardY, boardX] == 0;
        // 衝突なし
    }

    public async UniTask CheckLine(int row)
    {
        deleteLineRow.Clear();
        List<int> obstacleSkillLine = new();
        for (int y = row; y < board.GetLength(0); y++)
        {
            // 一列全部そろっているか確認
            if (CheckIsRowFilledWithOnes(y))
            {
                bool deleteCancelFlag = false;
                for (int x = 0; x < board.GetLength(1); x++)
                {
                    deleteCancelFlag = CheckBlockType.Invoke(MinoType.DeleteCancel,x, y);
                    if (deleteCancelFlag)
                    {
                        DeleteMino?.Invoke(x,y,false,false);
                        board[y, x] = 0;
                        break;
                    }
                }
                if (deleteCancelFlag)
                {
                    continue;
                }
                DeleteLine(y);
                ChangeColor?.Invoke(-1, y);

                if (ObstacleSkillFlag)
                {
                    ObstacleSkillFlag = false;
                    obstacleSkillLine.Add(y);
                    continue;
                }
                deleteLineRow.Add(y);
                GameManager.Instance.DeleteLine++;
                GameManager.Instance.DownFlag = true;
            }
        }
        CheckTreasureData();
        for (int y = 1; y < board.GetLength(0); y++)
        {
            CheckDeleteLine(y);    
        }
        
        // 邪魔ブロックを生成（スキル）
        foreach (var ob in obstacleSkillLine)
        {
            await CreateObstacle(ob);
        }
        
        CheckMaxPutPos();
        SetTestBlock?.Invoke();
    }
    
    async UniTask CreateObstacle(int row)
    {
        List<int> blockX = new List<int>();
        // スキルブロックを検索
        for (int i = 0; i < board.GetLength(1); i++)
        {
            if (board[row, i] != 0)
            {
                blockX.Add(i);
                DeleteMino?.Invoke(i, row,false,false);
            }
        }
        await CreateObstaclesFromMultipleCenters(row, blockX, GameManager.Instance.boardWidth);
    }

 
    async UniTask CreateObstaclesFromMultipleCenters(int row, List<int> centerPoints, int maxColumns)
    {
        List<(int left,int right)> dir = new();
        List<int> num = new ();
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            num.Add(i);
        }
        foreach (var center in centerPoints)
        {
            dir.Add((center,center));
            CreateObstacleBlock?.Invoke(center, row);
            num.Remove(center);
            board[row, center] = 1;
        }

        while (true)
        {
            for (int i = 0; i < dir.Count; i++)
            {
                var valueTuple = dir[i];
                valueTuple.left--;
                if (valueTuple.left > 0 && num.Contains(valueTuple.left))
                {
                    num.Remove(valueTuple.left);
                    if (Random.Range(0, 5) != 0)
                    {
                        CreateObstacleBlock?.Invoke(valueTuple.left, row);
                        board[row, valueTuple.left] = 1;
                    }
                }

                valueTuple.right++;
                if (valueTuple.right < GameManager.Instance.boardWidth && num.Contains(valueTuple.right))
                {
                    num.Remove(valueTuple.right);
                    if (Random.Range(0, 5) != 0)
                    {
                        CreateObstacleBlock?.Invoke(valueTuple.right, row);
                        board[row, valueTuple.right] = 1;
                    }
                }

                dir[i] = valueTuple;
                
            }

            await UniTask.Delay(100);
            if (num.Count == 0)
            {
                break;
            }
        }
    }
    public void CheckMaxPutPos()
    {
        int startX = GameManager.Instance.boardWidth / 2 - 1;
        int startY = GameManager.Instance.boardHeight - 2;
        Func<bool> pinch = () =>
        {
            for (int y = startY - 2; y < GameManager.Instance.boardHeight; y++)
            {
                for (int x = 0; x < GameManager.Instance.boardWidth; x++)
                {
                    if (board[y, x] != 0)
                    {
                        
                        return true;
                    }
                }

            }
            return false;
        };

        if(pinch())
        {
            GameManager.Instance.BackGroundEmmision_Start?.Invoke();
        }
        else
        {
            GameManager.Instance.BackGroundEmmision_Stop?.Invoke();
        }
        

        for (int y = startY; y <= startY + 1; y++)
        {
            for (int x = startX; x <= startX + 1; x++)
            {

                if (board[y, x] != 0)
                {
                    GameManager.Instance.maxPutposFlag = true;
                    return;
                }
            }
        }
        GameManager.Instance.maxPutposFlag = false;
        
    }
    
    public async Task EnemyAttack(int x,int y)
    {
        board[y, x] = 0;
        DeleteMino?.Invoke(x, y,true,false);
        
        await Task.Delay(200);
        //CheckMaxPutPos();
        SetTestBlock?.Invoke();
    }

    public void CheckDeleteLine(int row)
    {
        //deleteLineRow.Clear();
        if (CheckBlankLine(row))
        {
            if (!deleteLineRow.Contains(row))
            {
                deleteLineRow.Add(row);
                DeleteLine(row);
                CheckMaxPutPos();    
            }
        }
        CheckTreasureData();
    }

    private void CheckTreasureData()
    {
        for (int y = 0; y < board.GetLength(0) - 1; y++)
        {
            for (int x = 0; x < board.GetLength(1); x++)
            {
                if(board[y,x] == 2)
                {
                    if ((bool)(CheckTreasure?.Invoke(x, y)))
                    {
                        board[y, x] = 0;
                    }
                }
            }
        }
    }

    public bool CheckBlankLine(int row)
    {
        for (int x = 0; x < GameManager.Instance.boardWidth; x++)
        {
            if (board[row, x] != 0)
            {
                return false;
            }
        }

        return true;
    }
    public bool HitCheck(int x,int y)
    {
        if(board[y, x] != 0)
        {
            return false;
        }
        return true;
    }

    public void SetBoardData(int x,int y,bool isTreasure)
    {
        board[y, x] = isTreasure ? 2 : 1;
        SetTestBlock?.Invoke();
    }

    // 敵定期攻撃用
    public void SetEnemyAttackBlock(int x, int y)
    {
        board[y, x] = -1;
        SetAttackBlock?.Invoke(x,y);
    }

    public void UpLine()
    {
        // 一番上を削除
        for (int x = 0; x < board.GetLength(1); x++)
        {
            board[board.GetLength(0) - 1, x] = 0;
            DeleteMino?.Invoke(x,board.GetLength(0) - 1,false,false);  
        }
        for (int y = board.GetLength(0) - 1; y > 1; y--)
        {
            for (int x = 0; x < board.GetLength(1); x++)
            {
                board[y, x] = board[y - 1, x];
                board[y - 1, x] = 0;
                UpMino?.Invoke(x,y);
            }
        }
        CheckMaxPutPos();
        SetTestBlock?.Invoke();
        
    }

    public void DeleteBomb(int posX,int posY)
    {
        bool skillCheck = false;
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (x + posX > 0 && x + posX < GameManager.Instance.boardWidth)
                {
                    if (y + posY > 0 && y + posY < GameManager.Instance.boardHeight)
                    {
                        skillCheck = CheckBlockType.Invoke(MinoType.SkillCancel, x, y);
                        if (skillCheck) break;
                    }
                }
            }
        }

        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (x + posX > 0 && x + posX < GameManager.Instance.boardWidth)
                {
                    if (y + posY > 0 && y + posY < GameManager.Instance.boardHeight)
                    {
                        bombCol.Add(new Vector2Int(x+posX,y+posY));
                        GameManager.Instance.DeleteMino++;
                        board[y+ posY, x+ posX] = 0;
                        DeleteMino?.Invoke(x+ posX, y + posY,false,skillCheck);
                    }
                }
            }
        }
    }

    public void DeleteStripes(int posX, int posY)
    {
        bool skillCheck = false;
        stripeFlag = true;
        for (int y = 0; y < GameManager.Instance.boardHeight - 1; y++)
        {
            skillCheck = CheckBlockType.Invoke(MinoType.SkillCancel,posX, y);
            if (skillCheck) break;
        }
        for (int y = 0; y < GameManager.Instance.boardHeight - 1; y++)
        {
            board[y, posX] = 0;
            DeleteMino?.Invoke(posX, y,false,skillCheck);
        }

        for (int x = 0; x < GameManager.Instance.boardWidth; x++)
        {
            skillCheck = CheckBlockType.Invoke(MinoType.SkillCancel,x, posY);
            if (skillCheck) break;
        }
        for (int x = 0; x < GameManager.Instance.boardWidth; x++)
        {
            if (board[posY, x] != 0)
            {
                board[posY, x] = 0;
                DeleteMino?.Invoke(x, posY, false,skillCheck);
            }
        }
    }

    public async Task FallingMino()
    {
        List<Vector2Int> fallMino = new();
        bombCol = bombCol
            .GroupBy(val => val.x)               // X座標でグループ化
            .Select(group => group.OrderBy(val => val.y).First())  // 各X座標内でyが最小の要素を選択
            .ToList();                           // リストに変換
        
        foreach (var val in bombCol)
        {
            for (int y = val.y; y < GameManager.Instance.boardHeight; y++)
            {
                if (board[y, val.x] != 0)
                {
                    fallMino.Add(new Vector2Int(val.x,y));    
                }
            }
        }
        bombCol.Clear();

        while (fallMino.Count != 0)
        {
            for (int i = 0; i < fallMino.Count; )
            {
                var val = fallMino[i];
                if (val.y - 1 > 0)
                {
                    if (HitCheck(val.x, val.y - 1))
                    {
                        board[val.y - 1, val.x] = board[val.y, val.x];
                        board[val.y, val.x] = 0;
                        DownMino?.Invoke(val.x,val.y - 1);
                        fallMino[i] = new Vector2Int(val.x, val.y - 1);

                        i++;
                    }
                    else
                    {
                        fallMino.RemoveAt(i);
                    }
                }
                else
                {
                    fallMino.RemoveAt(i);
                }
            }
            await Task.Delay(100);
        }

        await CheckLine(0);
        
    }

    void DeleteLine(int row)
    {
        bool skillCheck = false;
        bool deleteCancelFlag = false;
        for (int i = 0; i < board.GetLength(1); i++)
        {
            skillCheck = CheckBlockType.Invoke(MinoType.SkillCancel,i, row);
            deleteCancelFlag = CheckBlockType.Invoke(MinoType.DeleteCancel,i, row);
            if (deleteCancelFlag)
            {
                DeleteMino?.Invoke(i,row,false,false);
                break;
            }
            if (skillCheck ) break;
        }

        if (!deleteCancelFlag)
        {
            for (int i = 0; i < board.GetLength(1); i++)
            {
                board[row, i] = 0;
                DeleteMino?.Invoke(i,row,false,skillCheck);
            }    
        }
        
        /*
        if(ObstacleSkillFlag)
        {
            CreateObstacle(row);
        }
        */
        SetTestBlock?.Invoke();
    }



    void DownLine(int row)
    {
        for (int y = row; y < board.GetLength(0) - 1; y++)
        {
            for (int x = 0; x < board.GetLength(1); x++)
            {
                board[y, x] = board[y + 1, x];
                board[y + 1, x] = 0;
                DownMino?.Invoke(x,y);
            }
        }

        //DeleteLine(board.GetLength(0)-1);
    }

    void AllDownLineMino()
    {
        deleteLineRow.Sort();
        for (int i = deleteLineRow.Count - 1; i >= 0; i--)
        {
            DownLine(deleteLineRow[i]);
        }
        deleteLineRow.Clear();

        GameManager.Instance.DownFlag = false;
        CheckMaxPutPos();
    }
    
    bool CheckIsRowFilledWithOnes(int row)
    {
        for (int x = 0; x < board.GetLength(1); x++)
        {
            if (board[row, x] == 0)
            {
                return false;
            }
        }

        return true;
    }

    public Vector2Int GetRandomMinoPos()
    {
        List<Vector2Int> postList = new();
        for (int y = 2; y < board.GetLength(0)-2; y++)
        {
            for (int x = 1; x < board.GetLength(1)-2; x++)
            {
                if (board[y, x] != 0)
                {
                    postList.Add(new Vector2Int(x,y));    
                }
            }
        }

        if (postList.Count == 0)
        {
            return new Vector2Int(Random.Range(1, board.GetLength(1) - 2), 2);
        }

        return postList[Random.Range(0, postList.Count)];
    }

    public async UniTask Alignment()
    {
        int[,] newBoard = new int[GameManager.Instance.boardHeight, GameManager.Instance.boardWidth];

        int yCount = 1;
        int xCount = 0;
        List<Vector2Int > trePos = new();
        for (int y = 0; y < board.GetLength(0) ; y++)
        {
            for (int x = 0; x < board.GetLength(1); x++)
            {
                // 宝箱以外を移動させる
                if (board[y, x] != 2 && board[y,x] != 0)
                {
                    newBoard[yCount, xCount] = board[y,x];
                    
                    if(AlignmentMino != null)
                    {
                        await AlignmentMino.Invoke(new Vector2Int(x,y),new Vector2Int(xCount, yCount));
                    }
                    xCount++;
                    if (xCount >= GameManager.Instance.boardWidth - 1)
                    {
                        yCount++;
                        xCount = 0;
                    }
                }
                else if (board[y, x] == 2)
                {
                    trePos.Add(new Vector2Int(x,y));
                }
            }
        }
        // 宝箱の移動
        while (trePos.Count > 0)
        {
            int maxPosX = 0;
            int maxPosY = 0;
            Vector2Int target = trePos[0];
            var pos = GetTreasurePos?.Invoke((target.x, target.y));
            trePos = trePos.Except(pos).ToList();
            Vector2 movePos = Vector2.zero;
            List<Vector2Int> targetPos = new List<Vector2Int>();
            
            // すでに登録されているかチェック
            if (newBoard[yCount, xCount] == 0)          
            {
                foreach (var p in pos)
                {
                    maxPosX = maxPosX < MathF.Abs(p.x - target.x) ? (int)MathF.Abs(p.x - target.x) : maxPosX;
                    maxPosY = maxPosY < MathF.Abs(p.y - target.y) ? (int)MathF.Abs(p.y - target.y) : maxPosY;
                }

                if (maxPosX + 1 + xCount >= GameManager.Instance.boardWidth - 1)
                {
                    yCount++;
                    xCount = 0;
                }

                foreach (var p in pos)
                {
                    Vector2Int absData = new Vector2Int((int)MathF.Abs(p.x - target.x), (int)MathF.Abs(p.y - target.y));
                    newBoard[yCount + absData.y, xCount + absData.x] = board[p.y, p.x];
                    targetPos.Add(new Vector2Int(xCount + absData.x, yCount + absData.y));
                }
                movePos.x = xCount + (maxPosX / 2.0f);
                movePos.y = yCount + (maxPosY / 2.0f);
                MoveTreasurePos.Invoke(pos, targetPos,movePos);

                xCount += maxPosX + 1; // 0を考慮して一個先を見る
                if (xCount > GameManager.Instance.boardWidth - 1)
                {
                    yCount++;
                    xCount = 0;
                }
                if(trePos.Count>0)
                {
                    trePos.RemoveAt(0);
                }
                
            }
            else
            {
                xCount++;
                if (xCount >= GameManager.Instance.boardWidth - 1)
                {
                    yCount++;
                    xCount = 0;
                }
            }
        }
        

        board = newBoard;
    }
}

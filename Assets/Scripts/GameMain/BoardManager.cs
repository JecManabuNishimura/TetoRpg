using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BoardManager 
{
    static private BoardManager instance;
    public int[,] board; // ボードの状態を管理する配列

    public event Action<int,int> ChangeColor;
    public event Action<int,int,bool> DeleteMino;
    public event Action<int,int> DownMino;
    public event Action<int,int> UpMino;
    public event Action<int,int> TableNullMino;
    public event Func<int,int,bool> CheckTreasure;
    public event Action ResetTable;
    public event Action ClearTable;

    public event Action SetTestBlock;
    private List<int> deleteLineRow = new();
    private List<Vector2Int> bombCol = new();
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

    public void Initialize()
    {
        board = new int[GameManager.boardHeight, GameManager.boardWidth];
        GameManager.DownMino += AllDownLineMino;
        GameManager.FallingMino += FallingMino;
    }

    public void ResetBoard()
    {
        var newBoard = new int[GameManager.boardHeight, GameManager.boardWidth];
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
        if (boardY >= GameManager.boardHeight)
        {
            return true;
        }
        if (boardX < 0 || boardX >= GameManager.boardWidth || boardY <= 0)
        {
            return false;
        }

        return board[boardY, boardX] == 0;
        // 衝突なし
    }

    public void CheckLine(int row)
    {
        deleteLineRow.Clear();
        for (int y = row; y < board.GetLength(0); y++)
        {
            // 一列全部そろっているか確認
            if (CheckIsRowFilledWithOnes(y))
            {
                deleteLineRow.Add(y);
                GameManager.DeleteLine++;
                DeleteLine(y);
                ChangeColor?.Invoke(-1, y);
                GameManager.DownFlag = true;
            }
        }
        CheckTreasureData();
        CheckMaxPutPos();
        SetTestBlock?.Invoke();
    }

    public void CheckMaxPutPos()
    {
        if (board[20, 4] != 0 || board[19, 4] != 0 || 
            board[20, 5] != 0 || board[19, 5] != 0 || 
            board[20, 6] != 0 || board[19, 6] != 0)
        {
            GameManager.maxPutposFlag = true;
        }
        else
        {
            GameManager.maxPutposFlag = false;
        }
    }
    
    public async Task EnemyAttack(int x,int y)
    {
        board[y, x] = 0;
        DeleteMino?.Invoke(x, y,true);
        
        await Task.Delay(200);
        CheckMaxPutPos();
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
        for (int x = 0; x < GameManager.boardWidth; x++)
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

    public void UpLine()
    {
        // 一番上を削除
        for (int x = 0; x < board.GetLength(1); x++)
        {
            board[board.GetLength(0) - 1, x] = 0;
            DeleteMino?.Invoke(x,board.GetLength(0) - 1,false);  
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
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (x + posX > 0 && x + posX < GameManager.boardWidth)
                {
                    if (y + posY > 0 && y + posY < GameManager.boardHeight)
                    {
                        bombCol.Add(new Vector2Int(x+posX,y+posY));
                        board[y+ posY, x+ posX] = 0;
                        DeleteMino?.Invoke(x+ posX, y + posY,false);
                    }
                }
            }
        }
    }

    public void DeleteStripes(int posX, int posY)
    {
        for (int y = 0; y < GameManager.boardHeight - 1; y++)
        {
            board[y, posX] = 0;
            DeleteMino?.Invoke(posX, y,false);

        }
        for (int x = 0; x < GameManager.boardWidth; x++)
        {
            board[posY, x] = 0;
            DeleteMino?.Invoke(x, posY,false);

        }

        if (!deleteLineRow.Contains(posY))
        {
            deleteLineRow.Add(posY);
            GameManager.DeleteLine++;    
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
            for (int y = val.y; y < GameManager.boardHeight; y++)
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

        CheckLine(0);
    }

    void DeleteLine(int row)
    {
        for (int i = 0; i < board.GetLength(1); i++)
        {
            DeleteMino?.Invoke(i,row,false);  
            board[row, i] = 0;
        }
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

        GameManager.DownFlag = false;
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
    
}

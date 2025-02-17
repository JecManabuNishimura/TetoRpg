using System.Collections.Generic;
using UnityEngine;

public class AttackData : MonoBehaviour
{
    public static List<Vector2Int> GetAttack(AttackName name)
    {
        return name switch
        {
            AttackName.OneRowLineRandom => OneRowLineRandom(),
            AttackName.OneColLineRandom => OneColLineRandom(),
            AttackName.TwoRowLineRandom => TwoRowLineRandom(),
            AttackName.TwoRowLineConnect => TwoRowLineConnect(),
            AttackName.CrossLineRandom => CrossLineRandom(),
            AttackName.Cross5to5Random => Cross5to5Random(),
        };
    }

    public static List<Vector2Int> GetSpecialAttack(SpecialAttackName name)
    {
        return name switch
        {
            SpecialAttackName.LastAddLine => LastAddLine(),
            SpecialAttackName.Attack4to4 => Attack4To4(),
            SpecialAttackName.NextUpBoost => NextUpBoost(),
            SpecialAttackName.AttackBlockSpawn => AttackBlockSpawn(),
            
        };
    }

    //============================================================
    // 通常攻撃
    //============================================================
    
    //***********************************************
    // 横一列攻撃
    //***********************************************
    private static List<Vector2Int> OneRowLineRandom()
    {
        int y = Random.Range(10, GameManager.Instance.boardHeight -1);
        List<Vector2Int> pos = new();
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y));
        }

        return pos;
    }
    //***********************************************
    // 縦一列攻撃
    //***********************************************
    private static List<Vector2Int> OneColLineRandom()
    {
        int x = Random.Range(0, GameManager.Instance.boardWidth);
        List<Vector2Int> pos = new();
        for (int i = 1; i < GameManager.Instance.boardHeight; i++)
        {
            pos.Add(new Vector2Int(x,i));
        }

        return pos;
    }
    //***********************************************
    // 横２列攻撃　位置ランダム
    //***********************************************
    private static List<Vector2Int> TwoRowLineRandom()
    {
        int y = Random.Range(2, 8);
        int y2 = 0;
        while (true)
        {
            y2 = Random.Range(2, 8);
            if (y2 != y)
            {
                break;
            }
        }
        List<Vector2Int> pos = new();
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y));
        }
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y2));
        }

        return pos;
    }
    //***********************************************
    // 横２列攻撃　位置上下
    //***********************************************
    private static List<Vector2Int> TwoRowLineConnect()
    {
        int y = Random.Range(2, 8);
        int y2 = 0;
        if (y == 2)
        {
            y2 = 3;
        }
        else
        {
            y2 = y - 1;
        }
        List<Vector2Int> pos = new();
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y));
        }
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y2));
        }

        return pos;
    }
    //***********************************************
    //　クロス攻撃 All
    //***********************************************
    private static List<Vector2Int> CrossLineRandom()
    {
        List<Vector2Int> pos = new();
        int y = Random.Range(2, 8);
        int x = Random.Range(0, GameManager.Instance.boardWidth);
        for (int i = 1; i < GameManager.Instance.boardHeight; i++)
        {
            pos.Add(new Vector2Int(x,i));
        }
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            if(!pos.Contains(new Vector2Int(i,y)))
            {
                pos.Add(new Vector2Int(i,y));
            }
        }

        return pos;
    }
    //***********************************************
    //　クロス攻撃 5*5
    //***********************************************
    private static List<Vector2Int> Cross5to5Random()
    {
        List<Vector2Int> pos = new();
        int y = Random.Range(2, 8);
        int x = Random.Range(0, GameManager.Instance.boardWidth);
        for (int i = 0; i < 5; i++)
        {
            if((y - 2)+i < GameManager.Instance.boardHeight && (y - 2)+i > 0)
            {
                pos.Add(new Vector2Int(x,(y - 2)+i));
            }
        }
        for (int i = 0; i < 5; i++)
        {
            if((x - 2)+i < GameManager.Instance.boardWidth && (x - 2) + i >= 0)
            {
                if(!pos.Contains(new Vector2Int((x - 2)+i,y)))
                {
                    pos.Add(new Vector2Int((x - 2)+i,y));
                }
            }
        }

        return pos;
    }

    //============================================================
    // スペシャル攻撃
    //============================================================
    private static List<Vector2Int> LastAddLine()
    {
        GameManager.Instance.LineCreateFlag = true;
        return null;
    }

    //***********************************************
    //　お邪魔ブロックゲージ加速
    //***********************************************
    private static List<Vector2Int> NextUpBoost()
    {
        GameManager.Instance.NextUpCountAmount++;
        return null;
    }

    private static List<Vector2Int> Attack4To4()
    {
        var posX = BoardManager.Instance.GetRandomMinoPos().x;
        var posY = BoardManager.Instance.GetRandomMinoPos().y;
        List<Vector2Int> pos = new();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                pos.Add(new Vector2Int(posX - 1 + x,posY - 1 + y));
            }
        }

        return pos;
    }

    private static List<Vector2Int> AttackBlockSpawn ()
    {
        List<Vector2Int> attackPoint = new List<Vector2Int>();
        for (int y = 3; y < 8; y++) // 縦方向 (y)
        {
            for (int x = 0; x < BoardManager.Instance.board.GetLength(1); x++) // 横方向 (x)
            {
                if (BoardManager.Instance.board[y, x] != 0)
                {
                    attackPoint.Add(new Vector2Int(x, y));
                }
            }
        }

        if (attackPoint.Count != 0)
        {
            List<Vector2Int> selectAttack = new();
            selectAttack.Add(attackPoint[Random.Range(0, attackPoint.Count)]);

            return selectAttack;
        }
        else
        {
            return null;
        }

    }
}

public enum AttackName
{
    OneColLineRandom,
    OneRowLineRandom,
    TwoRowLineRandom,
    TwoRowLineConnect,
    CrossLineRandom,
    Cross5to5Random,
    BombAttack,
    BombMultiAttack,
    
}

public enum SpecialAttackName
{
    LastAddLine,
    Attack4to4,
    NextUpBoost,
    AttackBlockSpawn,
}

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
        };
    }

    public static List<Vector2Int> GetSpecialAttack(SpecialAttackName name)
    {
        return name switch
        {
            SpecialAttackName.LastAddLine => LastAddLine(),
            SpecialAttackName.Attack4to4 => Attack4To4(),
        };
    }

    //============================================================
    // 通常攻撃
    //============================================================
    private static List<Vector2Int> OneRowLineRandom()
    {
        int y = Random.Range(3, 6);
        List<Vector2Int> pos = new();
        for (int i = 0; i < GameManager.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y));
        }

        return pos;
    }
    private static List<Vector2Int> OneColLineRandom()
    {
        int x = Random.Range(0, GameManager.boardWidth);
        List<Vector2Int> pos = new();
        for (int i = 1; i < GameManager.boardHeight; i++)
        {
            pos.Add(new Vector2Int(x,i));
        }

        return pos;
    }

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
        for (int i = 0; i < GameManager.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y));
        }
        for (int i = 0; i < GameManager.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y2));
        }

        return pos;
    }
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
        for (int i = 0; i < GameManager.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y));
        }
        for (int i = 0; i < GameManager.boardWidth; i++)
        {
            pos.Add(new Vector2Int(i,y2));
        }

        return pos;
    }

    //============================================================
    // スペシャル攻撃
    //============================================================
    private static List<Vector2Int> LastAddLine()
    {
        GameManager.LineCreateFlag = true;
        return null;
    }

    private static List<Vector2Int> Attack4To4()
    {
        var posX = Random.Range(1, GameManager.boardWidth - 2);
        var posY = Random.Range(GameManager.boardHeight / 2, GameManager.boardHeight - 2);
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
}

public enum AttackName
{
    OneColLineRandom,
    OneRowLineRandom,
    TwoRowLineRandom,
    TwoRowLineConnect,
    BombAttack,
    BombMultiAttack,
}

public enum SpecialAttackName
{
    LastAddLine,
    Attack4to4,
}

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

    public static void GetSpecialAttack(SpecialAttackName name)
    {
        switch (name)
        {
            case SpecialAttackName.LastAddLine:
                LastAddLine();
                break;
        }
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
    private static void LastAddLine()
    {
        GameManager.LineCreateFlag = true;
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
}

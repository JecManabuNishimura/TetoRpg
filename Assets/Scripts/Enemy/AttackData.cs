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
    
}

public enum SpecialAttackName
{
    LastAddLine,
}

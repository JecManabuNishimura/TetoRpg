using System;
using UnityEngine;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class MinoFactory 
{
    /*
    static private readonly int[,,] tetrominoes = new int[,,]
    {
        // I
        {
            { 0, 0, 0, 0 },
            { 1, 1, -1, 1 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
        },
        // J
        {
            { 1, 0, 0, 0 },
            { 1, -1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
        },
        // L
        {
            { 0, 0, 1, 0 },
            { 1, -1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
        },
        // O
        {
            { 0, 0, 0, 0 },
            { 0, -1, 1, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 },
        },
        // S
        {
            { 0, 0, 0, 0 },
            { 1, 1, 0, 0 },
            { 0, -1, 1, 0 },
            { 0, 0, 0, 0 },
        },
        // T
        {
            { 0, 0, 0, 0 },
            { 1, -1, 1, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 0, 0 },
        },
        // Z
        {
            { 0, 0, 0, 0 },
            { 0, -1, 1, 0 },
            { 1, 1, 0, 0 },
            { 0, 0, 0, 0 },
        },
        // Z
        {
            { 1, 1, 1, 1 },
            { 0, -1, 1, 0 },
            { 1, 1, 0, 0 },
            { 1, 1, 1, 1 },
        },
    };
    */
    // Treasure 宝
    static private readonly int[,,] treasureMinoes = new int[,,]
    {
        // 小さい
        {
            { 0, 0, 0, 0 },
            { 0, 1, 1, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 },
        },
    };
    //static public int TetoMinoLenght => tetrominoes.GetLength(0);  
    static public int[,] GetMinoData(int index,bool isTreasure = false)
    {
        if (isTreasure)
        {
            var minoData = treasureMinoes;
            // 今はパターンが一つしかないので、仮に0にする
            index = 0;
            if (index < 0 || minoData.GetLength(0) <=index)
            {
                return null;
            }

            int[,] tetMino = new int[minoData.GetLength(1), minoData.GetLength(2)];
            for (int y = 0; y < minoData.GetLength(1); y++)
            {
                for (int x = 0; x < minoData.GetLength(2); x++)
                {
                    tetMino[y, x] = minoData[index, y, x];
                }
            }

            return tetMino;
        }
        else
        {
            return MinoData.Entity.GetMinoData(index);
        }
        
    }

    
    static public int[,] RotatePiece(int[,] piece ,int rotNum,int pieceType)
    {
        for (int i = 0; i < rotNum % 4; i++)
        {
            piece = RotateAroundAxis(piece,pieceType);
        }

        return piece;
    }
    static private int[,] RotateAroundAxis(int[,] matrix,int pieceType)
    {
        int n = matrix.GetLength(0);
        int m = matrix.GetLength(1);
        int axisX = -1, axisY = -1;
        int[,] rotated = new int[n, m]; // 新しい配列を初期化
        
        // 軸の位置を探す
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (matrix[i, j] <= -1)
                {
                    axisX = j;
                    axisY = i;
                    break;
                }
            }
            if (axisX != -1) break; // 軸が見つかったらループを抜ける
        }

        // 軸が見つからなかった場合は元の配列を返す
        if (axisX == -1)
        {
            // ここで真ん中回転を実施
            int l = matrix.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    rotated[i, j] = matrix[j, l - 1 - i];
                }
            }
            return rotated;
        }
        
        // 新しい位置を計算
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                // 軸の位置を保持
                if (i == axisY && j == axisX)
                {
                    rotated[i, j] = matrix[i, j];
                }
                else
                {
                    int newX = axisX + (i - axisY);
                    int newY = axisY - (j - axisX);

                    // 配列の範囲内であれば値を設定
                    if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                    {
                        rotated[newY, newX] = matrix[i, j];
                    }
                }
            }
        }

        return rotated;
    }
}

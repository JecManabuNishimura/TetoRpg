using System;
using System.Collections.Generic;
using System.Linq;
using MyMethods;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class MapEncountEnemy : MonoBehaviour
{
    [SerializeField] private GameObject enemyListParent;
    [SerializeField] private SplineContainer splineContainer; // Dolly Spline のパス
    [SerializeField] private List<GameObject> EnemyList;
    
    void OnValidate()
    {
        if (enemyListParent != null && splineContainer != null)
        {
            EnemyList.Clear();

            // 子オブジェクトの Transform をリスト化し、それをソート
            var sortedEnemies = enemyListParent.transform
                .Cast<Transform>() // 子要素を Transform 型として列挙
                .OrderBy(child => GetNormalizedPositionOnSpline(child.position)) // Spline 上の進行度でソート
                .ToList();

            // ソートされた順に List に追加
            foreach (var enemy in sortedEnemies)
            {
                EnemyList.Add(enemy.gameObject);
            }
        }
    }
// 指定したワールド座標が Spline 上のどの位置かを取得（0.0～1.0 の範囲）
    private float GetNormalizedPositionOnSpline(Vector3 position)
    {
        var spline = splineContainer.Spline;

        float closestT = 0f;
        float closestDistance = float.MaxValue;

        // Spline を細かくサンプリングして最近接位置を探す
        for (float t = 0; t <= 1f; t += 0.01f) // 分解能を調整可能
        {
            Vector3 splinePosition = spline.EvaluatePosition(t);
            float distance = Vector3.Distance(position, splinePosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestT = t;
            }
        }

        return closestT;
    }
    
    
    public GameObject GetEnemyData()
    {
        if(EnemyList.Count != 0)
        {
            return EnemyList[0];
        }

        return null;
    }

    public void DeleteEnemy()
    {
        if(EnemyList.Count != 0)
        {
            EnemyList.RemoveAt(0);
        }
    }
}

using System;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class CameraMove : MonoBehaviour
{
    [SerializeField] public CinemachineSplineDolly dollyCart;
    [SerializeField] private SplineContainer spline;
    
    private bool moveFlag = true;

    public void Awake()
    {
        GameManager.cameraMove = this;
        dollyCart.CameraPosition = 0f;
    }

    public async void MoveCamera()
    {
        while (true)
        {
            if (!GameManager.cameraFlag)
            {
                await Task.Yield();
                
                continue;
            }
            dollyCart.CameraPosition += 0.1f;
            if (CheckEnemyEncount(MapManager.Instance.GetEnemyPos))
            {
                GameManager.cameraFlag = false;
                // バトル開始
                GameManager.Battle();
                continue;
            }

            if (spline.CalculateLength() <= dollyCart.CameraPosition)
            {
                break;
            }
            await Task.Yield(); // 1フレーム待機
        }
    }

    private bool CheckEnemyEncount(Vector3 pos)
    {
        var nowPos = dollyCart.CameraPosition / spline.CalculateLength();
        return Vector3.Distance(spline.EvaluatePosition(nowPos), pos) <= 3f;
    }
}

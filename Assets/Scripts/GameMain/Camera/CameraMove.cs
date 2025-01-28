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
        GameManager.Instance.cameraMove = this;
        dollyCart.CameraPosition = 0f;
    }

    public async void MoveCamera()
    {
        while (true)
        {
            if (!GameManager.Instance.cameraFlag)
            {
                await Task.Yield();

                break;
            }
            dollyCart.CameraPosition += 0.1f;
            if (CheckEnemyEncount(MapManager.Instance.GetEnemyPos))
            {
                GameManager.Instance.cameraFlag = false;
                // バトル開始
                GameManager.Instance.Battle();
                break;
            }

            if (spline.CalculateLength() <= dollyCart.CameraPosition)
            {
                Debug.Log("syuuryou");
                break;
            }
            await Task.Yield(); // 1フレーム待機
        }
    }

    private bool CheckEnemyEncount(Vector3 pos)
    {
        if(spline == null)
        {
            Debug.LogWarning("SplineContainerが破棄されています。");
        }
        var nowPos = dollyCart.CameraPosition / spline.CalculateLength();
        return Vector3.Distance(spline.EvaluatePosition(nowPos), pos) <= 3f;
    }

    private void OnDestroy()
    {
        Destroy(spline);
    }
}

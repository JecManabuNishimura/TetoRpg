using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleCircleLayoutGroup : UIBehaviour, ILayoutGroup 
{
    public float radius = 100;
    public float offsetAngle;
    public float angle;
    public Transform stopPos;
    public float rotationSpeed = 100f;   // 回転速度
    private bool isRotating = false;     // 回転中フラグ
    private int currentIndex = 0;        // 現在の項目インデックス
    public GameObject selectedItem;
#if  UNITY_EDITOR
    protected override void OnValidate ()
    {
        base.OnValidate ();
        Arrange ();
    }
#endif
    // 要素数が変わると自動的に呼ばれるコールバック
    #region ILayoutController implementation
    public void SetLayoutHorizontal (){}
    public void SetLayoutVertical ()
    {
        Arrange ();
    }
    #endregion

    public async void AddAngle(bool right)
    {
        if (isRotating) return; // 回転中は新しい入力を無視
        isRotating = true;

        int itemCount = transform.childCount;
        if (itemCount == 0)
        {
            isRotating = false;
            return;
        }

        // 次のインデックスを計算
        currentIndex = (currentIndex + (right ? -1 : 1) + itemCount) % itemCount;

        // 次の目標角度を計算
        float splitAngle = 360f / itemCount;
        float targetAngle = -splitAngle * currentIndex;

        // 現在の角度から目標角度へスムーズに回転
        while (!Mathf.Approximately(angle, targetAngle))
        {
            angle = Mathf.MoveTowardsAngle(angle, targetAngle, rotationSpeed * Time.deltaTime);
            Arrange(); // 配置を再計算
            await Task.Yield();
        }
        selectedItem = transform.GetChild(currentIndex).gameObject;

        isRotating = false;
    }

    public int GetIndex()
    {
        return currentIndex;
    }

    public Transform GetSelectObj()
    {
        return transform.GetChild(currentIndex).transform;
    }

    // 子オブジェクトの配置を更新
    void Arrange()
    {
        if (transform.childCount == 0) return; // 子要素がいない場合は何もしない

        float splitAngle = 360f / transform.childCount; // 各要素の角度間隔
        float startAngle = 90f;
        for (int elementId = 0; elementId < transform.childCount; elementId++)
        {
            var child = transform.GetChild(elementId) as RectTransform;

            // 各要素の配置計算
            float currentAngle = splitAngle * elementId + offsetAngle + angle + startAngle; // 現在の角度を維持
            child.anchoredPosition = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * radius;
        }
    }
}

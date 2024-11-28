using System;
using UnityEngine;

namespace Enemy
{
    public class Bullet : MonoBehaviour
    {
        public GameObject target;
        [SerializeField] private float speed = 6.0f;
        [SerializeField] private float rotSpeed = 180.0f;
        private bool moveFlag = false;

        public void Update()
        {
            if (moveFlag)
            {
                Move();
            }
        }

        public void StartMove()
        {
            moveFlag = true;
        }

        private void Move()
        {
            if (target == null)
            {
                return;
            }

            // ターゲットまでの方向を計算 (2D: X, Y平面での方向)
            Vector3 vecTarget = target.transform.position - transform.position;
            vecTarget.z = 0; // Z軸の差を無視して、X, Y平面でターゲット方向を計算

            // 自オブジェクトの正面方向を計算 (2D: X, Y平面での方向)
            Vector3 vecForward = transform.up; // 2D空間では上方向（Y軸）を正面とする
            vecForward.z = 0; // Z軸の差を無視して、X, Y平面で正面方向を計算

            // ターゲットと現在位置からの角度を計算
            float angleDiff = Vector3.Angle(vecForward, vecTarget);  // 角度の差
            float angleAdd = rotSpeed * Time.deltaTime;  // 回転角度

            // 回転角がangleAdd以下ならターゲット方向に完全に向く
            if (angleDiff <= angleAdd)
            {
                // ターゲット方向を向ける
                transform.up = vecTarget.normalized;  // 2D平面でターゲットを向く
            }
            else
            {
                // 回転角の範囲内で補完
                float t = angleAdd / angleDiff;
                Vector3 newDirection = Vector3.Lerp(transform.up, vecTarget.normalized, t);  // 徐々にターゲット方向に向ける
                transform.up = newDirection;  // 正面をターゲット方向に補完
            }

            // 前進 (Y軸方向に進む)
            transform.position += transform.up * speed * Time.deltaTime;

            if (Vector3.Distance(target.transform.position, transform.position) < 3f)
            {
                Destroy(gameObject);
            }
        }
    }
}

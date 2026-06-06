//-------------------------------------------------------
//
//  CameraFollow.cs
//
//  概要
/// 指定したターゲットにカメラを追従させる2D用スクリプト。
//
//  更新履歴
//
//  2026/06/04  作成
//
//-------------------------------------------------------
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("追従ターゲット")]
    [Tooltip("追従させたいオブジェクト")]
    public Transform target;

    [Header("追従設定")]
    [Tooltip("追従の滑らかさ（0に近いほど即時、1に近いほど遅延が大きい）")]
    [Range(0f, 1f)]
    public float smoothing = 0.1f;

    [Tooltip("ターゲットからのオフセット（位置調整）")]
    public Vector2 offset = Vector2.zero;

    [Header("カメラ範囲制限（任意）")]
    [Tooltip("カメラ範囲制限を有効にする")]
    public bool useBounds = false;

    [Tooltip("カメラが移動できるX軸の最小・最大値")]
    public float minX = -10f;
    public float maxX = 10f;

    [Tooltip("カメラが移動できるY軸の最小・最大値")]
    public float minY = -10f;
    public float maxY = 10f;

    private Vector3 _velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

    //    目標位置を計算（Z軸は現在のカメラのZ値を維持）
        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

     //   範囲制限を適用
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

    //    SmoothDampで滑らかに追従
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            smoothing
        );
    }

    // <summary>
    // カメラをターゲット位置に即時ワープさせる（シーン開始時などに使用）
    // </summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

    //    エディタ上で範囲制限を可視化
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}

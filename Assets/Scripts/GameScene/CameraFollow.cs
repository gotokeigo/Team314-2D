//-------------------------------------------------------
//
//  CameraFollow.cs
//
//  概要
/// 指定したターゲットにカメラを追従させるスクリプト。
//
//  更新履歴
//
//  2026/06/04  作成
//  2026/07/06  クォータービュー対応。オフセットをVector3に変更。
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
    public Vector3 offset = Vector3.zero;   // Vector2 → Vector3に変更
    [Header("カメラ範囲制限（任意）")]
    [Tooltip("カメラ範囲制限を有効にする")]
    public bool useBounds = false;
    [Tooltip("カメラが移動できるX軸の最小・最大値")]
    public float minX = -10f;
    public float maxX = 10f;
    [Tooltip("カメラが移動できるZ軸の最小・最大値")]
    public float minZ = -10f;
    public float maxZ = 10f;
    private Vector3 _velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            target.position.z + offset.z
        );

        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);  // YではなくZ
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            smoothing
        );
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            target.position.z + offset.z
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minZ + maxZ) / 2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxZ - minZ, 0f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}

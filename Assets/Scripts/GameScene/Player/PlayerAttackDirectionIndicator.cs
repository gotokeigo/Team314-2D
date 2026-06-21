//--------------------------------------
//
//  AttackDirectionIndicator.cs
//
//  概要
//  プレイヤーの攻撃方向を矢印で表示するスクリプト
//
//  更新履歴
//
//  2026/06/21  作成  仮実装（後にカーソル方向に変更予定）
//
//--------------------------------------
using UnityEngine;

public class AttackDirectionIndicator : MonoBehaviour
{
    [Tooltip("矢印の長さ")]
    [SerializeField] private float arrowLength = 1.5f;
    [Tooltip("矢印の太さ")]
    [SerializeField] private float arrowWidth = 0.05f;
    [Tooltip("矢印の色")]
    [SerializeField] private Color arrowColor = Color.white;

    private LineRenderer _lineRenderer;
    private PlayerAttack _playerAttack;
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
        _playerAttack = GetComponentInParent<PlayerAttack>();

        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = arrowWidth;
        _lineRenderer.endWidth = arrowWidth * 0.1f;     // 先端を細くして矢印っぽくする
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = arrowColor;
        _lineRenderer.endColor = arrowColor;
        _lineRenderer.sortingOrder = 10;                // 前面に表示
    }

    private void Update()
    {
        if (_playerController == null) return;

        Vector2 dir = _playerController.LastMoveDirection;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(dir * arrowLength);

        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
    }
}

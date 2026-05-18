//--------------------------------------
//
//  PlayerController.cs
//
//  概要
//  プレイヤーの挙動を制御するスクリプト
//
//  更新履歴
//
//  2026/04/27  作成
//              InputSystemを使いプレイヤーをWASDで移動できるように
//
//  2026/04/28  攻撃を最後に移動した方向に出すために最後に移動した方向を取得する用にした
//
//--------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private float _speedMultiplier = 1.0f; //プレイヤー移動速度倍率

    // 最後に移動した方向（初期値は下向き）
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();

        // 移動入力があった時だけ方向を更新
        if (_moveInput != Vector2.zero)
        {
            LastMoveDirection = _moveInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = _rb.position + _moveInput * moveSpeed * _speedMultiplier * Time.fixedDeltaTime;   //
        _rb.MovePosition(newPosition);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }
}

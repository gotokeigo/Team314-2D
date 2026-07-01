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
    [Tooltip("プレイヤーの移動速度")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform modelTransform;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private float _speedMultiplier = 1.0f; //プレイヤー移動速度倍率
    //private Animator _animator;
   [SerializeField] private Animator _animator;

    // 最後に移動した方向（初期値は下向き）
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
       //  _animator = GetComponentInChildren<Animator>();
        Animator[] animators = GetComponentsInChildren<Animator>();

        Debug.Log("Animatorの数 = " + animators.Length);

        foreach (Animator a in animators)
        {
            Debug.Log(a.gameObject.name);
        }
    }
    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        // 移動入力があった時だけ方向を更新
        if (_moveInput != Vector2.zero)
        {
            LastMoveDirection = _moveInput.normalized;
            _animator.SetBool("isMoving", true);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
            Debug.Log("ゲーム終了");
        }
    }
    private void FixedUpdate()
    {
        Vector2 newPosition = _rb.position + _moveInput * moveSpeed * _speedMultiplier * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);

        if (_moveInput != Vector2.zero)
        {
            SetLookDirection(_moveInput);
        }
        // 攻撃溜め中ならWalk、それ以外はRun
        _animator.SetBool("isWalk", _speedMultiplier < 1.0f);
    }
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }
    public void SetLookDirection(Vector2 dir)
    {
        if (dir.x > 0)
        {
            modelTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (dir.x < 0)
        {
            modelTransform.localScale = new Vector3(-1, 1, 1);
        }
    }
    
}

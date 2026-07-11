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
//  2026/07/06  3D対応。Rigidbody2D→Rigidbody、Vector2→Vector3に変更。
//
//--------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Tooltip("プレイヤーの移動速度")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform modelTransform;
    private Rigidbody _rb;
    private Vector3 _moveInput;                                    
    private float _speedMultiplier = 1.0f;
    [SerializeField] private Animator _animator;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.back; 

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        Animator[] animators = GetComponentsInChildren<Animator>();
        Debug.Log("Animatorの数 = " + animators.Length);
        foreach (Animator a in animators)
        {
            Debug.Log(a.gameObject.name);
        }
    }

    private void OnMove(InputValue value)
    {
        Vector3 input = value.Get<Vector3>();                       
        _moveInput = new Vector3(input.x, 0f, input.y);            // XZ平面に変換

        if (_moveInput != Vector3.zero)
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
        _rb.linearVelocity = new Vector3(
            _moveInput.x * moveSpeed * _speedMultiplier,
            _rb.linearVelocity.y,
            _moveInput.z * moveSpeed * _speedMultiplier
        );

        if (_moveInput != Vector3.zero)
        {
            SetLookDirection(_moveInput);
        }

        _animator.SetBool("isWalk", _speedMultiplier < 1.0f);
    }
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    public void SetCharging(bool charging)
    {
        _animator.SetBool("isCharging", charging);
    }

    public void SetLookDirection(Vector3 dir)
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
    //public void SetLookDirection(Vector3 dir)
    //{
    //    dir.y = 0f;

    //    if (dir.sqrMagnitude > 0.001f)
    //    {
    //        modelTransform.rotation = Quaternion.LookRotation(dir);
    //    }
    //}
    public void PlayHitAnimation()
    {
        _animator.SetTrigger("isHit");
    }
    public void PlayAttackAnimation()
    {
        _animator.SetTrigger("isAttack");
    }
}

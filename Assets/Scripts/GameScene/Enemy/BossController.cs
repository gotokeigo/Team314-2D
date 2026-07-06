//--------------------------------------
//
//  BossController.cs
//
//  概要
//  ボスの挙動を制御するスクリプト
//
//  更新履歴
//
//  2026/06/11  作成
//              プロトタイプ。通常敵よりHP・移動速度が高いだけで固有行動なし。
//
//  2026/07/06  3D対応。
//
//--------------------------------------
using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("移動速度")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("接触時のダメージ")]
    [SerializeField] private int attackDamage = 2;
    [Tooltip("最大HP")]
    [SerializeField] private float maxHp = 100f;
    [Tooltip("吹き飛ぶ時の回転力")]
    [SerializeField] private float rotationForce = 5f;
    [Tooltip("このレベル以上のプレイヤーはボスをワンパンできる")]
    [SerializeField] private int oneHitKillPlayerLevel = 10;

    public float MaxHp => maxHp;

    private float _currentHp;
    private bool _isDead;
    private bool _isKnockedBack;
    private bool _isFalling;

    private Transform _player;
    private Rigidbody _rb;
    private PlayerHealth _playerHp;
    private float _stopDistance;

    private int _defaultLayer;
    private int _knockbackLayer;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentHp = maxHp;

        Collider bossCol = GetComponent<Collider>();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
            _playerHp = playerObject.GetComponent<PlayerHealth>();
            Collider playerCol = playerObject.GetComponent<Collider>();
            if (bossCol != null && playerCol != null)
            {
                _stopDistance = bossCol.bounds.extents.x + playerCol.bounds.extents.x;
            }
            else
            {
                _stopDistance = 1f;
            }
        }
        else
        {
            Debug.LogError("Playerタグが見つかりません！");
        }

        _defaultLayer = gameObject.layer;
        _knockbackLayer = LayerMask.NameToLayer("EnemyKnockback");
        Physics.IgnoreLayerCollision(_knockbackLayer, _defaultLayer, true);
        Physics.IgnoreLayerCollision(_knockbackLayer, _knockbackLayer, true);
    }

    private void FixedUpdate()
    {
        if (_player == null || _rb == null) return;
        if (_isFalling) return;
        if (_playerHp != null && _playerHp.IsDead)
        {
            _rb.linearVelocity = Vector3.zero;
            return;
        }
        if (_isKnockedBack) return;

        Chase();
    }

    private void Chase()
    {
        Vector3 direction = _player.position - transform.position;
        float distance = direction.magnitude;
        if (distance <= _stopDistance)
        {
            _rb.MovePosition(_rb.position);
            return;
        }
        Vector3 newPosition = _rb.position + direction.normalized * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        PlayerHealth playerHp = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHp != null)
        {
            playerHp.TakeDamage(attackDamage);
        }
    }

    public bool TakeDamage(float damage)
    {
        if (_isDead) return false;

        int playerLevel = ExperienceManager.Instance.PlayerLevel;
        if (playerLevel >= oneHitKillPlayerLevel)
        {
            damage = float.MaxValue;
        }

        _currentHp -= damage;
        Debug.Log($"ボスHP: {_currentHp}/{maxHp}");
        if (_currentHp <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public void SmallKnockBack(Vector3 direction, float force)
    {
        if (_isDead) return;
        StartCoroutine(SmallKnockBackCoroutine(direction, force));
    }

    private IEnumerator SmallKnockBackCoroutine(Vector3 direction, float force)
    {
        _isKnockedBack = true;
        _rb.AddForce(direction * force, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        _isKnockedBack = false;
    }

    private void Die()
    {
        _isDead = true;

        GameTimer gameTimer = FindFirstObjectByType<GameTimer>();
        if (gameTimer != null)
        {
            gameTimer.OnBossDefeated();
        }
    }

    public void KnockBack(Vector3 direction, float force)
    {
        _isKnockedBack = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.freezeRotation = false;
        _rb.AddForce(direction * force, ForceMode.Impulse);
        _rb.AddTorque(Vector3.up * rotationForce, ForceMode.Impulse);
        StartCoroutine(KnockBackCoroutine());
    }

    private IEnumerator KnockBackCoroutine()
    {
        gameObject.layer = _knockbackLayer;

        yield return new WaitForSeconds(0.3f);

        if (!_isDead)
        {
            _isKnockedBack = false;
            _rb.freezeRotation = true;
            _rb.rotation = Quaternion.identity;
            gameObject.layer = _defaultLayer;
        }
    }

    private void OnBecameInvisible()
    {
        if (_isKnockedBack || _isDead)
        {
            Destroy(gameObject);
        }
    }

    public void FallOff()
    {
        if (_isFalling) return;
        if (!_isDead && !_isKnockedBack) return;
        _isFalling = true;
        _isDead = true;
        _isKnockedBack = false;
        StopAllCoroutines();

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        _rb.freezeRotation = false;

        StartCoroutine(FallAndDestroy());
    }

    private IEnumerator FallAndDestroy()
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = t * t;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            transform.position = new Vector3(
                startPos.x,
                startPos.y - eased * 3f,
                startPos.z
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}

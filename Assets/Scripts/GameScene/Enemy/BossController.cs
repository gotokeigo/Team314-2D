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
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // ★追加
public class BossController : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("死んだときのSE")] // ★追加
    [SerializeField] private AudioClip deathSound; // ★追加
    private AudioSource _audioSource;
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

    private NavMeshAgent _agent; // ★追加

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>(); // ★追加
        _audioSource = GetComponent<AudioSource>();
        _currentHp = maxHp;

        float currentNormalEnemyHp = PlayerPrefs.GetFloat("NextEnemyHP", 10f); // ザコ敵のHPを取得
        maxHp = currentNormalEnemyHp * 2f;                                   // 2倍にする
        _currentHp = maxHp;                                                  // 現在のHPを満タンに

        Debug.Log($"【ボス出現】ザコHPの2倍の体力「{maxHp}」に設定されました！");

        // ★追加：AIの速度と回転速度を設定
        if (_agent != null)
        {
            _agent.speed = moveSpeed;
            _agent.angularSpeed = 120f;
        }

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

            // ★追加：AIの停止距離をセット
            if (_agent != null) _agent.stoppingDistance = _stopDistance;
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
        if (_player == null || _agent == null) return; // ★ _rb から _agent に変更
        if (_isFalling) return;
        if (_playerHp != null && _playerHp.IsDead)
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true; // ★追加：追跡を停止
            return;
        }
        if (_isKnockedBack) return;

        Chase();
    }

    private void Chase()
    {
        if (_agent.isOnNavMesh)
        {
            _agent.SetDestination(_player.position);
        }
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

        //int playerLevel = ExperienceManager.Instance.PlayerLevel;
        //if (playerLevel >= oneHitKillPlayerLevel)
        //{
        //    damage = float.MaxValue;
        //}

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
        // ★追加：ノックバック開始時にAIを一時停止
        if (_agent.isOnNavMesh) _agent.isStopped = true;
        _rb.AddForce(direction * force, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        _isKnockedBack = false;
        // ★追加：生きていればAIを再開
        if (_agent.isOnNavMesh && !_isDead) _agent.isStopped = false;
    }

    private void Die()
    {
        _isDead = true;

        // ★追加：AudioSourceを使ってSEを鳴らす
        if (_audioSource != null && deathSound != null)
        {
            _audioSource.PlayOneShot(deathSound);
        }

        // ★追加：死亡時にAIを停止
        if (_agent.isOnNavMesh) _agent.isStopped = true;

        // 【修正】自分自身ではなく、子要素（boss_idou_motion）からAnimatorを取得する
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        GameTimer gameTimer = FindFirstObjectByType<GameTimer>();
        if (gameTimer != null)
        {
            gameTimer.OnBossDefeated();
        }
        Destroy(gameObject, 1.5f);
    }

    public void KnockBack(Vector3 direction, float force)
    {
        _isKnockedBack = true;
        // ★追加：大きなノックバック開始時にもAIを一時停止
        if (_agent.isOnNavMesh) _agent.isStopped = true;
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

            // ★追加：生きて復帰したらAIを再開
            if (_agent.isOnNavMesh) _agent.isStopped = false;
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

        // ★追加：落下が始まったらAIの機能自体を完全にOFFにする
        if (_agent != null) _agent.enabled = false;

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

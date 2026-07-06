//--------------------------------------
//
//  EnemyController.cs
//
//  概要
//  敵の挙動を制御するスクリプト
//
//  更新履歴
//
//  2026/04/27  作成
//              敵をプレイヤーのタグを持っているオブジェクトに向かわせるように。
//              敵がプレイヤーに当たった時にプレイヤーにダメージを与えるようにした。
//
//  2026/04/28  敵がプレイヤーの攻撃に当たった時に吹き飛び、画面外に行ったときに消滅するようになった
//                  敵の吹っ飛ばす速度はPlayerAttack.csのknockBackForceで変更
//
//  2026/05/04  ノックバック中に他の敵と衝突しないようレイヤーを切り替えるように
//
//  2026/05/10  敵にHPとレベルを追加
//
//  2026/05/18  プレイヤーの攻撃が当たってHPが0になったときに画面端に行くまで吹き飛び続けるように変更。
//              ExperienceManagerにヒットした数を渡していたので倒した数を渡すように変更。
//              敵を倒したときに回転しながら飛んでいくのを追加
//
//  2026/05/20  敵がその辺をふらつくように、またプレイヤータグを持っているのを追いかけていたのを
//              プレイヤータグを持っているのが近づいてきたときに、追いかけ始めるように変更
//
//--------------------------------------
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.Processors;

public class EnemyController : MonoBehaviour
{
    [Header("敵のステータス設定")]
    [Tooltip("敵の移動速度")]
    [SerializeField] private float moveSpeed;       //  移動速度
    [Tooltip("プレイヤーに接触したときにプレイヤーに与えるダメージ")]
    [SerializeField] private int attackDamage;      //  接触時のダメージ
    [Tooltip("敵の攻撃インターバル")]
    [SerializeField] private float attackInterval;  //  攻撃間隔
    [Tooltip("敵の最大HP")]
    [SerializeField] private float maxHp;           //  敵の最大HP
    [Tooltip("敵の最大レベル")]
    [SerializeField] private int level;             //  敵のレベル
    [Tooltip("敵の吹き飛ばしたときの回転力")]
    [SerializeField] private float rotationForce = 5f;     // 敵が吹き飛ぶ時の回転力

    [Header("敵のさまよう・索敵関係")]
    [Tooltip("プレイヤーを発見する距離")]
    [SerializeField] private float detectionRange = 5f;     // プレイヤーを発見する距離
    [Tooltip("敵が初期沸き位置からさまよう範囲")]
    [SerializeField] private float wanderRadius = 3f;       // さまよう範囲
    [Tooltip("次の目標地点を決める間隔")]
    [SerializeField] private float wanderInterval = 2f;     // 次の目標地点を決める間隔

    private float _attackTimer;
    private bool _isKnockedBack; // 吹き飛んでいるか
    private float _currentHp;    // 敵の現在HP
    private Transform _player;
    private Rigidbody _rb;
    private PlayerHealth _playerHp;
    private float _stopDistance;
    private bool _isDead;
    private bool _isFalling;
    public float MaxHp => maxHp;    // 外部からmaxHpを参照用

    //  敵集団管理用
    private bool _isDiscovered;                             // プレイヤーを発見しているか
    private Vector3 _wanderTarget;                          // さまよう目標地点
    private float _wanderTimer;                             // さまようタイマー
    private float _currentMoveSpeed;                        // 現在の移動速度（グループから設定される)


    private int _defaultLayer;
    private int _knockbackLayer;

    public bool IsFalling => _isFalling;    //外部から落下中か参照用

    public int Level => level;                      // Experienceからレベルを参照用
    public float GetBaseSpeed() => moveSpeed;   // 基本速度を返す

    // ★ ここを追加！グループからHPを読み取れるようにする窓口
    public float GetMaxHP() => maxHp;
    public float GetCurrentHP() => _currentHp;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentHp = maxHp;                          // 敵の最大HP

        Collider enemyCol = GetComponent<Collider>();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
            _playerHp = playerObject.GetComponent<PlayerHealth>();
            Collider playerCol = playerObject.GetComponent<Collider>();
            _stopDistance = enemyCol.bounds.extents.x + playerCol.bounds.extents.x;
        }


        _defaultLayer = gameObject.layer;
        _knockbackLayer = LayerMask.NameToLayer("EnemyKnockback");
        Physics.IgnoreLayerCollision(_knockbackLayer, _defaultLayer, true);
        Physics.IgnoreLayerCollision(_knockbackLayer, _knockbackLayer, true);

        _currentMoveSpeed = moveSpeed;
        _wanderTarget = GetNewWanderTarget();
    }

    void FixedUpdate()
    {
        if (_player == null || _rb == null) return;
        if (_isFalling) return;
        if (_playerHp != null && _playerHp.IsDead)
        {
            _rb.linearVelocity = Vector3.zero;
            return;
        }
        if (_isKnockedBack) return;

        // 未発見なら自動発見チェック
        if (!_isDiscovered)
        {
            float distToPlayer = Vector3.Distance(_rb.position, _player.position);
            if (distToPlayer <= detectionRange)
            {
                SetDiscovered(true);
            }
        }

        if (_isDiscovered)
        {
            Chase();
        }
        else
        {
            Wander();
        }
    }

    // 追跡処理（既存のFixedUpdateの移動処理を移動）
    private void Chase()
    {
        Vector3 direction = (Vector3)(_player.position - transform.position);
        float distance = direction.magnitude;
        if (distance <= _stopDistance)
        {
            _rb.MovePosition(_rb.position);
            return;
        }
        Vector3 newPosition = _rb.position + direction.normalized * _currentMoveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    // さまよう処理
    private void Wander()
    {
        Vector3 direction = (_wanderTarget - _rb.position);
        if (direction.magnitude <= 0.1f) return;    // 目標地点に着いたら止まって待つ

        Vector3 newPosition = _rb.position + direction.normalized * _currentMoveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    // さまよう目標地点をランダムに決める
    private Vector3 GetNewWanderTarget()
    {
        _wanderTimer = wanderInterval;
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        return _rb.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }

    // グループから発見状態を設定する
    public void SetDiscovered(bool discovered)
    {
        // 変化がないときは通知しない
        if (_isDiscovered == discovered) return;

        _isDiscovered = discovered;

        if (discovered)
        {
            EnemyManager.Instance?.NotifyDiscovered();
        }
        else
        {
            EnemyManager.Instance?.NotifyLost();
        }
    }

    // グループから速度を設定する
    public void SetMoveSpeed(float speed)
    {
        _currentMoveSpeed = speed;
    }

    public bool IsDiscovered => _isDiscovered;  // グループから参照用

    private void OnCollisionStay(Collision collision)
    {

        if (_isDead) return;

        if (!collision.gameObject.CompareTag("Player")) return;

        //PlayerHealth colPlayerHp = collision.gameObject.GetComponent<PlayerHealth>();
        PlayerHealth colPlayerHp = collision.gameObject.GetComponentInParent<PlayerHealth>();
        if (colPlayerHp != null)
        {
            colPlayerHp.TakeDamage(attackDamage);
        }
    }

    public bool TakeDamage(float damage)
    {
        if (_isDead) return false;
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            Die();
            return true;    // 死亡
        }
        return false;       // 生存
    }

    private void Die()
    {
        _isDead = true;

        // 発見状態だった場合は通知
        if (_isDiscovered)
        {
            _isDiscovered = false;
            EnemyManager.Instance?.NotifyLost();
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

    public void SmallKnockBack(Vector3 direction, float force)
    {
        if (_isDead) return;
        StartCoroutine(SmallKnockBackCoroutine(direction, force));
    }

    private IEnumerator SmallKnockBackCoroutine(Vector3 direction, float force)
    {
        _isKnockedBack = true;
        _rb.AddForce(direction * force, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);  // 追跡を止める時間（Inspectorで調整できないので短めに固定）
        _isKnockedBack = false;
    }

    private IEnumerator KnockBackCoroutine()
    {
        gameObject.layer = _knockbackLayer;

        yield return new WaitForSeconds(0.3f);

        // 死亡していたら吹き飛び状態のまま画面外まで飛ばし続ける
        if (!_isDead)
        {
            _isKnockedBack = false;
            _rb.freezeRotation = true;
            _rb.rotation = Quaternion.identity;
            gameObject.layer = _defaultLayer;
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        _rb.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(duration);
    }

    // OnBecameInvisible：死亡時も消えるように条件を変更
    private void OnBecameInvisible()
    {
        if (_isKnockedBack || _isDead)
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(GameObject target)
    {
        if (target != null)
        {
            _player = target.transform;
        }
    }

    // グループから目標地点を受け取る
    public void SetWanderTarget(Vector3 target)
    {
        if (_isFalling || _isDead) return;
        _wanderTarget = target;
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
        Vector3 startPos = transform.position;  // Vector2で管理（Z軸を触らない）

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // イーズイン：最初はゆっくり、だんだん加速
            float eased = t * t;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Vector2で操作してZ軸を保持
            transform.position = new Vector3(
                startPos.x,
                startPos.y - eased * 3f,
                transform.position.z
            );

            yield return null;
        }

        Destroy(gameObject);
    }

}

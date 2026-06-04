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
    [SerializeField] private float moveSpeed;       //  移動速度
    [SerializeField] private int AttackDamage;      //  接触時のダメージ
    [SerializeField] private float attackInterval;  //  攻撃間隔
    [SerializeField] private float maxHp;           //  敵の最大HP
    [SerializeField] private int level;             //  敵のレベル

    [SerializeField] private float rotationForce = 5f;     // 敵が吹き飛ぶ時の回転力

    [SerializeField] private float detectionRange = 5f;     // プレイヤーを発見する距離
    [SerializeField] private float wanderRadius = 3f;       // さまよう範囲
    [SerializeField] private float wanderInterval = 2f;     // 次の目標地点を決める間隔
    [SerializeField] private float fallGravity = 3f;        // 落下時の重力

    private float attackTimer;
    private bool isKnockedBack;                     // 吹き飛んでいるか
    private float currentHp;                        // 敵の現在HP
    private Transform player;
    private Rigidbody2D rb;
    private PlayerHealth playerHp;
    private float stopDistance;
    private bool isDead;
    private bool _isFalling;
    public float MaxHp => maxHp;    // 外部からmaxHpを参照用

    //  敵集団管理用
    private bool _isDiscovered;                             // プレイヤーを発見しているか
    private Vector2 _wanderTarget;                          // さまよう目標地点
    private float _wanderTimer;                             // さまようタイマー
    private float _currentMoveSpeed;                        // 現在の移動速度（グループから設定される)


    private int _defaultLayer;
    private int _knockbackLayer;

    public bool IsFalling => _isFalling;    //外部から落下中か参照用

    public int Level => level;                      // Experienceからレベルを参照用
    public float GetBaseSpeed() => moveSpeed;   // 基本速度を返す

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHp = maxHp;                          // 敵の最大HP

        Collider2D enemyCol = GetComponent<Collider2D>();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHp = playerObject.GetComponent<PlayerHealth>();
            Collider2D playerCol = playerObject.GetComponent<Collider2D>();
            stopDistance = enemyCol.bounds.extents.x + playerCol.bounds.extents.x;
        }
        else
        {
            Debug.LogError("Playerタグが見つかりません！");
        }

        _defaultLayer = gameObject.layer;
        _knockbackLayer = LayerMask.NameToLayer("EnemyKnockback");
        Physics2D.IgnoreLayerCollision(_knockbackLayer, _defaultLayer, true);
        Physics2D.IgnoreLayerCollision(_knockbackLayer, _knockbackLayer, true);

        _currentMoveSpeed = moveSpeed;
        _wanderTarget = GetNewWanderTarget();
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;
        if (_isFalling) return;
        if (playerHp != null && playerHp.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        if (isKnockedBack) return;

        // 未発見なら自動発見チェック
        if (!_isDiscovered)
        {
            float distToPlayer = Vector2.Distance(rb.position, player.position);
            if (distToPlayer <= detectionRange)
            {
                _isDiscovered = true;
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
        Vector2 direction = (Vector2)(player.position - transform.position);
        float distance = direction.magnitude;
        if (distance <= stopDistance)
        {
            rb.MovePosition(rb.position);
            return;
        }
        Vector2 newPosition = rb.position + direction.normalized * _currentMoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    // さまよう処理
    private void Wander()
    {
        Vector2 direction = (_wanderTarget - rb.position);
        if (direction.magnitude <= 0.1f) return;    // 目標地点に着いたら止まって待つ

        Vector2 newPosition = rb.position + direction.normalized * _currentMoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    // さまよう目標地点をランダムに決める
    private Vector2 GetNewWanderTarget()
    {
        _wanderTimer = wanderInterval;
        return rb.position + Random.insideUnitCircle * wanderRadius;
    }

    // グループから発見状態を設定する
    public void SetDiscovered(bool discovered)
    {
        _isDiscovered = discovered;
    }

    // グループから速度を設定する
    public void SetMoveSpeed(float speed)
    {
        _currentMoveSpeed = speed;
    }

    public bool IsDiscovered => _isDiscovered;  // グループから参照用

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        PlayerHealth playerHp = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHp != null)
        {
            playerHp.TakeDamage(AttackDamage);
        }
    }

    public bool TakeDamage(float damage)
    {
        if (isDead) return false;
        currentHp -= damage;
        Debug.Log($"敵HP: {currentHp,0}/{maxHp}");
        if (currentHp <= 0)
        {
            Die();
            return true;    // 死亡
        }
        return false;       // 生存
    }

    private void Die()
    {
        isDead = true;
    }

    public void KnockBack(Vector2 direction, float force)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.freezeRotation = false;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        rb.AddTorque(rotationForce, ForceMode2D.Impulse);
        StartCoroutine(KnockBackCoroutine());
    }

    private IEnumerator KnockBackCoroutine()
    {
        gameObject.layer = _knockbackLayer;

        yield return new WaitForSeconds(0.3f);

        // 死亡していたら吹き飛び状態のまま画面外まで飛ばし続ける
        if (!isDead)
        {
            isKnockedBack = false;
            rb.freezeRotation = true;
            rb.rotation = 0f;
            gameObject.layer = _defaultLayer;
        }
    }

    // OnBecameInvisible：死亡時も消えるように条件を変更
    private void OnBecameInvisible()
    {
        if (isKnockedBack || isDead)
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(GameObject target)
    {
        if (target != null)
        {
            player = target.transform;
        }
    }

    // グループから目標地点を受け取る
    public void SetWanderTarget(Vector2 target)
    {
        if (_isFalling || isDead) return;
        _wanderTarget = target;
    }

    public void FallOff()
    {

        if (_isFalling) return;
        if (!isDead && !isKnockedBack) return;
        _isFalling = true;
        isDead = true;
        isKnockedBack = false;
        StopAllCoroutines();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = false;

        StartCoroutine(FallAndDestroy());

    }

    private IEnumerator FallAndDestroy()
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector2 startPos = transform.position;  // Vector2で管理（Z軸を触らない）

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


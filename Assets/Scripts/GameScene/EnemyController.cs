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

    private float attackTimer;
    private bool isKnockedBack;                     // 吹き飛んでいるか
    private float currentHp;                        // 敵の現在HP
    private Transform player;
    private Rigidbody2D rb;
    private PlayerHealth playerHp;
    private float stopDistance;
    private bool isDead;

    private int _defaultLayer;
    private int _knockbackLayer;

    public int Level => level;                      // Experienceからレベルを参照用

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
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;
        if (playerHp != null && playerHp.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        if (isKnockedBack) return;

        Vector2 direction = (Vector2)(player.position - transform.position);
        float distance = direction.magnitude;
        if (distance <= stopDistance)
        {
            rb.MovePosition(rb.position);
            return;
        }
        Vector2 newPosition = rb.position + direction.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

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
}

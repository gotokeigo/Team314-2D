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
//--------------------------------------
using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private int AttackDamage;
    [SerializeField] private float attackInterval;
    private float attackTimer;
    private bool isKnockedBack;
    private Transform player;
    private Rigidbody2D rb;
    private PlayerHealth playerHp;
    private float stopDistance;

    private int _defaultLayer;      // 追加
    private int _knockbackLayer;    // 追加

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

        // レイヤーをキャッシュ ここから追加
        _defaultLayer = gameObject.layer;
        _knockbackLayer = LayerMask.NameToLayer("EnemyKnockback");

        // EnemyKnockbackレイヤーは通常の敵レイヤーと衝突しない
        Physics2D.IgnoreLayerCollision(_knockbackLayer, _defaultLayer, true);
        // EnemyKnockback同士も衝突しない（複数同時にノックバックされた場合）
        Physics2D.IgnoreLayerCollision(_knockbackLayer, _knockbackLayer, true);
        // ここまで追加
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
        if (!collision.gameObject.CompareTag("Player")) return;
        PlayerHealth playerHp = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHp != null)
        {
            playerHp.TakeDamage(AttackDamage);
        }
    }

    public void KnockBack(Vector2 direction, float force)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        StartCoroutine(KnockBackCoroutine());
    }

    private IEnumerator KnockBackCoroutine()
    {
        gameObject.layer = _knockbackLayer;     //ノックバック用レイヤーに切り替え

        yield return new WaitForSeconds(0.3f);
        isKnockedBack = false;

        gameObject.layer = _defaultLayer;       // 元のレイヤーに戻す
    }

    private void OnBecameInvisible()
    {
        if (isKnockedBack)
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

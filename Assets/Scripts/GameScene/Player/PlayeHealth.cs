//--------------------------------------
//
//  PlayerHealth.cs
//
//  概要
//  プレイヤーのHPを制御するスクリプト
//
//  更新履歴
//
//  2026/04/27  作成
//              敵に当たった時にプレイヤーのHPが減り無敵時間を得るようにした。
//              また、プレイヤーのHPが0になった時に操作を不能にし点滅した後にプレイヤーを消すようにした
//
//
//--------------------------------------

using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Tooltip("プレイヤー最大HP")]
    [SerializeField] private float maxHp;           //最大体力
    [Tooltip("被ダメ時の無敵時間")]
    [SerializeField] private float invincibleTime;  //無敵時間

    private bool isInvincible;
    private float currentHp;
    private Renderer Renderer;
    private Collider2D playerCollider;
    private int _enemyLayer;
    private int _enemyKnockbackLayer;


    public bool IsDead { get; private set; }

    public float MaxHp => maxHp;            //外部からの最大HP参照用
    public float CurrentHp => currentHp;    //外部からの現在HP参照用



    void Start()
    {
        currentHp = maxHp;
        IsDead = false;
        Renderer = GetComponentInChildren<Renderer>();
        playerCollider = GetComponent<Collider2D>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _enemyKnockbackLayer = LayerMask.NameToLayer("EnemyKnockback");
        int playerLayer = gameObject.layer;

    }

    public void TakeDamage(float damage)
    {
        //死亡中または無敵時間中はダメージは受けない
        if (IsDead || isInvincible) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();  //死亡処理呼び出し
        }
        else
        {
            StartCoroutine(InvincibleCoroutine());  // ダメージを受けても死ななかった場合は無敵時間を開始
        }
    }

    private void Die()
    {
        IsDead = true;

        Debug.Log("Player is Dead");

        // 動き停止
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        // 入力停止
        GetComponent<PlayerController>().enabled = false;
        // 当たり判定オフ
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(BlinkAndDestroy());
    }

    //点滅させてからプレイヤーを消す処理
    private IEnumerator BlinkAndDestroy()
    {
        for (int i = 0; i < 4; i++)
        {
            Renderer.enabled = false;         //スプライトを非表示
            yield return new WaitForSeconds(0.2f);

            Renderer.enabled = true;          //スプライトを表示
            yield return new WaitForSeconds(0.2f);
        }

        Destroy(gameObject);
    }

    // 無敵時間中に当たり判定をオフにする処理
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        // 敵レイヤーとの衝突だけオフ
        int playerLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyLayer, true);
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyKnockbackLayer, true);

        yield return new WaitForSeconds(invincibleTime);

        // 敵レイヤーとの衝突を戻す
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyLayer, false);
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyKnockbackLayer, false);

        isInvincible = false;
    }

}

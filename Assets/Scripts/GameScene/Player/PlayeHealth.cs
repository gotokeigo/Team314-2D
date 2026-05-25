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
    [SerializeField] private float maxHp;           //最大体力
    [SerializeField] private float invincibleTime;  //無敵時間

    private bool isInvincible;
    private float currentHp;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    public bool IsDead { get; private set; }


    void Start()
    {
        currentHp = maxHp;
        IsDead = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

    }

    public void TakeDamage(float damage)
    {
        //死亡中または無敵時間中はダメージは受けない
        if (IsDead || isInvincible) return;

        currentHp -= damage;
        Debug.Log("PlayerHP: " + currentHp);

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
            spriteRenderer.enabled = false;         //スプライトを非表示
            yield return new WaitForSeconds(0.2f);

            spriteRenderer.enabled = true;          //スプライトを表示
            yield return new WaitForSeconds(0.2f);
        }

        Destroy(gameObject);
    }

    // 無敵時間中に当たり判定をオフにする処理
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        playerCollider.enabled = false; //当たり判定オフ

        yield return new WaitForSeconds(invincibleTime); //設定された時間が経過したら

        playerCollider.enabled = true;  //当たり判定オン
        isInvincible = false;
    }

}

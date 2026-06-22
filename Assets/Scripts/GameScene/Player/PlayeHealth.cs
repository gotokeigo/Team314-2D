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
    [SerializeField] private float maxHp;
    [Tooltip("被ダメ時の無敵時間")]
    [SerializeField] private float invincibleTime;
    private bool _isInvincible;
    private float _currentHp;
    private Renderer _renderer;
    private Collider2D _playerCollider;
    private int _enemyLayer;
    private int _enemyKnockbackLayer;
    public bool IsDead { get; private set; }
    public float MaxHp => maxHp;
    public float CurrentHp => _currentHp;
    void Start()
    {
        _currentHp = maxHp;
        IsDead = false;
        _renderer = GetComponentInChildren<Renderer>();
        _playerCollider = GetComponent<Collider2D>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _enemyKnockbackLayer = LayerMask.NameToLayer("EnemyKnockback");
    }
    public void TakeDamage(float damage)
    {
        if (IsDead || _isInvincible) return;
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleCoroutine());
        }
    }
    private void Die()
    {
        IsDead = true;
        Debug.Log("Player is Dead");
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(BlinkAndDestroy());
    }
    private IEnumerator BlinkAndDestroy()
    {
        for (int i = 0; i < 4; i++)
        {
            _renderer.enabled = false;
            yield return new WaitForSeconds(0.2f);
            _renderer.enabled = true;
            yield return new WaitForSeconds(0.2f);
        }
        Destroy(gameObject);
    }
    private IEnumerator InvincibleCoroutine()
    {
        _isInvincible = true;
        int playerLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyLayer, true);
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyKnockbackLayer, true);
        yield return new WaitForSeconds(invincibleTime);
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyLayer, false);
        Physics2D.IgnoreLayerCollision(playerLayer, _enemyKnockbackLayer, false);
        _isInvincible = false;
    }
}

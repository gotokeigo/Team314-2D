//--------------------------------------
//
//  PlayerAttack.cs
//
//  概要
//  プレイヤーの攻撃を制御するスクリプト
//
//  更新履歴
//
//  2026/04/28  作成
//              敵を左クリックで攻撃、吹き飛ばせるようにした。
//
//  2026/04/29  プレイヤーが攻撃をしたときにヒットした敵の数をExperienceManagerに渡すように
//
//--------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;    // 攻撃の距離
    [SerializeField] private float knockBackForce = 10f;  // 吹き飛ばす力
    [SerializeField] private LayerMask enemyLayer;        // レイヤー

    private PlayerController _playerController;
    private bool _isAttacking;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void OnAttack(InputValue value)
    {
        if (_isAttacking) return;
        Attack();
    }

    private void Attack()
    {
        _isAttacking = true;
        Vector2 attackDirection = _playerController.LastMoveDirection;
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * attackRange;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCenter, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                Vector2 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                enemyController.KnockBack(knockBackDirection, knockBackForce);
            }
        }

        // ヒット数をExperienceManagerに渡す
        if (hitEnemies.Length > 0)
        {
            ExperienceManager.Instance.AddXp(hitEnemies.Length);
        }

        _isAttacking = false;
    }

    // デバッグ用：攻撃範囲をシーンビューに表示
    private void OnDrawGizmosSelected()
    {
        if (_playerController == null) return;
        Gizmos.color = Color.red;
        Vector2 attackCenter = (Vector2)transform.position +
            _playerController.LastMoveDirection * attackRange;
        Gizmos.DrawWireSphere(attackCenter, attackRange);
    }
}

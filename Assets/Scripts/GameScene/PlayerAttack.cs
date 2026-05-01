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
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;      // 攻撃の距離
    [SerializeField] private float knockBackForce = 10f;    // 吹き飛ばす力
    [SerializeField] private LayerMask enemyLayer;          // レイヤー
    [SerializeField] private GameObject batObject;          // バットオブジェクトをインスペクターで登録
    [SerializeField] private GameObject decoyPrefab;  // フィールドに追加


    private PlayerController _playerController;
    private bool _isAttacking;
    private SpriteRenderer _batSpriteRenderer;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();

        _batSpriteRenderer = batObject.GetComponent<SpriteRenderer>();
        Debug.Log(_batSpriteRenderer); // 追加

    }

    private void OnAttack(InputValue value)
    {
        if (_isAttacking) return;
        Attack();
    }

    private void Attack()
    {
        _isAttacking = true;
        _batSpriteRenderer.enabled = true;



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


        StartCoroutine(HideBat());

        // ヒット数をExperienceManagerに渡す
        if (hitEnemies.Length > 0)
        {
            ExperienceManager.Instance.AddXp(hitEnemies.Length);
        }
    }

    private IEnumerator HideBat()
    {
        Vector2 attackDirection = _playerController.LastMoveDirection;
        float baseAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

        float startAngle = baseAngle + 90f;
        float endAngle = baseAngle - 90f;

        float swingDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);

            // バットをプレイヤーの周りに弧を描くように移動
            float rad = currentAngle * Mathf.Deg2Rad;
            batObject.transform.localPosition = new Vector2(
                Mathf.Cos(rad) * attackRange,
                Mathf.Sin(rad) * attackRange
            );

            // バット自体の向きも合わせる
            batObject.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            yield return null;
        }

        _batSpriteRenderer.enabled = false;
        _isAttacking = false;
    }

    // デバッグ用：攻撃範囲をシーンビューに表示
    private void OnDrawGizmos()
    {
        if (_playerController == null) return;
        Gizmos.color = Color.red;
        Vector2 attackCenter = (Vector2)transform.position +
            _playerController.LastMoveDirection * attackRange;
        Gizmos.DrawWireSphere(attackCenter, attackRange);
    }

    private void OnDecoy(InputValue value)
    {
        Instantiate(decoyPrefab, transform.position, Quaternion.identity);
    }
}

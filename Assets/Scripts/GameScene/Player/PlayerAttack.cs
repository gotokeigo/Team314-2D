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
//  2026/05/10  攻撃を3段階チャージ式に変更
//              小チャージ：同レベル相手は一撃ではない
//              中チャージ：同レベル相手を一撃
//              大チャージ：自分のレベル×1.3まで一撃
//
//  2026/05/18  攻撃のチャージに応じて範囲、長さが変わるように変更。
//              攻撃をチャージしているときに移動速度が低下するように変更
//
//  2026/06/04  チャージ方式を変更。
//              移動中かつ発見状態の敵を引き連れているときに自動でチャージ。
//              チャージ速度は引き連れている敵の数に比例。
//              左クリックでその時点のチャージ段階の攻撃を発射しゲージリセット。
//              攻撃の見た目をバットスイングから扇形の風エフェクトに変更。
//
//--------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻撃設定")]
    [Tooltip("吹き飛ばした時の敵の飛んでく速さ")]
    [SerializeField] private float knockbackForce;
    [Tooltip("敵のレイヤーを設定する")]
    [SerializeField] private LayerMask enemyLayer;      // 敵のレイヤーを設定する
 //   [SerializeField] private GameObject decoyPrefab;

    [Header("チャージ設定")]
    [Tooltip("中チャージになる秒数")]
    [SerializeField] private float mediumChargeTime = 0.5f;     // 中チャージになる秒数
    [Tooltip("強チャージになる秒数")]
    [SerializeField] private float largeChargeTime = 1.0f;      // 強チャージになる秒数
    [Tooltip("敵1体辺りのチャージ速度上昇倍率(0.1で1.1倍)")]
    [SerializeField] private float enemyCountSpeedMultiplier = 0.1f; // 敵1体あたりの速度上昇倍率(0.1で1.1倍)

    [Header("小チャージ設定")]
    [Tooltip("小攻撃の攻撃が届く距離")]
    [SerializeField] private float smallAttackRange;
    [Tooltip("小攻撃の攻撃が届く角度")]
    [SerializeField] private float smallAttackAngle;

    [Header("中チャージ設定")]
    [Tooltip("中攻撃の攻撃が届く距離")]
    [SerializeField] private float mediumAttackRange;
    [Tooltip("中攻撃の攻撃が届く角度")]
    [SerializeField] private float mediumAttackAngle;

    [Header("強チャージ設定")]
    [Tooltip("強攻撃の攻撃が届く距離")]
    [SerializeField] private float largeAttackRange;
    [Tooltip("強攻撃の攻撃が届く角度")]
    [SerializeField] private float largeAttackAngle;

    [Header("ダメージ設定")]
    [SerializeField] private float mediumDamage;
    [SerializeField] private float largeKillLevelMultiplier;

    [Header("風エフェクト設定")]
    [SerializeField] private GameObject windEffectPrefab;       // 風エフェクトのPrefab

    private enum ChargeLevel { None, Small, Medium, Large }

    private PlayerController _playerController;
    private InputAction _attackAction;
    private float _chargeAmount = 0f;                           // 現在のチャージ量（0〜1）
    private bool _isAttacking;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _attackAction = GetComponent<PlayerInput>().actions["Attack"];
    }

    private void Update()
    {
        UpdateCharge();

        if (_attackAction.WasReleasedThisFrame() && !_isAttacking)
        {
            ChargeLevel level = DetermineChargeLevel(_chargeAmount);
            if (level != ChargeLevel.None)
            {
                Attack(level);
            }
            _chargeAmount = 0f;     // 攻撃ボタンを押したらリセット
        }
    }

    private void UpdateCharge()
    {
        bool isMoving = _playerController.LastMoveDirection != Vector2.zero;
        int discoveredCount = EnemyManager.Instance != null
            ? EnemyManager.Instance.DiscoveredEnemyCount
            : 0;

        if (isMoving && discoveredCount > 0)
        {
            // 敵の数が多いほど速くなる（例: 10体で2倍速）
            float speedMultiplier = 1f + discoveredCount * enemyCountSpeedMultiplier;
            _chargeAmount += Time.deltaTime * speedMultiplier;
            _chargeAmount = Mathf.Min(_chargeAmount, largeChargeTime);
        }
    }

    private ChargeLevel DetermineChargeLevel(float amount)
    {
        if (amount >= largeChargeTime) return ChargeLevel.Large;
        if (amount >= mediumChargeTime) return ChargeLevel.Medium;
        if (amount > 0f) return ChargeLevel.Small;
        return ChargeLevel.None;
    }

    private void Attack(ChargeLevel chargeLevel)
    {
        _isAttacking = true;

        float currentRange = chargeLevel switch
        {
            ChargeLevel.Small => smallAttackRange,
            ChargeLevel.Medium => mediumAttackRange,
            ChargeLevel.Large => largeAttackRange,
            _ => smallAttackRange
        };
        float currentAngle = chargeLevel switch
        {
            ChargeLevel.Small => smallAttackAngle,
            ChargeLevel.Medium => mediumAttackAngle,
            ChargeLevel.Large => largeAttackAngle,
            _ => smallAttackAngle
        };

        Vector2 attackDirection = _playerController.LastMoveDirection;
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * currentRange;

        // 風エフェクト表示
        if (windEffectPrefab != null)
        {
            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            Instantiate(windEffectPrefab,
                attackCenter,
                Quaternion.Euler(0f, 0f, angle));
        }

        // 扇形範囲内の敵に当たり判定
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCenter, currentRange, enemyLayer);
        List<Collider2D> hitEnemiesInFan = new List<Collider2D>();

        foreach (Collider2D enemy in hitEnemies)
        {
            Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(attackDirection, dirToEnemy);
            if (angle <= currentAngle / 2f)
            {
                hitEnemiesInFan.Add(enemy);
            }
        }

        int killCount = 0;

        foreach (Collider2D enemy in hitEnemiesInFan)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                float damage = CalculateDamage(chargeLevel, enemyController);
                bool died = enemyController.TakeDamage(damage);

                if (died)
                {
                    Vector2 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyController.KnockBack(knockBackDirection, knockbackForce);
                    killCount++;
                }
            }
        }

        StartCoroutine(FinishAttack());

        if (killCount > 0)
        {
            ExperienceManager.Instance.AddXp(killCount);
        }
    }

    private float CalculateDamage(ChargeLevel chargeLevel, EnemyController enemy)
    {
        switch (chargeLevel)
        {
            case ChargeLevel.Small:
                return enemy.MaxHp / 2f;

            case ChargeLevel.Medium:
                return mediumDamage;

            case ChargeLevel.Large:
                int playerLevel = ExperienceManager.Instance.PlayerLevel;
                if (enemy.Level <= playerLevel * largeKillLevelMultiplier)
                {
                    return float.MaxValue;
                }
                else
                {
                    return mediumDamage;
                }

            default:
                return enemy.MaxHp / 2f;
        }
    }

    // 攻撃フラグを戻すだけ（エフェクト側で表示時間を管理）
    private IEnumerator FinishAttack()
    {
        yield return null;
        _isAttacking = false;
    }

    //private void OnDecoy(InputValue value)
    //{
    //    Instantiate(decoyPrefab, transform.position, Quaternion.identity);
    //}

    private void OnDrawGizmos()
    {
        if (_playerController == null) return;
        Vector2 attackDirection = _playerController.LastMoveDirection;

        DrawFanGizmo(attackDirection, smallAttackRange, smallAttackAngle, Color.red);
        DrawFanGizmo(attackDirection, mediumAttackRange, mediumAttackAngle, Color.yellow);
        DrawFanGizmo(attackDirection, largeAttackRange, largeAttackAngle, Color.green);
    }

    private void DrawFanGizmo(Vector2 attackDirection, float range, float angle, Color color)
    {
        Gizmos.color = color;
        float halfAngle = angle / 2f;
        Vector3 leftDir = Quaternion.Euler(0, 0, halfAngle) * (Vector3)attackDirection;
        Vector3 rightDir = Quaternion.Euler(0, 0, -halfAngle) * (Vector3)attackDirection;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * range * 2f);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * range * 2f);
    }
}

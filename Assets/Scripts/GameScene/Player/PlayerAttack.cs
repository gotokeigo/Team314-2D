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
//  2026/06/11  ボスへの攻撃処理を追加
//
//--------------------------------------
using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻撃設定")]
    [Tooltip("敵を吹き飛ばしたときの敵が吹き飛んでいく速さ")]
    [SerializeField] private float knockbackForce;
    [Tooltip("攻撃が当たるレイヤー")]
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("攻撃時にプレイヤーが振るオブジェクトを指定")]
    [SerializeField] private GameObject weaponObject;
    [Tooltip("攻撃間隔（秒）")]
    [SerializeField] private float attackInterval = 0.5f;
    [Tooltip("敵に与える硬直時間（秒）")]
    [SerializeField] private float stunDuration = 0.3f;
    //  [SerializeField] private GameObject decoyPrefab;

    [Header("チャージ関連")]
    [Tooltip("中攻撃になるのに必要な溜め時間")]
    [SerializeField] private float mediumChargeTime;
    [Tooltip("強攻撃になるのに必要な溜め時間")]
    [SerializeField] private float largeChargeTime;
    [SerializeField] private float chargeSpeedMultiplier;

    [Header("小チャージ設定")]
    [Tooltip("小攻撃の範囲(長さ)")]
    [SerializeField] private float smallAttackRange;
    [Tooltip("小攻撃の範囲(角度)")]
    [SerializeField] private float smallAttackAngle;

    [Header("中チャージ設定")]
    [Tooltip("中攻撃の範囲(長さ)")]
    [SerializeField] private float mediumAttackRange;
    [Tooltip("中攻撃の範囲(角度)")]
    [SerializeField] private float mediumAttackAngle;

    [Header("強チャージ設定")]
    [Tooltip("強攻撃の範囲(長さ)")]
    [SerializeField] private float largeAttackRange;
    [Tooltip("強攻撃の範囲(角度)")]
    [SerializeField] private float largeAttackAngle;


    private enum ChargeLevel { Small, Medium, Large }

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;
    private bool _isAttacking;
    private bool _isCharging;
    private float _chargeStartTime;
    private SpriteRenderer _weaponSpriteRenderer;
    private InputAction _attackAction;
    private float _attackTimer = 0f;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _weaponSpriteRenderer = weaponObject.GetComponent<SpriteRenderer>();
        _attackAction = GetComponent<PlayerInput>().actions["Attack"];
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnAttack(InputValue value)
    {
        if (_playerHealth != null && _playerHealth.IsDead) return;  //プレイヤーのHPがないとき攻撃できなくする
        if (_isAttacking || _isCharging) return;
        _isCharging = true;
        _chargeStartTime = Time.time;
        _playerController.SetSpeedMultiplier(chargeSpeedMultiplier);
    }

    // Update() に追加
    private void Update()
    {
        if (_playerHealth != null && _playerHealth.IsDead) return;

        // 攻撃タイマーを減らす
        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
        }

        if (!_isCharging || _isAttacking) return;

        if (_attackAction.WasReleasedThisFrame())
        {
            if (_attackTimer > 0f) return;  // インターバル中は攻撃しない
            _isCharging = false;
            _playerController.SetSpeedMultiplier(1.0f);
            float chargeTime = Time.time - _chargeStartTime;
            ChargeLevel chargeLevel = DetermineChargeLevel(chargeTime);
            Attack(chargeLevel);
            _attackTimer = attackInterval;  // タイマーをセット
        }
    }

    private ChargeLevel DetermineChargeLevel(float chargeTime)
    {
        if (chargeTime >= largeChargeTime) return ChargeLevel.Large;
        if (chargeTime >= mediumChargeTime) return ChargeLevel.Medium;
        return ChargeLevel.Small;
    }

    private void Attack(ChargeLevel chargeLevel)
    {
        _isAttacking = true;
        _weaponSpriteRenderer.enabled = true;

        Debug.Log($"チャージレベル: {chargeLevel}");

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
            // 通常敵への攻撃
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
                else
                {
                    enemyController.Stun(stunDuration);  // 追加: 倒しきれなかったら硬直
                }
                continue;   // EnemyControllerがあればBossControllerは見ない
            }

            // ボスへの攻撃
            BossController bossController = enemy.GetComponent<BossController>();
            if (bossController != null)
            {
                float damage = CalculateDamageBoss(chargeLevel, bossController);
                bool died = bossController.TakeDamage(damage);
                if (died)
                {
                    Vector2 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    bossController.KnockBack(knockBackDirection, knockbackForce);
                }
            }
        }

        StartCoroutine(HideBat(currentRange));

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
                return enemy.MaxHp / 2f;    // 必ず2発

            case ChargeLevel.Medium:
            case ChargeLevel.Large:
                return ExperienceManager.Instance.PlayerAttackPower;

            default:
                return enemy.MaxHp / 2f;
        }
    }

    private float CalculateDamageBoss(ChargeLevel chargeLevel, BossController boss)
    {
        switch (chargeLevel)
        {
            case ChargeLevel.Small:
                return boss.MaxHp / 2f;     // 必ず2発

            case ChargeLevel.Medium:
            case ChargeLevel.Large:
                return ExperienceManager.Instance.PlayerAttackPower;

            default:
                return boss.MaxHp / 2f;
        }
    }

   

    private IEnumerator HideBat(float range)
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
            float rad = currentAngle * Mathf.Deg2Rad;
            weaponObject.transform.localPosition = new Vector2(
                Mathf.Cos(rad) * range,
                Mathf.Sin(rad) * range
            );
            weaponObject.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        _weaponSpriteRenderer.enabled = false;
        _isAttacking = false;
    }

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

    //  private void OnDecoy(InputValue value)
    //  {
    //      Instantiate(decoyPrefab, transform.position, Quaternion.identity);
    //  }
}

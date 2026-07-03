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
//  2026/06/16  StatusUIに現在のチャージレベルを引き渡せるように変更
//
//  2026/06/22  硬直をやめて生存時に小ノックバックするように変更
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
    [Tooltip("敵を吹き飛ばしたときの敵が吹き飛んでいく速さ（死亡時）")]
    [SerializeField] private float knockbackForce;
    [Tooltip("攻撃が当たるレイヤー")]
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("攻撃時にプレイヤーが振るオブジェクトを指定")]
    [SerializeField] private GameObject weaponObject;
    [Tooltip("攻撃間隔（秒）")]
    [SerializeField] private float attackInterval = 0.5f;
    [Tooltip("ヒット時の小ノックバック力（生存時）")]
    [SerializeField] private float hitKnockbackForce;

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

    [SerializeField]
    private GameObject attackRangeObject;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip attackSE;

    public enum ChargeLevel { Small, Medium, Large }
    public ChargeLevel CurrentChargeLevel { get; private set; } = ChargeLevel.Small;
    public bool IsCharging => _isCharging;

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
        if (_playerHealth != null && _playerHealth.IsDead) return;
        if (_isAttacking || _isCharging) return;
        _isCharging = true;
        attackRangeObject.SetActive(true);
        _chargeStartTime = Time.time;
        _playerController.SetSpeedMultiplier(chargeSpeedMultiplier);
        _playerController.SetCharging(true);

        Vector2 attackDirection = GetMouseDirection();

        _playerController.SetLookDirection(attackDirection);
    }

    private void Update()
    {
        if (_playerHealth != null && _playerHealth.IsDead) return;

        if (_isCharging)
        {
            Vector2 mouseDir = GetMouseDirection();
            _playerController.SetLookDirection(mouseDir);
        }

        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
        }

        if (!_isCharging || _isAttacking) return;

        float elapsedTime = Time.time - _chargeStartTime;
        CurrentChargeLevel = DetermineChargeLevel(elapsedTime);

        float range = smallAttackRange;

        switch (CurrentChargeLevel)
        {
            case ChargeLevel.Medium:
                range = mediumAttackRange;
                break;

            case ChargeLevel.Large:
                range = largeAttackRange;
                break;
        }

        attackRangeObject.transform.localScale =
            new Vector3(range, range, 1.0f);

        // Vector2 dir = _playerController.LastMoveDirection;
        Vector2 dir = GetMouseDirection();

        if (dir != Vector2.zero)
        {
            attackRangeObject.transform.localPosition =
                dir.normalized * range;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            attackRangeObject.transform.localRotation =
                Quaternion.Euler(0f, 0f, angle);
        }

        if (_attackAction.WasReleasedThisFrame())
        {
            attackRangeObject.SetActive(false);
            if (_attackTimer > 0f) return;
            _isCharging = false;
            _playerController.SetSpeedMultiplier(1.0f);
            _playerController.SetCharging(false);
            float chargeTime = Time.time - _chargeStartTime;
            CurrentChargeLevel = DetermineChargeLevel(chargeTime);
            ChargeLevel chargeLevel = DetermineChargeLevel(chargeTime);
            Attack(chargeLevel);
            _attackTimer = attackInterval;
        }
    }

    public ChargeLevel DetermineChargeLevel(float chargeTime)
    {
        if (chargeTime >= largeChargeTime) return ChargeLevel.Large;
        if (chargeTime >= mediumChargeTime) return ChargeLevel.Medium;
        return ChargeLevel.Small;
    }

    private void Attack(ChargeLevel chargeLevel)
    {
        _isAttacking = true;
        _weaponSpriteRenderer.enabled = true;

        audioSource.PlayOneShot(attackSE);

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

        //Vector2 attackDirection = _playerController.LastMoveDirection;
        Vector2 attackDirection = GetMouseDirection();

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
                    // 死亡時は強くノックバック
                    Vector2 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyController.KnockBack(knockBackDirection, knockbackForce);
                    killCount++;
                }
                else
                {
                    // 生存時は小ノックバック
                    Vector2 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyController.SmallKnockBack(knockBackDirection, hitKnockbackForce);
                }
                continue;
            }

            // ボスへの攻撃
            BossController bossController = enemy.GetComponent<BossController>();
            if (bossController != null)
            {
                float damage = CalculateDamageBoss(chargeLevel, bossController);
                bool died = bossController.TakeDamage(damage);
                if (died)
                {
                    // 死亡時は強くノックバック
                    Vector2 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    bossController.KnockBack(knockBackDirection, knockbackForce);
                }
                else
                {
                    // 生存時は小ノックバック
                    Vector2 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    bossController.SmallKnockBack(knockBackDirection, hitKnockbackForce);
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
                return enemy.MaxHp / 2f;

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
                return boss.MaxHp / 2f;

            case ChargeLevel.Medium:
            case ChargeLevel.Large:
                return ExperienceManager.Instance.PlayerAttackPower;

            default:
                return boss.MaxHp / 2f;
        }
    }

    private IEnumerator HideBat(float range)
    {
        // Vector2 attackDirection = _playerController.LastMoveDirection;
        Vector2 attackDirection = GetMouseDirection();

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
        CurrentChargeLevel = ChargeLevel.Small;
        //_playerController.lookMode = PlayerController.LookMode.Move;
    }

    private void OnDrawGizmos()
    {
        if (_playerController == null) return;
        // Vector2 attackDirection = _playerController.LastMoveDirection;
        Vector2 attackDirection = GetMouseDirection();

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
    private Vector2 GetMouseDirection()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();

        // スクリーン座標 → ワールド座標
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z = 0;

        return (mousePos - transform.position).normalized;
    }

}

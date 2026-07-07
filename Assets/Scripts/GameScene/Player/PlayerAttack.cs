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
//  2026/07/06  3D対応。
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

    [SerializeField] private GameObject attackRangeObject;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSE;

    [Header("エフェクト")]
    [SerializeField] private GameObject chargeEffectPrefab;
    [SerializeField] private GameObject chargeWindPrefab;
    [SerializeField] private GameObject slashEffectPrefab;
    //[SerializeField] private GameObject attackRangeFBX;
    //[SerializeField] private GameObject attackRangePivot;
    //[SerializeField] private float smallOffset = 2f;
    //[SerializeField] private float mediumOffset = 3f;
    //[SerializeField] private float largeOffset = 4f;
    [Header("攻撃範囲表示")]
    [SerializeField] private LineRenderer lineRenderer;
    private GameObject chargeEffectInstance;
    private GameObject chargeWindInstance;
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
    private ParticleSystem[] chargeParticles;
    private bool maxChargeReached = false;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _weaponSpriteRenderer = weaponObject.GetComponent<SpriteRenderer>();
        _attackAction = GetComponent<PlayerInput>().actions["Attack"];
        _playerHealth = GetComponent<PlayerHealth>();
        lineRenderer.enabled = false;
    }

    private void OnAttack(InputValue value)
    {
        if (_playerHealth != null && _playerHealth.IsDead) return;
        if (_isAttacking || _isCharging) return;
        _isCharging = true;
        //attackRangeFBX.SetActive(true);
        //attackRangeObject.SetActive(true);
        lineRenderer.enabled = true;
        _chargeStartTime = Time.time;
        _playerController.SetSpeedMultiplier(chargeSpeedMultiplier);
        _playerController.SetCharging(true);

        if (chargeEffectPrefab != null && chargeEffectInstance == null)
        {
            chargeEffectInstance = Instantiate(chargeEffectPrefab, transform.position, Quaternion.identity, transform);
            chargeWindInstance = Instantiate(chargeWindPrefab, transform.position, Quaternion.identity, transform);
        }

        chargeParticles = chargeEffectInstance.GetComponentsInChildren<ParticleSystem>();
        maxChargeReached = false;

        Vector3 attackDirection = GetMouseDirection();
        _playerController.SetLookDirection(attackDirection);
    }

    private void Update()
    {
        if (_playerHealth != null && _playerHealth.IsDead) return;

        if (_isCharging)
        {
            Vector3 mouseDir = GetMouseDirection();
            _playerController.SetLookDirection(mouseDir);
        }

        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
        }

        if (!_isCharging || _isAttacking) return;

        float elapsedTime = Time.time - _chargeStartTime;
        CurrentChargeLevel = DetermineChargeLevel(elapsedTime);

        if (!maxChargeReached && CurrentChargeLevel == ChargeLevel.Large)
        {
            maxChargeReached = true;
            if (chargeParticles != null)
            {
                foreach (ParticleSystem ps in chargeParticles)
                {
                    ps.Pause();
                }
            }
        }

        //float offset = smallOffset;

        //switch (CurrentChargeLevel)
        //{
        //    case ChargeLevel.Small:
        //        attackRangeFBX.transform.localScale = new Vector3(60f, 60f, 60f);
        //        offset = smallOffset;
        //        break;
        //    case ChargeLevel.Medium:
        //        attackRangeFBX.transform.localScale = new Vector3(80f, 80f, 80f);
        //        offset = mediumOffset;
        //        break;
        //    case ChargeLevel.Large:
        //        attackRangeFBX.transform.localScale = new Vector3(100f, 100f, 100f);
        //        offset = largeOffset;
        //        break;
        //}

        //attackRangeFBX.transform.localPosition = new Vector3(0, offset, 0);

        Vector3 dir = GetMouseDirection();

        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
           // attackRangePivot.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 270.0f);

            float range = CurrentChargeLevel switch
            {
                ChargeLevel.Small => smallAttackRange,
                ChargeLevel.Medium => mediumAttackRange,
                ChargeLevel.Large => largeAttackRange,
                _ => smallAttackRange
            };
            DrawAttackRange(dir, range * 1.7f);
        }

        if (_attackAction.WasReleasedThisFrame())
        {
            //  attackRangeFBX.SetActive(false);
            //  attackRangeObject.SetActive(false);
            lineRenderer.enabled = false;
            if (_attackTimer > 0f) return;
            _isCharging = false;
            _playerController.SetSpeedMultiplier(1.0f);
            _playerController.SetCharging(false);
            if (chargeEffectInstance != null)
            {
                Destroy(chargeEffectInstance);
            }

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



        Vector3 attackDirection = GetMouseDirection();
        float effectAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        Vector3 attackCenter = transform.position + attackDirection * currentRange;

        if (slashEffectPrefab != null)
        {
            GameObject slash = Instantiate(slashEffectPrefab, transform.position, Quaternion.Euler(0, 0, effectAngle));
            ParticleSystem[] particles = slash.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in particles)
            {
                var main = ps.main;
                switch (chargeLevel)
                {
                    case ChargeLevel.Small: main.startSize = 25f; break;
                    case ChargeLevel.Medium: main.startSize = 35f; break;
                    case ChargeLevel.Large: main.startSize = 50f; break;
                }
            }
        }

        Collider[] hitEnemies = Physics.OverlapSphere(attackCenter, currentRange, enemyLayer);
        List<Collider> hitEnemiesInFan = new List<Collider>();

        foreach (Collider enemy in hitEnemies)
        {
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(attackDirection, dirToEnemy);
            if (angle <= currentAngle / 2f)
            {
                hitEnemiesInFan.Add(enemy);
            }
        }

        int killCount = 0;

        foreach (Collider enemy in hitEnemiesInFan)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                float damage = CalculateDamage(chargeLevel, enemyController);
                bool died = enemyController.TakeDamage(damage);
                if (died)
                {
                    Vector3 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyController.KnockBack(knockBackDirection, knockbackForce);
                    killCount++;
                }
                else
                {
                    Vector3 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyController.SmallKnockBack(knockBackDirection, hitKnockbackForce);
                }
                continue;
            }

            BossController bossController = enemy.GetComponent<BossController>();
            if (bossController != null)
            {
                float damage = CalculateDamageBoss(chargeLevel, bossController);
                bool died = bossController.TakeDamage(damage);
                if (died)
                {
                    Vector3 knockBackDirection = (enemy.transform.position - transform.position).normalized;
                    bossController.KnockBack(knockBackDirection, knockbackForce);
                }
                else
                {
                    Vector3 knockBackDirection = (enemy.transform.position - transform.position).normalized;
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
        Vector3 attackDirection = GetMouseDirection();

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
            weaponObject.transform.localPosition = new Vector3(
                Mathf.Cos(rad) * range,
                Mathf.Sin(rad) * range,
                0f
            );
            weaponObject.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        _weaponSpriteRenderer.enabled = false;
        _isAttacking = false;
        CurrentChargeLevel = ChargeLevel.Small;
    }

    private void OnDrawGizmos()
    {
        if (_playerController == null) return;
        Vector3 attackDirection = GetMouseDirection();

        DrawFanGizmo(attackDirection, smallAttackRange, smallAttackAngle, Color.red);
        DrawFanGizmo(attackDirection, mediumAttackRange, mediumAttackAngle, Color.yellow);
        DrawFanGizmo(attackDirection, largeAttackRange, largeAttackAngle, Color.green);
    }

    private void DrawFanGizmo(Vector3 attackDirection, float range, float angle, Color color)
    {
        Gizmos.color = color;
        float halfAngle = angle / 2f;
        Vector3 leftDir = Quaternion.Euler(0, 0, halfAngle) * attackDirection;
        Vector3 rightDir = Quaternion.Euler(0, 0, -halfAngle) * attackDirection;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * range * 2f);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * range * 2f);
    }

    private Vector3 GetMouseDirection()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // Y=プレイヤーのY座標の平面との交点を取る
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.y = transform.position.y;
            return (worldPos - transform.position).normalized;
        }

        return transform.forward;
    }

    private void DrawAttackRange(Vector3 direction, float range)
    {
        int segments = 30;

        float attackAngle = CurrentChargeLevel switch
        {
            ChargeLevel.Small => smallAttackAngle,
            ChargeLevel.Medium => mediumAttackAngle,
            ChargeLevel.Large => largeAttackAngle,
            _ => smallAttackAngle
        };

        //lineRenderer.positionCount = segments + 2;
        lineRenderer.positionCount = segments + 3;
        // プレイヤーの位置
        lineRenderer.SetPosition(0, transform.position);

        // 扇形の開始角度
        float baseAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - attackAngle / 2f;

        // 扇形を描画
        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + attackAngle * i / segments;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 point = transform.position + new Vector3(
                Mathf.Cos(rad),
                0f,
                Mathf.Sin(rad)
            ) * range;

            lineRenderer.SetPosition(i + 1, point);
        }
        lineRenderer.SetPosition(segments + 2, transform.position);
    }
    //private void DrawAttackRange(Vector3 direction, float range)
    //{
    //    int segments = 30;

    //    float attackAngle = CurrentChargeLevel switch
    //    {
    //        ChargeLevel.Small => smallAttackAngle,
    //        ChargeLevel.Medium => mediumAttackAngle,
    //        ChargeLevel.Large => largeAttackAngle,
    //        _ => smallAttackAngle
    //    };

    //    float startAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - attackAngle / 2f;

    //    for (int i = 0; i <= segments; i++)
    //    {
    //        float angle = startAngle + attackAngle * i / segments;
    //        float rad = angle * Mathf.Deg2Rad;

    //        Vector3 pos = transform.position + new Vector3(
    //            Mathf.Cos(rad),
    //            Mathf.Sin(rad),
    //            0f
    //        ) * range;
    //    }
    //}
}

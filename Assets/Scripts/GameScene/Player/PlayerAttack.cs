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
//--------------------------------------
using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{

    [Header("攻撃設定")]
    [SerializeField] private float knockBackForce;          //  吹き飛ばす力    
    [SerializeField] private LayerMask enemyLayer;          //  敵のレイヤー
    [SerializeField] private GameObject batObject;          //  攻撃時に表示されるbatのオブジェクト

    //けす？
    [SerializeField] private GameObject decoyPrefab;        //  Gキー(現状)を押したときに出るデコイのモデルを入れる

    [Header("小チャージ設定")]                                   
    [SerializeField] private float smallAttackRange;             //  攻撃範囲
    [SerializeField] private float smallAttackAngle;             //  扇型の角度

    [Header("中チャージ設定")]                                   
    [SerializeField] private float mediumAttackRange;            //  攻撃範囲
    [SerializeField] private float mediumAttackAngle;            //  扇型の角度

    [Header("強チャージ設定")]                                   
    [SerializeField] private float largeAttackRange;             //  攻撃範囲
    [SerializeField] private float largeAttackAngle;             //  扇型の角度

    [Header("チャージ時間")]
    [SerializeField] private float mediumChargeTime;        // 中チャージになる秒数
    [SerializeField] private float largeChargeTime;         // 大チャージになる秒数
    [SerializeField] private float chargeSpeedMultiplier = 1.0f;    //チャージ中の速度倍率

    [Header("ダメージ設定")]
    [SerializeField] private float mediumDamage;            // 中チャージダメージ（同レベル敵のHP以上に設定）
    [SerializeField] private float largeKillLevelMultiplier;// 大チャージで一撃のレベル倍率

    private enum ChargeLevel { Small, Medium, Large }

    private PlayerController _playerController;
    private bool _isAttacking;                      //  攻撃しているか
    private bool _isCharging;                       //  攻撃をチャージしているか
    private float _chargeStartTime;                 //  チャージを始めた時間
    private SpriteRenderer _batSpriteRenderer;      //  バットのスプライトレンダラー切り替え用
    private InputAction _attackAction;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _batSpriteRenderer = batObject.GetComponent<SpriteRenderer>();

        _attackAction = GetComponent<PlayerInput>().actions["Attack"];  //PlayerInputからAttackアクションを取得
    }

    //  ボタンを押した時だけチャージ開始
    private void OnAttack(InputValue value)
    {
        if (_isAttacking || _isCharging) return;

        _isCharging = true;
        _chargeStartTime = Time.time;   //  チャージの時間を数える
        _playerController.SetSpeedMultiplier(chargeSpeedMultiplier);
    }

    private void Update()
    {
        if (!_isCharging || _isAttacking) return;

        //ボタンが離されたら攻撃
        if (_attackAction.WasReleasedThisFrame())
        {
            _isCharging = false;
            _playerController.SetSpeedMultiplier(1.0f); //速度をもとに戻す
            float chargeTime = Time.time - _chargeStartTime;
            ChargeLevel chargeLevel = DetermineChargeLevel(chargeTime);     // チャージ時間に応じたチャージレベルを取得
            Attack(chargeLevel);
        }
    }

    //  チャージ時間に応じて渡す値を変えるのを制御するプログラム
    private ChargeLevel DetermineChargeLevel(float chargeTime)
    {
        if (chargeTime >= largeChargeTime)
        {
            return ChargeLevel.Large;
        }
        if (chargeTime >= mediumChargeTime)
        {
            return ChargeLevel.Medium;
        }

        return ChargeLevel.Small;
    }

    private void Attack(ChargeLevel chargeLevel)
    {
        _isAttacking = true;
        _batSpriteRenderer.enabled = true;

        Debug.Log($"チャージレベル: {chargeLevel}");

        // チャージレベルに応じた範囲と角度を取得
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

        int killCount = 0;  //倒した数をカウント

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
                    enemyController.KnockBack(knockBackDirection, knockBackForce);
                    killCount++;    //倒したらカウント
                }
            }
        }

        StartCoroutine(HideBat(currentRange));  // currentRangeを渡す

        if (killCount > 0)                          
        {
            ExperienceManager.Instance.AddXp(killCount);
        }
    }

    private float CalculateDamage(ChargeLevel chargeLevel, EnemyController enemy)
    {
        switch (chargeLevel)
        {
            // 弱チャージ：どんな敵でも必ず2発
            case ChargeLevel.Small:
                return enemy.MaxHp / 2f;    // 変更

            // 中チャージ
            case ChargeLevel.Medium:
                return mediumDamage;

            // 強チャージ
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

    //  武器を振っているように制御してる
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
            batObject.transform.localPosition = new Vector2(
                Mathf.Cos(rad) * range,     // attackRange → range
                Mathf.Sin(rad) * range      // attackRange → range
            );
            batObject.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        _batSpriteRenderer.enabled = false;
        _isAttacking = false;
    }

    //  実行中に一時停止(Shift+Ctrl+P)したときにプレイヤーの攻撃範囲を表示するプログラム
    //  小の範囲は赤、中の範囲は黄色、大の範囲は緑
    private void OnDrawGizmos()
    {
        if (_playerController == null) return;
        Vector2 attackDirection = _playerController.LastMoveDirection;

        DrawFanGizmo(attackDirection, smallAttackRange, smallAttackAngle, Color.red);
        DrawFanGizmo(attackDirection, mediumAttackRange, mediumAttackAngle, Color.yellow);
        DrawFanGizmo(attackDirection, largeAttackRange, largeAttackAngle, Color.green);
    }

    //  プレイヤーの攻撃範囲を表示するために、攻撃距離　角度を取得し計算するプログラム
    private void DrawFanGizmo(Vector2 attackDirection, float range, float angle, Color color)
    {
        Gizmos.color = color;
        float halfAngle = angle / 2f;
        Vector3 leftDir = Quaternion.Euler(0, 0, halfAngle) * (Vector3)attackDirection;
        Vector3 rightDir = Quaternion.Euler(0, 0, -halfAngle) * (Vector3)attackDirection;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * range * 2f);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * range * 2f);
    }

    private void OnDecoy(InputValue value)
    {
        Instantiate(decoyPrefab, transform.position, Quaternion.identity);
    }
}

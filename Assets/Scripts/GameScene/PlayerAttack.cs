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
//--------------------------------------
using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{

    [Header("攻撃設定")]
    [SerializeField] private float attackRange;             //  攻撃範囲
    [SerializeField] private float knockBackForce;          //  吹き飛ばす力     
    [SerializeField] private float attackAngle;             //  扇型の角度
    [SerializeField] private LayerMask enemyLayer;          //  敵のレイヤー
    [SerializeField] private GameObject batObject;          //  攻撃時に表示されるbatのオブジェクト

    //けす？
    [SerializeField] private GameObject decoyPrefab;        //  Gキー(現状)を押したときに出るデコイのモデルを入れる

    [Header("チャージ時間")]
    [SerializeField] private float mediumChargeTime;        // 中チャージになる秒数
    [SerializeField] private float largeChargeTime;         // 大チャージになる秒数
    [SerializeField] private float chargeSpeedMultiplier = 1.0f;    //チャージ中の速度倍率

    [Header("ダメージ設定")]
    [SerializeField] private float smallDamage;             // 小チャージダメージ（敵のHPより小さく設定）
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
            ChargeLevel chargeLevel = DetermineChargeLevel(chargeTime);
            Attack(chargeLevel);
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
        _batSpriteRenderer.enabled = true;

        Debug.Log($"チャージレベル: {chargeLevel}");

        Vector2 attackDirection = _playerController.LastMoveDirection;
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * attackRange;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCenter, attackRange, enemyLayer);
        List<Collider2D> hitEnemiesInFan = new List<Collider2D>();

        foreach (Collider2D enemy in hitEnemies)
        {
            Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(attackDirection, dirToEnemy);
            if (angle <= attackAngle / 2f)
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

        StartCoroutine(HideBat());

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
                return smallDamage;


            case ChargeLevel.Medium:
                return mediumDamage;


            case ChargeLevel.Large:
                int playerLevel = ExperienceManager.Instance.PlayerLevel;
                // 自分のレベル × 倍率 以下の敵は一撃
                if (enemy.Level <= playerLevel * largeKillLevelMultiplier)
                {
                    return float.MaxValue; // 即死
                }
                else
                {
                    return mediumDamage;   // 範囲外は中チャージ相当
                }

            default:
                return smallDamage;
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

            float rad = currentAngle * Mathf.Deg2Rad;
            batObject.transform.localPosition = new Vector2(
                Mathf.Cos(rad) * attackRange,
                Mathf.Sin(rad) * attackRange
            );
            batObject.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            yield return null;
        }

        _batSpriteRenderer.enabled = false;
        _isAttacking = false;
    }

    //  ゲーム実行中にとめたらしたら攻撃範囲見えるやつ
    private void OnDrawGizmos()
    {
        if (_playerController == null) return;
        Gizmos.color = Color.red;

        Vector2 attackDirection = _playerController.LastMoveDirection;

        float halfAngle = attackAngle / 2f;
        Vector3 leftDir = Quaternion.Euler(0, 0, halfAngle) * (Vector3)attackDirection;
        Vector3 rightDir = Quaternion.Euler(0, 0, -halfAngle) * (Vector3)attackDirection;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * attackRange * 2f);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * attackRange * 2f);
    }

    private void OnDecoy(InputValue value)
    {
        Instantiate(decoyPrefab, transform.position, Quaternion.identity);
    }
}

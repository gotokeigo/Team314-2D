//-------------------------------------------------------
//
//  ShoutProjectile.cs
//
//  概要
//  プレイヤーの叫び声を制御するスクリプト
//  敵グループに当たったらAlertGroup()を呼ぶ
//
//  更新履歴
//
//  2026/05/19  作成
//
//-------------------------------------------------------
using UnityEngine;

public class ShoutProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;         // 飛ぶ速度
    [SerializeField] private float lifeTime = 3f;       // 消えるまでの時間

    private Vector2 _direction;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _direction * speed * Time.fixedDeltaTime);
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // EnemyGroupControllerを持つオブジェクトに当たったらアラート
        EnemyController enemyController = other.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            // 親のEnemyGroupControllerを探す
            EnemyGroupController group = other.GetComponentInParent<EnemyGroupController>();
            if (group != null)
            {
                group.AlertGroup();
            }
            else
            {
                // グループに属していない敵は単体で発見状態に
                enemyController.SetDiscovered(true);
            }
        }

        Destroy(gameObject);
    }
}

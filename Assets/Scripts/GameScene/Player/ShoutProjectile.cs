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
//  2026/07/06  3D対応。
//
//-------------------------------------------------------
using UnityEngine;

public class ShoutProjectile : MonoBehaviour
{
    [Tooltip("シャウトの判定が飛ぶ速度")]
    [SerializeField] private float speed = 10f;
    [Tooltip("シャウトが消えるまでの時間")]
    [SerializeField] private float lifeTime = 3f;

    private Vector3 _direction;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _direction * speed * Time.fixedDeltaTime);
    }

    public void SetDirection(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemyController = other.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            EnemyGroupController group = other.GetComponentInParent<EnemyGroupController>();
            if (group != null)
            {
                group.AlertGroup();
            }
            else
            {
                enemyController.SetDiscovered(true);
            }
        }
        Destroy(gameObject);
    }
}

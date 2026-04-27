using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private int AttackDamage;
    [SerializeField] private float attackInterval;
    private float CurrentHp;
    private float attackTimer;


    private Transform player;
    private Rigidbody2D rb;
    private PlayerHealth playerHp;
    private float stopDistance;

    void Start()
    {

        rb = GetComponent<Rigidbody2D>();

        // 自分と相手のコライダーサイズから停止距離を計算
        Collider2D enemyCol = GetComponent<Collider2D>();

        //プレイヤータグを持っているオブジェクトを探す
        GameObject playerObject = GameObject.FindWithTag("Player");


        if (playerObject != null)
        {
            //プレイヤーの座標を取得
            player = playerObject.transform;
            //プレイヤーのHpを取得
            playerHp = playerObject.GetComponent<PlayerHealth>();

            Collider2D playerCol = playerObject.GetComponent<Collider2D>();

            // 両方のコライダーの半径を合計して停止距離にする
            stopDistance = enemyCol.bounds.extents.x + playerCol.bounds.extents.x;
        }
        else
        {
            Debug.LogError("Playerタグが見つかりません！");
        }
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;
        if (playerHp != null && playerHp.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (Vector2)(player.position - transform.position);
        float distance = direction.magnitude;

        if (distance < stopDistance)
        {
            rb.MovePosition(rb.position);
            return;
        }

        Vector2 newPosition = rb.position + direction.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Debug.Log("Enemy Hit Player");

        PlayerHealth playerHp = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHp != null)
        {
            playerHp.TakeDamage(AttackDamage);
        }
    }
}
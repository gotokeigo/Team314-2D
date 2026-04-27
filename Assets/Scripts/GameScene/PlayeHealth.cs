using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHp = 10;           //最大体力
    [SerializeField] private float invincibleTime = 1;  //無敵時間

    private bool isInvincible;
    private float currentHp;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    public bool IsDead { get; private set; }


    void Start()
    {
        currentHp = maxHp;
        IsDead = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

    }

    public void TakeDamage(float damage)
    {
        if (IsDead || isInvincible) return;

        currentHp -= damage;
        Debug.Log("PlayerHP: " + currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleCoroutine());
        }
    }

    private void Die()
    {
        IsDead = true;

        Debug.Log("Player is Dead");

        // 動き停止
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // 入力停止
        GetComponent<PlayerController>().enabled = false;

        // 当たり判定オフ
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(BlinkAndDestroy());
    }

    private IEnumerator BlinkAndDestroy()
    {
        for (int i = 0; i < 4; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.2f);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.2f);
        }

        Destroy(gameObject);
    }
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        playerCollider.enabled = false;

        yield return new WaitForSeconds(invincibleTime);

        playerCollider.enabled = true;
        isInvincible = false;
    }

}

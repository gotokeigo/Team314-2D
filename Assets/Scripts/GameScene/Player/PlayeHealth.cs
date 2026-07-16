//--------------------------------------
//
//  PlayerHealth.cs
//
//  概要
//  プレイヤーのHPを制御するスクリプト
//
//  更新履歴
//
//  2026/04/27  作成
//              敵に当たった時にプレイヤーのHPが減り無敵時間を得るようにした。
//              また、プレイヤーのHPが0になった時に操作を不能にし点滅した後にプレイヤーを消すようにした
//
//  2026/07/06  3D対応。
//
//--------------------------------------
using UnityEngine;
using System.Collections;
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("プレイヤー最大HP")]
    [SerializeField] private float maxHp;
    [Tooltip("被ダメ時の無敵時間")]
    [SerializeField] private float invincibleTime;
    private bool _isInvincible;
    private float _currentHp;
    private Renderer _renderer;
    private Collider _playerCollider;
    private int _enemyLayer;
    private int _enemyKnockbackLayer;
    private PlayerController _playerController;
    [Tooltip("被弾エフェクト")]
    [SerializeField] private GameObject hitEffectPrefab;
    [Header("SE")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip hitSE;
    public bool IsDead { get; private set; }
    public float MaxHp => maxHp;
    public float CurrentHp => _currentHp;

    void Start()
    {
        _currentHp = maxHp;
        IsDead = false;
        _renderer = GetComponentInChildren<Renderer>();
        _playerCollider = GetComponentInChildren<Collider>();
        _playerController = GetComponent<PlayerController>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _enemyKnockbackLayer = LayerMask.NameToLayer("EnemyKnockback");
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || _isInvincible) return;
        _currentHp -= damage;

        // 被弾SE
        if (audioSource != null && hitSE != null)
        {
            audioSource.PlayOneShot(hitSE);
        }

        // ヒットエフェクト表示
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // 被弾モーション再生
        _playerController.PlayHitAnimation();

        if (_currentHp <= 0)
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
        ResultData.FinalLevel = ExperienceManager.Instance.PlayerLevel;
        GameTimer gameTimer = FindFirstObjectByType<GameTimer>();
        if (gameTimer != null)
        {
            ResultData.SurvivedTime = gameTimer.CurrentTime;
        }
        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // CircleWipeを呼び出す
        CircleWipe circleWipe = FindFirstObjectByType<CircleWipe>();
        if (circleWipe != null)
        {
            circleWipe.WipeOut(1.5f, () =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
            });
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
        }

        StartCoroutine(BlinkAndDestroy());
    }

    private IEnumerator BlinkAndDestroy()
    {
        for (int i = 0; i < 4; i++)
        {
            _renderer.enabled = false;
            yield return new WaitForSeconds(0.2f);
            _renderer.enabled = true;
            yield return new WaitForSeconds(0.2f);
        }


        int minutes = (int)(ResultData.SurvivedTime / 60f);
        int seconds = (int)(ResultData.SurvivedTime % 60f);

        Destroy(gameObject);
        if (minutes < 10)
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameClearScene");
    }

    private IEnumerator InvincibleCoroutine()
    {
        _isInvincible = true;
        int playerLayer = gameObject.layer;
        Physics.IgnoreLayerCollision(playerLayer, _enemyLayer, true);
        Physics.IgnoreLayerCollision(playerLayer, _enemyKnockbackLayer, true);
        yield return new WaitForSeconds(invincibleTime);
        Physics.IgnoreLayerCollision(playerLayer, _enemyLayer, false);
        Physics.IgnoreLayerCollision(playerLayer, _enemyKnockbackLayer, false);
        _isInvincible = false;
    }
}

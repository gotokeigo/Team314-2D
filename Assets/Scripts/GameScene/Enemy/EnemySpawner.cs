//--------------------------------------
//
//  EnemySpawner.cs
//
//  概要
//  プレイヤーの周囲カメラ描画外に敵グループをスポーンするスクリプト
//
//  更新履歴
//
//  2026/06/11  作成
//
//--------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    [Tooltip("スポーンエリアの半径（カメラより大きくする）")]
    [SerializeField] private float spawnAreaSize = 15f;
    [Tooltip("スポーン間隔（秒）")]
    [SerializeField] private float spawnInterval = 3f;
    [Tooltip("移動方向前方へのスポーン確率（0〜1）")]
    [SerializeField] private float forwardSpawnBias = 0.7f;

    [Header("スポーン制限")]
    [Tooltip("シーン上の敵の最大数")]
    [SerializeField] private int enemyGroupMaxCount = 30;
    [Tooltip("スポーンエリアの何倍離れたら敵を消すか")]
    [SerializeField] private float despawnDistanceMultiplier = 2f;

    // ★追加：1グループあたりの敵の数（初期値は4体）
    [Header("時間経過・集団数強化設定")]
    [Tooltip("初期の1グループあたりの敵の数")]
    [SerializeField] private int enemiesPerGroup = 4;
    private float _timeTracker = 0f;
    private float _difficultyInterval = 30f; // 30秒ごとに増加


    [Header("敵グループPrefab")]
    [Tooltip("スポーンする敵グループのPrefabリスト")]
    [SerializeField] private List<GameObject> enemyGroupPrefabs = new List<GameObject>();

    private Camera _mainCamera;
    private PlayerController _playerController;
    private float _spawnTimer;
    private int _currentEnemyCount = 0;

    private void Start()
    {
        _mainCamera = Camera.main;
        _playerController = GetComponentInParent<PlayerController>();
        _spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (enemyGroupPrefabs.Count == 0) return;

        // ★追加：30秒経ったら、グループ内の敵の数を+1する（上限はプレハブ内の最大数）
        _timeTracker += Time.deltaTime;
        if (_timeTracker >= _difficultyInterval)
        {
            enemiesPerGroup += 1; // 4 ➔ 5 ➔ 6 と増える
            Debug.Log($"30秒経過：1グループあたりの敵の数が {enemiesPerGroup} 体にアップしました！");
            _timeTracker = 0f;
        }

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;
            SpawnEnemyGroup();
        }
        DespawnFarEnemies();
    }

    private void SpawnEnemyGroup()
    {
        if (_currentEnemyCount >= enemyGroupMaxCount) return;

        Vector2 spawnPos = GetSpawnPosition();
        GameObject prefab = enemyGroupPrefabs[Random.Range(0, enemyGroupPrefabs.Count)];

        // ★追加：生成されるグループの Start() が走る直前に、数を受け渡す
        PlayerPrefs.SetInt("NextGroupSize", enemiesPerGroup);

        Instantiate(prefab, spawnPos, Quaternion.identity);

        _currentEnemyCount++;   // グループ単位でカウント
    }

    private Vector2 GetSpawnPosition()
    {
        Vector2 moveDir = _playerController != null
            ? _playerController.LastMoveDirection
            : Vector2.zero;

        if (moveDir == Vector2.zero)
        {
            return GetRandomSpawnPosition(Vector2.zero);
        }

        if (Random.value <= forwardSpawnBias)
        {
            return GetRandomSpawnPosition(moveDir);
        }
        else
        {
            return GetRandomSpawnPosition(-moveDir);
        }
    }

    private Vector2 GetRandomSpawnPosition(Vector2 biasDir)
    {
        int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomOffset;
            if (biasDir == Vector2.zero)
            {
                randomOffset = Random.insideUnitCircle * spawnAreaSize;
            }
            else
            {
                Vector2 random = Random.insideUnitCircle * spawnAreaSize;
                randomOffset = (random + biasDir * spawnAreaSize * 0.5f).normalized *
                               Random.Range(spawnAreaSize * 0.5f, spawnAreaSize);
            }

            Vector2 candidatePos = (Vector2)transform.position + randomOffset;

            // カメラ外、障害物と重なっていない、かつFieldタグの範囲内にスポーン
            if (!IsInCameraView(candidatePos) && !IsOverlappingObstacle(candidatePos) && IsInsideField(candidatePos))
            {
                return candidatePos;
            }
        }

        Vector2 fallbackDir = biasDir == Vector2.zero ? Vector2.up : -biasDir;
        return (Vector2)transform.position + fallbackDir * spawnAreaSize;
    }

    // Fieldタグのコライダー内かどうか判定
    private bool IsInsideField(Vector2 pos)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(pos);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Field"))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsOverlappingObstacle(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapCircle(pos, 0.5f, LayerMask.GetMask("Obstacle"));
        return hit != null;
    }

    private bool IsInCameraView(Vector2 worldPos)
    {
        if (_mainCamera == null) return false;
        Vector3 viewportPos = _mainCamera.WorldToViewportPoint(worldPos);
        return viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f &&
               viewportPos.z > 0f;
    }

    // EnemyControllerではなくEnemyGroupControllerで取得する
    private void DespawnFarEnemies()
    {
        float despawnDistance = spawnAreaSize * despawnDistanceMultiplier;
        EnemyGroupController[] allGroups = FindObjectsByType<EnemyGroupController>(FindObjectsSortMode.None);

        foreach (EnemyGroupController group in allGroups)
        {
            float dist = Vector2.Distance(transform.position, group.transform.position);
            if (dist > despawnDistance)
            {
                // グループ内の発見状態の敵をEnemyManagerに通知
                EnemyController[] enemies = group.GetComponentsInChildren<EnemyController>();
                foreach (EnemyController enemy in enemies)
                {
                    if (enemy.IsDiscovered)
                    {
                        EnemyManager.Instance?.NotifyLost();
                    }
                }

                _currentEnemyCount--;
                Destroy(group.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnAreaSize);
    }


}

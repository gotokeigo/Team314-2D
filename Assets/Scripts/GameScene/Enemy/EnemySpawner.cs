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
//  2026/07/06  3D対応。Vector2→Vector3、Physics2D→Physicsに変更。
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

    [Tooltip("スポーンするY座標")]
[SerializeField] private float spawnY = 0f;

    [Header("スポーン制限")]
    [Tooltip("シーン上の敵の最大数")]
    [SerializeField] private int enemyGroupMaxCount = 30;
    [Tooltip("スポーンエリアの何倍離れたら敵を消すか")]
    [SerializeField] private float despawnDistanceMultiplier = 2f;

    [Header("時間経過・集団数強化設定")]
    [Tooltip("初期の1グループあたりの敵の数")]
    [SerializeField] private int enemiesPerGroup = 4;
    [Tooltip("敵の数（人数）が増える時間間隔（秒）")] 
    [SerializeField] private float countDifficultyInterval = 30f; 
    [Tooltip("初期状態の敵の最大HP")]
    [SerializeField] private float baseEnemyMaxHp = 10f;
    [Tooltip("30秒ごとに上昇するHPの量")]
    [SerializeField] private float hpIncreaseAmount = 5f;
    [Tooltip("敵のHPが増える時間間隔（秒）")] // 
    [SerializeField] private float hpDifficultyInterval = 10f; 

    private float _currentEnemyMaxHp; // 現在の難易度に応じたHP
    private float _countTimeTracker = 0f; // 人数用のタイマーに変更
    private float _hpTimeTracker = 0f;    // HP用のタイマーに変更

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

        _currentEnemyMaxHp = baseEnemyMaxHp; // 初期HPを設定
    }

    private void Update()
    {
        // --- ① 人数アップのタイマー（30秒ごと） ---
        _countTimeTracker += Time.deltaTime;
        if (_countTimeTracker >= countDifficultyInterval)
        {
            enemiesPerGroup += 1;
            Debug.Log($"【人数アップ】{countDifficultyInterval}秒経過：1グループの数が {enemiesPerGroup} 体にアップしました！");
            _countTimeTracker = 0f;
        }

        // --- ② HPアップのタイマー（10秒ごと） ---
        _hpTimeTracker += Time.deltaTime;
        if (_hpTimeTracker >= hpDifficultyInterval)
        {
            _currentEnemyMaxHp += hpIncreaseAmount;
            Debug.Log($"【HPアップ】{hpDifficultyInterval}秒経過：敵の最大HPが {_currentEnemyMaxHp} にアップしました！");
            _hpTimeTracker = 0f;
        }

        // --- スポーン処理 ---
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

        Vector3 spawnPos = GetSpawnPosition();

        // 有効な位置が見つからなかった場合はスポーンしない
        if (float.IsInfinity(spawnPos.x)) return;

        spawnPos.y = spawnY;
        GameObject prefab = enemyGroupPrefabs[Random.Range(0, enemyGroupPrefabs.Count)];
        PlayerPrefs.SetInt("NextGroupSize", enemiesPerGroup);
        PlayerPrefs.SetFloat("NextEnemyHP", _currentEnemyMaxHp);
        Instantiate(prefab, spawnPos, Quaternion.identity);
        _currentEnemyCount++;
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 moveDir = _playerController != null
            ? _playerController.LastMoveDirection
            : Vector3.zero;

        if (moveDir == Vector3.zero)
        {
            return GetRandomSpawnPosition(Vector3.zero);
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

    private Vector3 GetRandomSpawnPosition(Vector3 biasDir)
    {
        int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomOffset;
            if (biasDir == Vector3.zero)
            {
                Vector2 circle = Random.insideUnitCircle * spawnAreaSize;
                randomOffset = new Vector3(circle.x, circle.y,0 );
            }
            else
            {
                Vector2 random = Random.insideUnitCircle * spawnAreaSize;
                Vector3 random3D = new Vector3(random.x, 0f, random.y);
                Vector3 bias3D = new Vector3(biasDir.x, 0f, biasDir.z);
                randomOffset = (random3D + bias3D * spawnAreaSize * 0.5f).normalized *
                               Random.Range(spawnAreaSize * 0.5f, spawnAreaSize);
            }

            Vector3 candidatePos = transform.position + randomOffset;

            if (!IsInCameraView(candidatePos) && !IsOverlappingObstacle(candidatePos) && IsInsideField(candidatePos))
            {
                return candidatePos;
            }
        }

        // フォールバックもFieldチェックする
        Vector3 fallbackDir = biasDir == Vector3.zero ? Vector3.forward : -biasDir;
        Vector3 fallbackPos = transform.position + fallbackDir * spawnAreaSize;

        if (IsInsideField(fallbackPos))
        {
            return fallbackPos;
        }

        return Vector3.positiveInfinity;  // フィールド外なのでスポーンしない
    }

    private bool IsInsideField(Vector3 pos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, 0.1f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Field"))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsOverlappingObstacle(Vector3 pos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, 0.5f, LayerMask.GetMask("Obstacle"));
        return hits.Length > 0;
    }

    private bool IsInCameraView(Vector3 worldPos)
    {
        if (_mainCamera == null) return false;
        Vector3 viewportPos = _mainCamera.WorldToViewportPoint(worldPos);
        return viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f &&
               viewportPos.z > 0f;
    }

    private void DespawnFarEnemies()
    {
        float despawnDistance = spawnAreaSize * despawnDistanceMultiplier;
        EnemyGroupController[] allGroups = FindObjectsByType<EnemyGroupController>(FindObjectsSortMode.None);

        foreach (EnemyGroupController group in allGroups)
        {
            float dist = Vector3.Distance(transform.position, group.transform.position);
            if (dist > despawnDistance)
            {
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

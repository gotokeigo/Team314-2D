//-------------------------------------------------------
//
//  EnemyGroupController.cs
//
//  概要
//  敵の集団を管理するスクリプト
//
//  更新履歴
//
//  2026/05/19  作成
//
//  2026/05/20  集団の数が多いほど移動速度が落ちるように
//
//-------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupController : MonoBehaviour
{
    [Tooltip("範囲内の敵の量で移動速度を変化させる")]
    [SerializeField] private float groupSlowRadius;        // この範囲内の敵の数で速度が変わる
    [Tooltip("最大減速倍率")]
    [SerializeField] private float minSpeedMultiplier;   // 最大減速倍率（固まった時）
    [Tooltip("最大減速になる敵の数")]
    [SerializeField] private int maxGroupSizeForSlow;       // この数以上固まると最大減速
    [Tooltip("敵グループの未発見時歩き回る範囲")]
    [SerializeField] private float groupWanderRadius = 5f;      // グループの徘徊範囲
    [Tooltip("敵グループの次の目標地点を決める間隔")]
    [SerializeField] private float groupWanderInterval = 3f;    // 次の目標地点を決める間隔

    // --- [Git追記] グループ合流・UI設定用の変数 ---
    [Header("グループ合流・UI設定")]
    [SerializeField] private float mergeRadius = 4f;
    [SerializeField] private TMPro.TextMeshProUGUI countText;
    public List<EnemyController> Enemies => _enemies;
    public bool IsDiscovered => _isDiscovered;
    // --------------------------------------------

    private Vector2 _groupWanderTarget;                         // グループ共通の目標地点
    private float _groupWanderTimer;                            // 徘徊タイマー
    private Vector2 _groupOrigin;                               // グループの初期位置

    private bool _isDiscovered;
    private List<EnemyController> _enemies = new List<EnemyController>();

    private void Start()
    {
        foreach (EnemyController enemy in GetComponentsInChildren<EnemyController>())
        {
            _enemies.Add(enemy);
        }

        // グループの初期位置を記録して最初の目標地点を決める
        _groupOrigin = transform.position;
        _groupWanderTarget = GetNewGroupWanderTarget();
    }

    private void Update()
    {
        _enemies.RemoveAll(e => e == null);

        // --- [Git追記] UIの更新処理 ---
        if (countText != null)
        {
            if (_enemies.Count > 0)
            {
                countText.text = _enemies.Count.ToString();
                countText.transform.position = CalcGroupCenter() + new Vector2(0, 1.5f);
            }
            else countText.text = "";
        }
        // ----------------------------

        if (_enemies.Count == 0) return;

        // --- [Git追記] 近くの別グループを吸収して合流 --
        TryMergeWithNearbyGroups();
        //----------------------------

        if (!_isDiscovered)
        {
            foreach (EnemyController enemy in _enemies)
            {
                if (enemy.IsDiscovered)
                {
                    DiscoverAll();
                    break;
                }
            }
        }

        // 未発見時はグループ共通の目標地点に向かって徘徊
        if (!_isDiscovered)
        {
            _groupWanderTimer -= Time.deltaTime;
            if (_groupWanderTimer <= 0f)
            {
                _groupWanderTarget = GetNewGroupWanderTarget();
            }

            // グループ全員に同じ目標地点を伝える
            foreach (EnemyController enemy in _enemies)
            {
                enemy.SetWanderTarget(_groupWanderTarget);
            }
        }

        foreach (EnemyController enemy in _enemies)
        {
            int nearbyCount = CountNearbyEnemies(enemy);
            float speedMultiplier = CalcSpeedMultiplier(nearbyCount);
            enemy.SetMoveSpeed(enemy.GetBaseSpeed() * speedMultiplier);
        }

        // ==================== 【ここを書き換えます】 ====================
        Vector2 groupCenter = CalcGroupCenter(); // グループの現在の中心点

        foreach (EnemyController enemy in _enemies)
        {
            if (enemy == null) continue;

            // 敵から見たグループの中心点への方向と距離
            Vector2 toCenter = groupCenter - (Vector2)enemy.transform.position;
            float distanceToCenter = toCenter.magnitude;

            // 敵同士の隙間が 2.0 ユニット以上開いてバラバラになっている場合
            if (distanceToCenter > 2.0f)
            {
                // 💡無理やり位置を動かすのをやめて、敵のAIの「目標地点（WanderTarget）」を
                // 一時的に「グループの中心（groupCenter）」に書き換えて、そっちに向かって歩かせます！
                enemy.SetWanderTarget(groupCenter);
            }
        }
        // ================================================================

    }

    // グループの目標地点をランダムに決める
    private Vector2 GetNewGroupWanderTarget()
    {
        _groupWanderTimer = groupWanderInterval;
        return _groupOrigin + Random.insideUnitCircle * groupWanderRadius;
    }

    // グループ全員をプレイヤー発見状態にする
    private void DiscoverAll()
    {
        Debug.Log("敵グループ:発見！");
        _isDiscovered = true;
        foreach (EnemyController enemy in _enemies)
        {
            enemy.SetDiscovered(true);
        }
    }

    // 指定した敵の周囲にいる敵の数を数える
    private int CountNearbyEnemies(EnemyController target)
    {
        int count = 0;
        foreach (EnemyController enemy in _enemies)
        {
            if (enemy == target) continue;
            float dist = Vector2.Distance(
                target.transform.position,
                enemy.transform.position
            );
            if (dist <= groupSlowRadius) count++;
        }
        return count;
    }

    // 周囲の敵の数から速度倍率を計算
    private float CalcSpeedMultiplier(int nearbyCount)
    {
        if (nearbyCount <= 0) return 1f;
        float t = Mathf.Clamp01((float)nearbyCount / maxGroupSizeForSlow);
        return Mathf.Lerp(1f, minSpeedMultiplier, t);
    }

    // 外部からプレイヤーに気づかせる（デコイなどから呼ぶ）
    public void AlertGroup()
    {
        DiscoverAll();
    }

    // --- [Git追記] 新規追加メソッド群 ---
    private void TryMergeWithNearbyGroups()
    {
        EnemyGroupController[] allGroups = FindObjectsByType<EnemyGroupController>(FindObjectsSortMode.None);
        foreach (EnemyGroupController otherGroup in allGroups)
        {
            if (otherGroup == this || otherGroup.Enemies.Count == 0) continue;

            float dist = Vector2.Distance(CalcGroupCenter(), otherGroup.CalcGroupCenter());
            if (dist <= mergeRadius)
            {
                if (otherGroup.IsDiscovered && !_isDiscovered)
                {
                    DiscoverAll();
                }

                foreach (EnemyController enemy in otherGroup.Enemies)
                {
                    if (enemy == null) continue;
                    enemy.transform.SetParent(this.transform);
                    _enemies.Add(enemy);
                }

                otherGroup.Enemies.Clear();
                // 吸収された側のグループが、本当に空っぽ（敵が0体）なら非表示にする
                if (otherGroup.Enemies.Count == 0)
                {
                    otherGroup.gameObject.SetActive(false);
                }
            }
        }
    }

    public Vector2 CalcGroupCenter()
    {
        if (_enemies.Count == 0) return transform.position;
        Vector2 sumPosition = Vector2.zero;
        int validCount = 0;
        foreach (EnemyController enemy in _enemies)
        {
            if (enemy != null)
            {
                if (validCount > 0)
                {
                    float distFromCurrentAverage = Vector2.Distance(sumPosition / validCount, enemy.transform.position);
                    if (distFromCurrentAverage > 10f) continue; // ふっとんだ敵は無視して次の敵へ
                }

                sumPosition += (Vector2)enemy.transform.position;
                validCount++;
            }
        }
        return validCount > 0 ? sumPosition / validCount : (Vector2)transform.position;
    }
    // ------------------------------------

}

//-------------------------------------------------------
//
//  EnemyGroupController.cs
//
//  概要
//  敵の集団を管理するプログラム
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
    [SerializeField] private float groupSlowRadius = 3f;        // この範囲内の敵の数で速度が変わる
    [SerializeField] private float minSpeedMultiplier;   // 最大減速倍率（固まった時）
    [SerializeField] private int maxGroupSizeForSlow = 5;       // この数以上固まると最大減速
    [SerializeField] private float groupWanderRadius = 5f;      // グループの徘徊範囲
    [SerializeField] private float groupWanderInterval = 3f;    // 次の目標地点を決める間隔

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

        // 追加：グループの初期位置を記録して最初の目標地点を決める
        _groupOrigin = transform.position;
        _groupWanderTarget = GetNewGroupWanderTarget();
    }

    private void Update()
    {
        _enemies.RemoveAll(e => e == null);
        if (_enemies.Count == 0) return;

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

        // 追加：未発見時はグループ共通の目標地点に向かって徘徊
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
    }

    // 追加：グループの目標地点をランダムに決める
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
}

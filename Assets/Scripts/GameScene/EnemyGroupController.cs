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

    private bool _isDiscovered;
    private List<EnemyController> _enemies = new List<EnemyController>();

    private void Start()
    {
        // 子オブジェクトの敵を全員登録
        foreach (EnemyController enemy in GetComponentsInChildren<EnemyController>())
        {
            _enemies.Add(enemy);
        }
    }

    private void Update()
    {
        // 死亡したEnemyをリストから削除
        _enemies.RemoveAll(e => e == null);

        if (_enemies.Count == 0) return;

        // 1体でも発見したらグループ全員に伝える
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

        // 各敵の速度をグループの密集度に応じて調整
        foreach (EnemyController enemy in _enemies)
        {
            int nearbyCount = CountNearbyEnemies(enemy);
            float speedMultiplier = CalcSpeedMultiplier(nearbyCount);
            enemy.SetMoveSpeed(enemy.GetBaseSpeed() * speedMultiplier);
        }
    }

    // グループ全員をプレイヤー発見状態にする
    private void DiscoverAll()
    {
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

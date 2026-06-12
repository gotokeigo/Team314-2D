//--------------------------------------
//
//  EnemyManager.cs
//
//  概要
//  発見状態の敵の数を管理するシングルトン
//
//  更新履歴
//
//  2026/06/04  作成
//
//--------------------------------------
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private int _discoveredEnemyCount = 0;
    public int DiscoveredEnemyCount => _discoveredEnemyCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void NotifyDiscovered()
    {
        _discoveredEnemyCount++;
    }

    public void NotifyLost()
    {
        _discoveredEnemyCount = Mathf.Max(0, _discoveredEnemyCount - 1);
    }
}

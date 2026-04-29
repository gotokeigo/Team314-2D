//--------------------------------------
//
//  ExperienceManager.cs
//
//  概要
//  プレイヤー経験値を制御するスクリプト
//
//  更新履歴
//
//  2026/04/28  作成  
//
//
//--------------------------------------
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] private float baseXpPerEnemy = 10f;  // 1体あたりの基本XP
    [SerializeField] private float multiHitBonus = 1.5f;  // 複数ヒット時のボーナス倍率
    private float totalXp;

    private void Awake()
    {
        // シングルトン
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXp(int hitCount)
    {
        float xp;
        if (hitCount <= 1)
        {
            xp = baseXpPerEnemy;
        }
        else
        {
            // 複数ヒットでボーナス
            xp = baseXpPerEnemy * (hitCount * multiHitBonus);
        }

        totalXp += xp;
        Debug.Log($"{hitCount}体ヒット！ +{xp}XP / 合計:{totalXp}XP");
    }
}

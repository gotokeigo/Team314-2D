//--------------------------------------
//
//  ExperienceManager.cs
//
//  概要
//  プレイヤー経験値を制御するスクリプト
//
//  更新履歴
//
//  2026/04/28  作成　経験値計算の処理を実装
//
//
//--------------------------------------
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] private float baseXpPerEnemy = 10f;  // 1体あたりの基本XP
    [SerializeField] private float multiHitBonusPerEnemy = 0.6f;  //    複数ヒットした時の計算処理に使用

    private float totalXp;

    private void Awake()
    {
        // シングルトン
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXp(int hitCount)
    {

        float xp = baseXpPerEnemy * (1.0f + (hitCount - 1) * multiHitBonusPerEnemy);    
        totalXp += xp;
        Debug.Log($"{hitCount}体ヒット！ +{xp}XP / 合計:{totalXp}XP");
    }
}

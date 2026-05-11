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
//  2026/05/10  プレイヤーレベルを追加
//
//--------------------------------------
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] private float baseXpPerEnemy;          // 1体あたりの基本XP
    [SerializeField] private float multiHitBonusPerEnemy;   // 複数ヒット時の補正
    [SerializeField] private float xpPerLevel;              // レベル1→2に必要な基準XP
    [SerializeField] private float levelScaling;            // レベルアップに必要なXPの増加率

    public int PlayerLevel { get; private set; } = 1;
    private float totalXp;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXp(int hitCount)
    {
        float xp = baseXpPerEnemy * (1.0f + (hitCount - 1) * multiHitBonusPerEnemy);
        totalXp += xp;
        Debug.Log($"{hitCount}体ヒット！ +{xp}XP / 合計:{totalXp}XP");

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        float required = XpRequiredForNextLevel();
        while (totalXp >= required)
        {
            totalXp -= required;
            PlayerLevel++;
            Debug.Log($"レベルアップ！ 現在レベル: {PlayerLevel}");
            required = XpRequiredForNextLevel();
        }
    }

    // 次のレベルに必要なXP（レベルが上がるほど多くなる）
    private float XpRequiredForNextLevel()
    {
        return xpPerLevel * Mathf.Pow(PlayerLevel, levelScaling);
    }
}

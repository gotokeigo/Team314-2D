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
//  2026/06/08  経験値制度を廃止。
//              1体倒すと1レベルアップ。
//              複数体同時に倒した場合はフィボナッチ数列に基づいてレベルアップ量が増える。
//
//  2026/06/11  経験値システムを再実装。
//              1体倒したときのベースXP、複数体撃破時の倍率、
//              レベルアップ必要XP、レベルスケーリングをInspectorで設定可能に。
//
//--------------------------------------
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Tooltip("1体倒したときのベースXP")]
    [SerializeField] private float baseXpPerKill = 10f;
    [Tooltip("複数体同時撃破時の1体あたりの追加倍率（0.1で1体増えるごとに10%増加）")]
    [SerializeField] private float multiKillBonusRate = 0.1f;
    [Tooltip("レベル1→2に必要な基準XP")]
    [SerializeField] private float xpPerLevel = 100f;
    [Tooltip("レベルが上がるほど必要XPが増える割合（1.0で線形、2.0で2乗）")]
    [SerializeField] private float levelScaling = 1.5f;

    [Header("攻撃力設定")]
    [Tooltip("レベル1の基本攻撃力")]
    [SerializeField] private float baseAttackPower = 10f;
    [Tooltip("レベルアップごとの攻撃力上昇量")]
    [SerializeField] private float attackPowerPerLevel = 5f;

    // 現在の攻撃力を返す
    public float PlayerAttackPower => baseAttackPower + (PlayerLevel - 1) * attackPowerPerLevel;

    public int PlayerLevel { get; private set; } = 1;
    public float CurrentXp => _totalXp;
    public float NextLevelXp => XpRequiredForNextLevel();
    private float _totalXp;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXp(int killCount)
    {
        // 複数体同時撃破でボーナス
        // 例: 1体=10XP, 2体=21XP, 3体=33XP (multiKillBonusRate=0.1の場合)
        float bonus = 1f + (killCount - 1) * multiKillBonusRate;
        float xp = baseXpPerKill * killCount * bonus;
        _totalXp += xp;

        Debug.Log($"{killCount}体撃破！ +{xp}XP / 合計:{_totalXp}XP");

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        float required = XpRequiredForNextLevel();
        while (_totalXp >= required)
        {
            _totalXp -= required;
            PlayerLevel++;
            Debug.Log($"レベルアップ！ 現在レベル: {PlayerLevel}");
            required = XpRequiredForNextLevel();
        }
    }

    // レベルが上がるほど必要XPが増える
    // levelScaling=1.0: 毎レベル同じXP
    // levelScaling=1.5: レベルが上がるほどきつくなる
    // levelScaling=2.0: レベルの2乗で増加
    private float XpRequiredForNextLevel()
    {
        return xpPerLevel * Mathf.Pow(PlayerLevel, levelScaling);
    }
}

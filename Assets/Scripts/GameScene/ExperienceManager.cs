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
//--------------------------------------
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    public int PlayerLevel { get; private set; } = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXp(int killCount)
    {
        int levelUp = FibonacciAt(killCount);
        PlayerLevel += levelUp;
    }

    // フィボナッチ数列のn番目を返す
    // 1体=1, 2体=1, 3体=2, 4体=3, 5体=5 ...
    private int FibonacciAt(int n)
    {
        if (n <= 2) return 1;
        int a = 1, b = 1;
        for (int i = 2; i < n; i++)
        {
            int temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }
}

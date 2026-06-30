//--------------------------------------
//
//  ResultUI.cs
//
//  概要
//  リザルト画面に最終レベルと生存時間を表示するスクリプト
//
//  更新履歴
//
//  2026/06/30  作成
//
//--------------------------------------
using UnityEngine;
using TMPro;
using System.Security;

public class ResultUI: MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void Start()
    {
        int minutes = (int)(ResultData.SurvivedTime / 60f);
        int seconds = (int)(ResultData.SurvivedTime % 60f);

        levelText.text = $"LastLevel: {ResultData.FinalLevel}";
        timeText.text = $"SurvivalTime: {minutes:00}:{seconds:00}";
    }
}

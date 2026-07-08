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
//  2026/07/08  スプライトフォントで数字表示に変更。レベル4桁対応。
//
//--------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("レベル表示（4桁）")]
    [SerializeField] private Image levelDigit1;     // 千の位
    [SerializeField] private Image levelDigit2;     // 百の位
    [SerializeField] private Image levelDigit3;     // 十の位
    [SerializeField] private Image levelDigit4;     // 一の位

    [Header("タイム表示")]
    [SerializeField] private Image minute10;
    [SerializeField] private Image minute1;
    [SerializeField] private Image second10;
    [SerializeField] private Image second1;

    [Header("スプライト")]
    [SerializeField] private Sprite[] numberSprites;

    private void Start()
    {
        int level = ResultData.FinalLevel;
        int minutes = (int)(ResultData.SurvivedTime / 60f);
        int seconds = (int)(ResultData.SurvivedTime % 60f);

        // レベルの桁数に応じて非表示にする
        levelDigit1.gameObject.SetActive(level >= 1000);
        levelDigit2.gameObject.SetActive(level >= 100);
        levelDigit3.gameObject.SetActive(level >= 10);

        SetNumber(levelDigit1, (level / 1000) % 10);
        SetNumber(levelDigit2, (level / 100) % 10);
        SetNumber(levelDigit3, (level / 10) % 10);
        SetNumber(levelDigit4, level % 10);

        SetNumber(minute10, minutes / 10);
        SetNumber(minute1, minutes % 10);
        SetNumber(second10, seconds / 10);
        SetNumber(second1, seconds % 10);
    }

    private void SetNumber(Image image, int number)
    {
        if (image == null) return;
        if (numberSprites == null || numberSprites.Length < 10) return;
        image.sprite = numberSprites[Mathf.Clamp(number, 0, 9)];
    }
}

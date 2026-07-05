//--------------------------------------
//
//  OptionUI.cs
//
//  概要
//  オプションパネルの表示・音量・マウスの速度調整
//
//  更新履歴
//
//  2026/07/04  作成  
//
//--------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionUI : MonoBehaviour
{
    [Header("オプションパネル")]
    [Tooltip("オプションパネルのゲームオブジェクト")]
    [SerializeField] private GameObject optionPanel;

    [Header("BGM設定")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TextMeshProUGUI bgmValueText;

    [Header("SE設定")]
    [SerializeField] private Slider seSlider;
    [SerializeField] private TextMeshProUGUI seValueText;


    private void Start()
    {
        optionPanel.SetActive(false);

        // まずリスナーを全部外す
        bgmSlider.onValueChanged.RemoveAllListeners();
        seSlider.onValueChanged.RemoveAllListeners();

        // リスナーなしの状態で値を設定
        bgmSlider.minValue = 0;
        bgmSlider.maxValue = 100;
        bgmSlider.wholeNumbers = true;
        bgmSlider.value = AudioManager.Instance.BgmVolume;

        seSlider.minValue = 0;
        seSlider.maxValue = 100;
        seSlider.wholeNumbers = true;
        seSlider.value = AudioManager.Instance.SeVolume;

        UpdateBgmText((int)bgmSlider.value);
        UpdateSeText((int)seSlider.value);

        // 値設定後にリスナーを登録
        bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        seSlider.onValueChanged.AddListener(OnSeSliderChanged);
    }

    // オプションボタンを押したら呼ぶ
    public void OpenOption()
    {
        optionPanel.SetActive(true);
    }

    // 閉じるボタンを押したら呼ぶ
    public void CloseOption()
    {
        optionPanel.SetActive(false);
    }

    private void OnBgmSliderChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value / 10f) * 10;  // 10刻みに丸める
        bgmSlider.value = intValue;
        AudioManager.Instance.SetBgmVolume(intValue);
        UpdateBgmText(intValue);
    }

    private void OnSeSliderChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value / 10f) * 10;
        seSlider.value = intValue;
        AudioManager.Instance.SetSeVolume(intValue);
        UpdateSeText(intValue);
    }

    private void UpdateBgmText(int value)
    {
        if (bgmValueText != null)
            bgmValueText.text = value.ToString();
    }

    private void UpdateSeText(int value)
    {
        if (seValueText != null)
            seValueText.text = value.ToString();
    }


}

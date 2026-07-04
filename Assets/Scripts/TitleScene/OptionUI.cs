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

    //[Header("マウス速度設定")]
    //[SerializeField] private Slider mouseSpeedSlider;
    //[SerializeField] private TextMeshProUGUI mouseSpeedValueText;

//    private float _defaultMouseSensitivity = 1f;

    private void Start()
    {
        optionPanel.SetActive(false);

        // スライダーの設定
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

        // スライダーのイベント登録
        bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        seSlider.onValueChanged.AddListener(OnSeSliderChanged);

        // マウス速度ボタンのイベント登録
        //mouseSpeedSlider.minValue = 0;
        //mouseSpeedSlider.maxValue = 2;
        //mouseSpeedSlider.wholeNumbers = true;
        //mouseSpeedSlider.value = 1;     // デフォルトは通常（真ん中）
        //UpdateMouseSpeedText((int)mouseSpeedSlider.value);

        //mouseSpeedSlider.onValueChanged.AddListener(OnMouseSpeedSliderChanged);
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

    //---
    //マウスカーソル速度のオプションはカーソルを仮想カーソルで扱うものにしないといけないため保留
    //---
    //private void OnMouseSpeedSliderChanged(float value)
    //{
    //    float multiplier = value switch
    //    {
    //        0 => 0.5f,  // 1/2倍
    //        1 => 1.0f,  // 通常
    //        2 => 2.0f,  // 2倍
    //        _ => 1.0f
    //    };
    //    PlayerPrefs.SetFloat("MouseSpeedMultiplier", multiplier);
    //    PlayerPrefs.Save();
    //    UpdateMouseSpeedText((int)value);
    //}

    //private void UpdateMouseSpeedText(int value)
    //{
    //    if (mouseSpeedValueText == null) return;
    //    mouseSpeedValueText.text = value switch
    //    {
    //        0 => "x0.5",
    //        1 => "x1.0",
    //        2 => "x2.0",
    //        _ => "x1.0"
    //    };
    //}
}

//-------------------------------------------------------
//
//  StatusUI.cs
//
//  概要
//  ステータスのUIを管理するスクリプト
//
//  更新履歴
//
//  2026/06/04  作成
//  2026/06/08  プレイヤーのレベル表示を追加
//  2026/06/16  プレイヤーアタックのチャージレベルを継承してUIに表示するように変更
//-------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUI : MonoBehaviour
{
    [Tooltip("プレイヤーHP参照用")]
    [SerializeField] private PlayerHealth playerHealth; // プレイヤーのHealthスクリプト
    [Tooltip("体力表示に使う画像をアタッチする")]
    [SerializeField] private Sprite heartSprite;        // ♡の画像をアタッチする
    [Tooltip("体力に使うハートを並べる位置")]
    [SerializeField] private Transform heartsPanel;     // ♡を並べる親オブジェクト
    [Tooltip("体力表示に使うハートの大きさ")]
    [SerializeField] private Vector2 heartSize = new Vector2(30.0f, 30.0f); //♡のサイズ
   // [Tooltip("プレイヤーのレベル表示に使うtextmeshの場所をアタッチ")]
  //  [SerializeField] private TextMeshProUGUI levelText; // インスペクターでアタッチ
    [Tooltip("プレイヤーのチャージレベル表示に使うtextmeshの場所をアタッチ")]
    [SerializeField] private TextMeshProUGUI chargeLevelText;
    [Tooltip("PlayerAttackスクリプトの参照")]
    [SerializeField] private PlayerAttack playerAttack;
    [Tooltip("レベル1000の位")]
    [SerializeField] private Image level1000;
    [Tooltip("レベル100の位")]
    [SerializeField] private Image level100;
    [Tooltip("レベル10の位")]
    [SerializeField] private Image level10;
    [Tooltip("レベル1の位")]
    [SerializeField] private Image level1;
    [Tooltip("数字のスプライト(0～9)")]
    [SerializeField] private Sprite[] numberSprites;

    private List<Image> _heartImages = new List<Image>();
    private int _maxHp;
    private int _lastLevel = -1;    // 前フレームのレベル保存用


    void Start()
    {
        _maxHp = (int)playerHealth.MaxHp;
        CreateHearts();
        UpdateLevelText();
    }

    void Update()
    {
        UpdateHearts();

        // レベルが変わった時だけ更新
        int currentLevel = ExperienceManager.Instance.PlayerLevel;
        if (currentLevel != _lastLevel)
        {
            UpdateLevelText();
        }


    }


     //♡を最大HP分生成
    private void CreateHearts()
    {
        for (int i = 0; i < _maxHp; i++)
        {
            // 四角形のImaege生成
            GameObject heart = new GameObject($"Heart_{i}");
            heart.transform.SetParent(heartsPanel, false);

            Image image = heart.AddComponent<Image>();
            image.sprite = heartSprite;
            image.color = Color.white;

            // サイズ設定
            RectTransform rect = heart.GetComponent<RectTransform>();
            rect.sizeDelta = heartSize;

            _heartImages.Add(image);
        }
    }



    // 現在HPに応じて♡の表示を更新
    private void UpdateHearts()
    {
        if (playerHealth == null) return;

        int currentHp = (int)playerHealth.CurrentHp;

        for (int i = 0; i < _heartImages.Count; i++)
        {
            // 現在HP以下のインデックスは表示、それ以外は非表示
            _heartImages[i].enabled = i < currentHp;
        }
    }

    //レベル表示を更新
    //private void UpdateLevelText()
    //{
    //    if (levelText == null) return;

    //    _lastLevel = ExperienceManager.Instance.PlayerLevel;
    //    levelText.text = $"PlayerLv:{_lastLevel}";
    //}
    private void UpdateLevelText()
    {
        _lastLevel = ExperienceManager.Instance.PlayerLevel;

        level1000.sprite = numberSprites[(_lastLevel / 1000) % 10];
        level100.sprite = numberSprites[(_lastLevel / 100) % 10];
        level10.sprite = numberSprites[(_lastLevel / 10) % 10];
        level1.sprite = numberSprites[_lastLevel % 10];
    }


}


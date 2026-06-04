//-------------------------------------------------------
//
//  PlayerHealthUI.cs
//
//  概要
//  プレイヤーの体力をUIに♡として表示するスクリプト
//
//  更新履歴
//
//  2026/06/04  作成
//
//-------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth; // プレイヤーのHealthスクリプト
    [SerializeField] private Sprite heartSprite;        // ♡の画像をアタッチする
    [SerializeField] private Transform heartsPanel;     // ♡を並べる親オブジェクト
    [SerializeField] private Vector2 heartSize = new Vector2(30.0f, 30.0f); //♡のサイズ

    private List<Image> _heartImages = new List<Image>();
    private int _maxHp;

    void Start()
    {
        _maxHp = (int)playerHealth.MaxHp;
        CreateHearts();
    }

    void Update()
    {
        UpdateHearts();
    }

    // ♡を最大HP分生成
    private void CreateHearts()
    {
        for (int i = 0; i < _maxHp; i++)
        {
            // 四角形のImaege生成
            GameObject heart = new GameObject($"Heart_{i}");
            heart.transform.SetParent(heartsPanel, false);

            Image image = heart.AddComponent<Image>();
            image.sprite = heartSprite;
            image.color = Color.white;    //後で♡画像に差し替える

            // サイズ設定
            RectTransform rect = heart.GetComponent<RectTransform>();
            rect.sizeDelta = heartSize;

            _heartImages.Add(image);
        }
    }

    // 現在HPに応じて♡の表示を更新
    private void UpdateHearts()
    {
        int currentHp = (int)playerHealth.CurrentHp;

        for(int i = 0; i < _heartImages.Count; i++)
        {
            //現在HP以下のインデックスは赤、それ以外はグレー
            _heartImages[i].color = i < currentHp ? Color.red : Color.gray;
        }
    }
}


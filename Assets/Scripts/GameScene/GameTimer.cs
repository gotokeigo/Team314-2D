//-------------------------------------------------------
//
//  GameTimer.cs
//
//  概要
//  ゲームのタイマーを管理するスクリプト
//
//  更新履歴
//
//  2026/06/02  作成  DebugLogに時間表示
//  2026/06/08  DebugLogに表示するのをやめTextMeshProに表示するように変更
//  2026/06/11  ボス出現メッセージをタイマーの2行目に表示。
//              ボス撃破時にメッセージを消すように変更。
//
//-------------------------------------------------------
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [Tooltip("目標時間(秒)")]
    [SerializeField] private float targetTime = 60f;
    [Tooltip("タイマー表示に使うtextmeshの場所をアタッチ")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image TimerFrame;
    [SerializeField] private Image minute10;
    [SerializeField] private Image minute1;
    [SerializeField] private Image second10;
    [SerializeField] private Image second1;

    [SerializeField] private Image colon;
    [SerializeField] private Image colon2;

    [SerializeField] private Sprite[] numberSprites;
    [SerializeField] private GameObject bossPrefab;

    private float _currentTime;
    private bool _isFinished;
    private bool _bossSpawned;
    private bool _bossMessage;

    public float CurrentTime => _currentTime;

    private void Start()
    {
        _currentTime = 0f;
        _isFinished = false;
        _bossSpawned = false;
        _bossMessage = false;
    }

    private void Update()
    {
        if (_isFinished) return;
        _currentTime += Time.deltaTime;

        int minutes = (int)(_currentTime / 60f);
        int seconds = (int)(_currentTime % 60f);

        //if (timerText != null)
        //{
        //    // ボス出現中は2行目にメッセージ表示
        //    if (_bossMessage)
        //    {
        //        timerText.text = $"{minutes:00}:{seconds:00}\nbossappeared";
        //    }
        //    else
        //    {
        //        timerText.text = $"{minutes:00}:{seconds:00}";
        //    }
        //}
        if (numberSprites.Length >= 10)
        {
            minute10.sprite = numberSprites[minutes / 10];
            minute1.sprite = numberSprites[minutes % 10];

            second10.sprite = numberSprites[seconds / 10];
            second1.sprite = numberSprites[seconds % 10];
        }

        if (_currentTime >= targetTime)
        {
            OnTimeUp();
        }
    }

    private void OnTimeUp()
    {
        if (bossPrefab != null && _bossSpawned == false)
        {
            Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
            _bossSpawned = true;
            _bossMessage = true;
            Debug.Log("ボスが出現した！");
        }
    }

    // BossControllerから呼ばれる
    public void OnBossDefeated()
    {
        _bossMessage = false;   // メッセージを消す
        Debug.Log("ボスを倒した！");
    }
}

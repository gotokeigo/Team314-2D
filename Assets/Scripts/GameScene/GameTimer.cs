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
    [Tooltip("最初のボス出現時間(秒)")]
    [SerializeField] private float targetTime = 60f;
    [Tooltip("2回目以降のボス出現間隔(秒)")]
    [SerializeField] private float bossSpawnInterval = 60f;
    [Tooltip("ボスのスポーン座標")]
    [SerializeField] private Vector3 bossSpawnPosition = Vector3.zero; 
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
    private float _nextBossSpawnTime;   //次にボスが湧く時間

    public float CurrentTime => _currentTime;

    private void Start()
    {
        _currentTime = 0f;
        _isFinished = false;
        _bossSpawned = false;
        _nextBossSpawnTime = targetTime;    //最初の出現時間をリセット
    }

    private void Update()
    {
        if (_isFinished) return;
        _currentTime += Time.deltaTime;

        int minutes = (int)(_currentTime / 60f);
        int seconds = (int)(_currentTime % 60f);


        if (numberSprites.Length >= 10)
        {
            minute10.sprite = numberSprites[minutes / 10];
            minute1.sprite = numberSprites[minutes % 10];

            second10.sprite = numberSprites[seconds / 10];
            second1.sprite = numberSprites[seconds % 10];
        }

        if (_currentTime >= _nextBossSpawnTime)
        {
            OnTimeUp();
            _nextBossSpawnTime += bossSpawnInterval;    //次の出現時間を更新
        }
    }

    private void OnTimeUp()
    {
        if (bossPrefab != null && _bossSpawned == false)
        {
            Instantiate(bossPrefab,bossSpawnPosition, Quaternion.identity);
            _bossSpawned = true;
        }
    }

    // BossControllerから呼ばれる
    public void OnBossDefeated()
    {
        Debug.Log("ボスを倒した！");
        _bossSpawned = false;
    }
}

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

public class GameTimer : MonoBehaviour
{
    [Tooltip("目標時間(秒)")]
    [SerializeField] private float targetTime = 60f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject bossPrefab;

    private float _currentTime;
    private bool _isFinished;
    private bool _bossSpawned;

    public float CurrentTime => _currentTime;

    private void Start()
    {
        _currentTime = 0f;
        _isFinished = false;
        _bossSpawned = false;
    }

    private void Update()
    {
        if (_isFinished) return;
        _currentTime += Time.deltaTime;

        int minutes = (int)(_currentTime / 60f);
        int seconds = (int)(_currentTime % 60f);

        if (timerText != null)
        {
            // ボス出現中は2行目にメッセージ表示
            if (_bossSpawned)
            {
                timerText.text = $"{minutes:00}:{seconds:00}\nbossappeared";
            }
            else
            {
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        if (_currentTime >= targetTime)
        {
            OnTimeUp();
        }
    }

    private void OnTimeUp()
    {
        if (bossPrefab != null && !_bossSpawned)
        {
            Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
            _bossSpawned = true;
            Debug.Log("ボスが出現した！");
        }
    }

    // BossControllerから呼ばれる
    public void OnBossDefeated()
    {
        _bossSpawned = false;   // メッセージを消す
        Debug.Log("ボスを倒した！");
    }
}

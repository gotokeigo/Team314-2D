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
//
//-------------------------------------------------------
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float targetTime = 60f;    // 目標時間（秒）
    //[SerializeField] private GameObject bossPrefab;     // ボスのPrefab

    private float _currentTime;
    private bool _isFinished;

    public float CurrentTime => _currentTime;

    private void Start()
    {
        _currentTime = 0f;
        _isFinished = false;
    }

    private void Update()
    {
        if (_isFinished) return;

        _currentTime += Time.deltaTime;

        // 経過時間をDebug.Logで表示
        int minutes = (int)(_currentTime / 60f);
        int seconds = (int)(_currentTime % 60f);
        Debug.Log($"経過時間: {minutes:00}:{seconds:00}");

        if (_currentTime >= targetTime)
        {
            OnTimeUp();
        }
    }

    private void OnTimeUp()
    {
        Debug.Log("ボスが出現した！");

        //if (bossPrefab != null)
        //{
        //    Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
        //}
    }
}

//-------------------------------------------------------
//
//  ResultManager.cs
//
//  概要
//  リザルトでUIをクリックした後の処理を行うスクリプト。
//
//  更新履歴
//
//  2026/06/30  作成
//  2026/07/08  SE再生後にシーン遷移するように変更
//
//-------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Tooltip("ボタン選択時のSE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioClip buttonSE;
    [SerializeField] private AudioClip ResultBGM;

    [SerializeField] private Fade fade;


    private bool _isTransitioning = false;

    private void Start()
    {
        PlayResultBGM();
        fade.FadeIn(1.0f);
    }

    public void OnClickBackTitle()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        PlayButtonSE();
        fade.FadeOut(0.6f, () =>
        {
            SceneManager.LoadScene("TitleScene");
        });
    }

    public void OnClickRetry()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        PlayButtonSE();
        fade.FadeOut(0.6f, () =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }



    private void PlayButtonSE()
    {
        if (audioSource != null && buttonSE != null)
        {
            audioSource.PlayOneShot(buttonSE);
        }
    }
    private void PlayResultBGM()
    {
        if (bgmAudioSource != null && ResultBGM != null)
        {
            bgmAudioSource.clip = ResultBGM;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }
}

//-------------------------------------------------------
//
//  TitleManager.cs
//
//  概要
/// タイトルでUIをクリックした後の処理を行うスクリプト。
//
//  更新履歴
//
//  2026/06/28  作成
//  2026/07/04  ボタン選択時のSE再生を追加
//  2026/07/08  SE再生中は入力を受け付けないように変更
//
//-------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Tooltip("オプションパネルのGameObject")]
    [SerializeField] private GameObject optionPanel;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [SerializeField] private AudioClip buttonSE;
    [SerializeField] private AudioClip TitleBGM;

    private bool _isTransitioning = false;  // 遷移中フラグ

    private void Start()
    {
        PlayTitleBGM();
    }

    public void OnClickGameStart()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        StartCoroutine(LoadSceneAfterSE("GameScene"));
    }

    private IEnumerator LoadSceneAfterSE(string sceneName)
    {
        PlayButtonSE();
        yield return new WaitForSeconds(buttonSE.length);
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickQuit()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        PlayButtonSE();
        Application.Quit();
    }

    public void OnClickOption()
    {
        if (_isTransitioning) return;
        PlayButtonSE();
        optionPanel.SetActive(true);
    }

    public void PlayButtonSE()
    {
        if (audioSource != null && buttonSE != null)
        {
            audioSource.PlayOneShot(buttonSE);
        }
    }
    private void PlayTitleBGM()
    {
        if (bgmAudioSource != null && TitleBGM != null)
        {
            bgmAudioSource.clip = TitleBGM;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }
}

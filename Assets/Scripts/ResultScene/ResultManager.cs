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
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Tooltip("ボタン選択時のSE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSE;

    private bool _isTransitioning = false;

    public void OnClickBackTitle()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        StartCoroutine(LoadSceneAfterSE("TitleScene"));
    }

    public void OnClickRetry()
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

    private void PlayButtonSE()
    {
        if (audioSource != null && buttonSE != null)
        {
            audioSource.PlayOneShot(buttonSE);
        }
    }
}

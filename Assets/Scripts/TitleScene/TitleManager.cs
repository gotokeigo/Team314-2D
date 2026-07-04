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
//
//-------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TitleManager : MonoBehaviour
{
    [Tooltip("オプションパネルのGameObject")]
    [SerializeField] private GameObject optionPanel;
    [Tooltip("ボタン選択時のSE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSE;

    public void OnClickGameStart()
    {
        StartCoroutine(LoadSceneAfterSE("GameScene"));
    }

    // シーン遷移をSEがなり終わってからにする
    private IEnumerator LoadSceneAfterSE(string sceneName)
    {
        PlayButtonSE();
        yield return new WaitForSeconds(buttonSE.length);  // SEの長さ分待つ
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickQuit()
    {
        PlayButtonSE();
        Application.Quit();
    }

    public void OnClickOption()
    {
        PlayButtonSE();
        optionPanel.SetActive(true);
    }

    private void PlayButtonSE()
    {
        if (audioSource != null && buttonSE != null)
        {
            audioSource.PlayOneShot(buttonSE);
        }
    }
}

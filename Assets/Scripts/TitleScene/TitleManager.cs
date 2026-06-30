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
//
//-------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{

    public void OnClickGameStart()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    public void OnclickOption()
    {
        // 音を入れた後実装
    }
}

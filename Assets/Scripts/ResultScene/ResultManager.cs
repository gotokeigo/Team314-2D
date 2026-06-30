//-------------------------------------------------------
//
//  TitleManager.cs
//
//  概要
/// リザルトでUIをクリックした後の処理を行うスクリプト。
//
//  更新履歴
//
//  2026/06/30  作成
//
//-------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{

    public void OnClickBackTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}

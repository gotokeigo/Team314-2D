//-------------------------------------------------------
//
//  FieldBoundary.cs
//
//  概要
//  フィールドの円の範囲外に出た敵を落下させるスクリプト
//
//  更新履歴
//
//  2026/06/04  作成
//
//-------------------------------------------------------
using UnityEngine;

public class FieldBoundary : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Enemy")) return;

        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        if (enemyController != null && !enemyController.IsFalling)
        {
            enemyController.FallOff();
        }
    }
}

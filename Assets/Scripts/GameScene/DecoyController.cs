////--------------------------------------
////
////  DecoyController.cs
////
////  概要
////  デコイを制御するスクリプト
////
////  更新履歴
////
////  2026/04/30  作成
////
////--------------------------------------
//using System.Collections;
//using UnityEngine;

//public class DecoyController : MonoBehaviour
//{
//    [SerializeField] private float decoyDuration = 5f;  // デコイの持続時間

//    private void Start()
//    {
//        StartCoroutine(DecoyCoroutine());
//    }

//    private IEnumerator DecoyCoroutine()
//    {
//        // 敵のターゲットをデコイに切り替える
//        SetEnemyTarget("Decoy");

//        yield return new WaitForSeconds(decoyDuration);

//        // 敵のターゲットをプレイヤーに戻す
//        SetEnemyTarget("Player");

//        Destroy(gameObject);
//    }

//    private void SetEnemyTarget(string targetTag)
//    {
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
//        Debug.Log("敵の数: " + enemies.Length); // 追加
//        foreach (GameObject enemy in enemies)
//        {
//            EnemyController enemyController = enemy.GetComponent<EnemyController>();
//            if (enemyController != null)
//            {
//                enemyController.SetTarget(GameObject.FindWithTag(targetTag));
//            }
//        }
//    }
//}

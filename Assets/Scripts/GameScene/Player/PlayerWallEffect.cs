//--------------------------------------
//
//  PlayerWallEffect.cs
//
//  概要
//  プレイヤーがフィールドの壁に当たったときにエフェクトを出すスクリプト
//
//  更新履歴
//
//  2026/07/09  作成
//
//--------------------------------------
using UnityEngine;

public class PlayerWallEffect : MonoBehaviour
{
    [Tooltip("壁に当たったときのエフェクトPrefab")]
    [SerializeField] private GameObject wallEffectPrefab;
    [Tooltip("エフェクトの表示時間")]
    [SerializeField] private float effectLifeTime = 1f;
    [Tooltip("エフェクトの位置オフセット")]
    [SerializeField] private Vector3 effectOffset = Vector3.zero;

    private int _fieldLayer;

    private void Start()
    {
        _fieldLayer = LayerMask.NameToLayer("Field");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != _fieldLayer) return;
        if (wallEffectPrefab == null) return;

        ContactPoint contact = collision.contacts[0];

        // Y座標をプレイヤーに合わせる
        Vector3 effectPos = new Vector3(
            contact.point.x,
            transform.position.y,
            contact.point.z
        );

        GameObject effect = Instantiate(wallEffectPrefab, effectPos + effectOffset, Quaternion.LookRotation(contact.normal)); Destroy(effect, effectLifeTime);
    }
}

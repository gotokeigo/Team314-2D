//--------------------------------------
//
//  WindEffectController.cs
//
//  概要
//  風エフェクトを一定時間表示して消えるスクリプト
//
//  更新履歴
//
//  2026/06/04  作成
//
//--------------------------------------
using UnityEngine;

public class WindEffectController : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.3f;     // 表示時間

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}

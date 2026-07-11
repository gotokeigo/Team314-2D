//--------------------------------------
//
//  ScaleImage.cs
//
//  概要
//  Imageを左右にスケールで揺らすスクリプト
//
//  更新履歴
//
//  2026/07/10  作成
//
//--------------------------------------
using UnityEngine;

public class ScaleImage : MonoBehaviour
{
    [Tooltip("スケールの変化量")]
    [SerializeField] private float scaleAmount = 0.1f;
    [Tooltip("変化の速度")]
    [SerializeField] private float speed = 1f;

    private Vector3 _initialScale;

    private void Start()
    {
        _initialScale = transform.localScale;   // 元のスケールを保存
    }

    private void Update()
    {
        float delta = Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = new Vector3(
            _initialScale.x + delta,
            _initialScale.y + delta,
            _initialScale.z
        );
    }
}

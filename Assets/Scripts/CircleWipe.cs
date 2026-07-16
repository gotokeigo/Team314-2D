//--------------------------------------
//
//  CircleWipe.cs
//
//  概要
//  円形のワイプトランジション
//  シェーダーで中心の穴が縮まって画面を覆う
//
//  更新履歴
//
//  2026/07/11  作成
//  2026/07/12  シェーダー方式に変更
//
//--------------------------------------
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public sealed class CircleWipe : MonoBehaviour
{
    [SerializeField] private Image wipeImage = null;    // シェーダーマテリアルをつけたImage
    private Material _material;

    private void Start()
    {
        _material = wipeImage.material;
        _material.SetFloat("_Radius", 1f);  // 最初は穴が全開
        wipeImage.enabled = false;
    }

    public void WipeOut(float duration, Action on_completed = null)
    {
        StartCoroutine(WipeCoroutine(duration, on_completed));
    }

    private IEnumerator WipeCoroutine(float duration, Action on_completed)
    {
        wipeImage.enabled = true;
        float elapsed_time = 0f;

        while (elapsed_time < duration)
        {
            float t = Mathf.Min(elapsed_time / duration, 1f);
            // Radiusを1→0に変化させる（穴が縮まる）
            _material.SetFloat("_Radius", Mathf.Lerp(1f, 0f, t));
            yield return null;
            elapsed_time += Time.deltaTime;
        }

        _material.SetFloat("_Radius", 0f);
        on_completed?.Invoke();
    }
}

//-------------------------------------------------------
//
//  ShoutVisualEffect.cs
//
//  概要
//  シャウトの範囲を前方後円墳型で表示するエフェクト
//
//  更新履歴
//
//  2026/05/25  作成
//
//-------------------------------------------------------
using UnityEngine;

public class ShoutVisualEffect : MonoBehaviour
{
    [SerializeField] private float displayTime = 2f;        // 表示時間
    [SerializeField] private float backCircleRadius = 1f;   // 後ろの円の半径
    [SerializeField] private float fanRange = 5f;           // 扇の奥行き
    [SerializeField] private float fanAngle = 90f;          // 扇の角度
    [SerializeField] private Color effectColor = new Color(1f, 1f, 0f, 0.5f);  // 色

    private LineRenderer _lineRenderer;
    private float _timer;

    private void Awake()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = true;

        // マテリアル設定
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = effectColor;
        _lineRenderer.endColor = effectColor;
    }

    public void Show(Vector2 origin, Vector2 direction)
    {
        _timer = displayTime;
        DrawShape(origin, direction);
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        // 時間経過で透明にしていく
        float alpha = Mathf.Clamp01(_timer / displayTime);
        _lineRenderer.startColor = new Color(effectColor.r, effectColor.g, effectColor.b, alpha);
        _lineRenderer.endColor = new Color(effectColor.r, effectColor.g, effectColor.b, alpha);

        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void DrawShape(Vector2 origin, Vector2 direction)
    {
        int circleSegments = 20;    // 後ろの円の分割数
        int fanSegments = 20;       // 扇の分割数

        // 点の総数：後ろの円 + 扇の左端 + 扇の弧 + 扇の右端
        int totalPoints = circleSegments + 1 + fanSegments + 1 + 1;
        _lineRenderer.positionCount = totalPoints;

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int index = 0;

        // 後ろの円を描く
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (360f / circleSegments) * i * Mathf.Deg2Rad;
            Vector2 point = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * backCircleRadius;
            _lineRenderer.SetPosition(index++, point);
        }

        // 扇の左端へ
        float leftAngle = (baseAngle + fanAngle / 2f) * Mathf.Deg2Rad;
        Vector2 leftEdge = origin + new Vector2(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle)) * fanRange;
        _lineRenderer.SetPosition(index++, leftEdge);

        // 扇の弧を描く
        for (int i = 0; i <= fanSegments; i++)
        {
            float t = (float)i / fanSegments;
            float angle = (baseAngle + fanAngle / 2f - fanAngle * t) * Mathf.Deg2Rad;
            Vector2 point = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * fanRange;
            _lineRenderer.SetPosition(index++, point);
        }

        // 扇の右端を円に戻す
        float rightAngle = (baseAngle - fanAngle / 2f) * Mathf.Deg2Rad;
        Vector2 rightEdge = origin + new Vector2(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle)) * fanRange;
        _lineRenderer.SetPosition(index++, rightEdge);
    }
}

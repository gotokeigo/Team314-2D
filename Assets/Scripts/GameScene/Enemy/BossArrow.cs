//--------------------------------------
//
//  BossArrow.cs
//  ボスの方向を画面上で指し示すUIインジケーター
//
//--------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class BossArrow : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("追従対象のプレイヤー")]
    [SerializeField] private Transform playerTransform;

    [Header("表示設定")]
    [Tooltip("画面中央（プレイヤー位置）からの矢印の距離(px)")]
    [SerializeField] private float distanceFromCenter = 200f;

    private Transform _bossTransform;
    private RectTransform _rectTransform;
    private Image _arrowImage; // 描画切り替え用のImage参照

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _arrowImage = GetComponent<Image>();

        // GameObjectはアクティブのまま、画像（描画）だけを最初はオフにする
        if (_arrowImage != null)
        {
            _arrowImage.enabled = false;
        }
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    /// <summary>
    /// ボスのターゲット設定
    /// </summary>
    public void SetBossTarget(Transform boss)
    {
        _bossTransform = boss;
        if (_bossTransform != null && _arrowImage != null)
        {
            _arrowImage.enabled = true; // 画像を表示
        }
    }

    /// <summary>
    /// ターゲット解除
    /// </summary>
    public void ClearBossTarget()
    {
        _bossTransform = null;
        if (_arrowImage != null)
        {
            _arrowImage.enabled = false; // 画像を非表示
        }
    }

    private void LateUpdate()
    {
        if (_bossTransform == null || playerTransform == null)
        {
            if (_arrowImage != null && _arrowImage.enabled)
            {
                _arrowImage.enabled = false;
            }
            return;
        }

        // 1. プレイヤーからボスへのワールド方向ベクトル（XZ平面）
        Vector3 dirToBoss = _bossTransform.position - playerTransform.position;
        dirToBoss.y = 0;

        if (dirToBoss == Vector3.zero) return;

        // 2. カメラの「上方向（画面の上）」と「右方向（画面の右）」を基準にして画面上の方向を計算
        Transform camTrans = Camera.main != null ? Camera.main.transform : null;
        if (camTrans == null) return;

        Vector3 camForward = camTrans.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = camTrans.right;
        camRight.y = 0;
        camRight.Normalize();

        // 画面上でのX方向（右）、Y方向（上）への成分を計算
        float screenX = Vector3.Dot(dirToBoss.normalized, camRight);
        float screenY = Vector3.Dot(dirToBoss.normalized, camForward);

        Vector2 screenDirection = new Vector2(screenX, screenY).normalized;

        // 3. 矢印の回転角度を設定（上を基準に回転）
        float angle = Mathf.Atan2(screenDirection.x, screenDirection.y) * Mathf.Rad2Deg;
        _rectTransform.localRotation = Quaternion.Euler(0, 0, -angle);

        // 4. 親Canvasサイズを取得して画面端にクランプ
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>() : null;

        float halfWidth = (canvasRect != null ? canvasRect.rect.width * 0.5f : Screen.width * 0.5f) - 50f;
        float halfHeight = (canvasRect != null ? canvasRect.rect.height * 0.5f : Screen.height * 0.5f) - 50f;

        float scaleX = Mathf.Abs(screenDirection.x) > 0.001f ? halfWidth / Mathf.Abs(screenDirection.x) : float.MaxValue;
        float scaleY = Mathf.Abs(screenDirection.y) > 0.001f ? halfHeight / Mathf.Abs(screenDirection.y) : float.MaxValue;
        float finalScale = Mathf.Min(scaleX, scaleY);

        _rectTransform.anchoredPosition = screenDirection * finalScale;
    }
}

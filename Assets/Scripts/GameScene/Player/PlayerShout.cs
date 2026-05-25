//-------------------------------------------------------
//
//  PlayerShout.cs
//
//  概要
//  プレイヤーの叫び声を飛ばすスクリプト
//  声に当たった敵グループをプレイヤー発見状態にする
//
//  更新履歴
//
//  2026/05/24  作成
//
//-------------------------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class PlayerShout : MonoBehaviour
{
    [SerializeField] private GameObject shoutPrefab;        // 声のPrefab
    [SerializeField] private float shoutCoolTime = 3f;      // クールタイム

    private PlayerController _playerController;
    private float _coolTimer;
    private GameObject _visualEffectObject;
    private ShoutVisualEffect _visualEffect;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();

        _visualEffectObject = new GameObject("ShoutVisualEffect");
        _visualEffect = _visualEffectObject.AddComponent<ShoutVisualEffect>();
    }

    private void Update()
    {
        if (_coolTimer > 0f)
        {
            _coolTimer -= Time.deltaTime;
        }
    }

    // Input Systemのキー入力
    // InputActionで入力された名前(今回だとShout)が呼ばれたときに実行される
    private void OnShout(InputValue value)
    {
        if (_coolTimer > 0f) return;
        _coolTimer = shoutCoolTime;

        Vector2 direction = _playerController.LastMoveDirection;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject shout = Instantiate(shoutPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        shout.GetComponent<ShoutProjectile>().SetDirection(direction);

        // 追加：エフェクト表示
        GameObject effectObj = new GameObject("ShoutVisualEffect");
        ShoutVisualEffect effect = effectObj.AddComponent<ShoutVisualEffect>();
        effect.Show(transform.position, direction);

        Debug.Log("Shout!");
    }
}

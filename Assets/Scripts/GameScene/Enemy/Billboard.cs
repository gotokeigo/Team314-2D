using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (_mainCameraTransform == null)
        {
            if (Camera.main != null) _mainCameraTransform = Camera.main.transform;
            return;
        }

        // カメラの回転だけを単純に同期
        transform.rotation = _mainCameraTransform.rotation;
    }
}

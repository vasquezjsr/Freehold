using UnityEngine;

namespace Freehold.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 2.1f, -5.5f);
        [SerializeField] float mouseSensitivity = 2.4f;
        [SerializeField] float minPitch = -25f;
        [SerializeField] float maxPitch = 55f;
        [SerializeField] float followSmooth = 12f;

        float _yaw;
        float _pitch = 12f;
        Transform _pivot;

        public Transform Pivot => _pivot;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            _pivot = new GameObject("CameraPivot").transform;
            _pivot.position = target.position + Vector3.up * 1.4f;
            _yaw = target.eulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            if (target == null || _pivot == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }

            _pivot.position = Vector3.Lerp(_pivot.position, target.position + Vector3.up * 1.4f, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
            _pivot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            var desired = _pivot.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
            transform.LookAt(_pivot.position);
        }
    }
}

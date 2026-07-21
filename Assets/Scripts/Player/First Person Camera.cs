using CustomUtil;
using CustomUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Labyrinth
{
    [RequireComponent(typeof(Camera))]
    public class FirstPersonCamera : MonoBehaviour
    {
        const float k_MouseSensitivityMultiplier = 0.01f;

        public InputActionReference cameraRotationInput = null;
        public Transform RotationPlayerObject = null;

        public Vector2 cameraRotationSpeed = new(4f, 3f);

        Vector2 _rotationDelta = Vector2.zero;
        Camera _cam = null;
        AudioListener _listener = null;

        public void RefreshInputs()
        {
            if (cameraRotationInput != null)
            {
                _rotationDelta = cameraRotationInput.action.ReadValue<Vector2>() * k_MouseSensitivityMultiplier;
                _rotationDelta.Scale(cameraRotationSpeed);
            }
        }

        void Start()
        {
            _cam = GetComponent<Camera>();
            _listener = GetComponent<AudioListener>();
        }

        void Update()
        {
            if (_cam)
            {
                _cam.enabled = GlobalCameraController.PlayerCamActive;
                _listener.enabled = GlobalCameraController.PlayerCamActive;
            }
            if (!GlobalCameraController.PlayerCamActive) return;


            RefreshInputs();

            if (_rotationDelta.magnitude > 0 && RotationPlayerObject)
            {
                float rotationX = transform.localEulerAngles.x;
                float newRotationY = RotationPlayerObject.transform.localEulerAngles.y + _rotationDelta.x;

                // Weird clamping code due to weird Euler angle mapping...
                float newRotationX = (rotationX - _rotationDelta.y);
                if (rotationX <= 90.0f && newRotationX >= 0.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
                if (rotationX >= 270.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

                transform.localRotation = Quaternion.Euler(transform.localEulerAngles.WithX(newRotationX));

                RotationPlayerObject.transform.localRotation = Quaternion.Euler(RotationPlayerObject.transform.localEulerAngles.WithY(newRotationY));

            }
        }
    }
}

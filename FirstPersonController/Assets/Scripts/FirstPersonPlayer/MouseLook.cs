using UnityEngine;
using Extensions;
using Input;

namespace FirstPersonPlayer {
    public class MouseLook : MonoBehaviour {
        [SerializeField, Range(0, 100)] private float sensitivity = 1;

        [SerializeField] private bool lockCursor;
        
        

        [Header("Vertical Rotation:")]
            [SerializeField] private bool verticalRotationAllowed = true;
            [SerializeField, Range(-360f, 360f), Tooltip("negative x = look up")] private float upperVerticalLimit = -70f;
            [SerializeField, Range(-360f, 360f), Tooltip("positive x = look down")] private float lowerVerticalLimit = 70f;

        [Header("Horizontal Rotation:")]
            [SerializeField] private bool horizontalRotationAllowed = true;


        [SerializeField] private Transform neck;
        private Transform _player;
        private InputAsset.PlayerActions _playerActions;


        private float initialUpperVerticalLimit;
        private float initialLowerVerticalLimit;
        private float initialSensitivity;


        public float XAngle { get; private set; }

        private void Awake() {
            if (lockCursor) Cursor.lockState = CursorLockMode.Locked;

            _player = transform;
            _playerActions = InputAsset.Instance.Player;
            _playerActions.LookAround.started += LookAround_started;

            initialLowerVerticalLimit = lowerVerticalLimit;
            initialUpperVerticalLimit = upperVerticalLimit;
            initialSensitivity = sensitivity;
        }

        private void LookAround_started(UnityEngine.InputSystem.InputAction.CallbackContext context) {
            Vector2 mouseDelta = context.ReadValue<Vector2>();
            mouseDelta *= sensitivity / 10;


            RotateHorizontally(mouseDelta.x);
            RotateVertically(-mouseDelta.y);
        }

        private void RotateHorizontally(float delta) {
            if (!horizontalRotationAllowed) return;

            Vector3 targetEuler = _player.localEulerAngles.Add(y: delta);

            _player.localEulerAngles = targetEuler;
        }

        private void RotateVertically(float delta) {
            if (!verticalRotationAllowed) return;


            XAngle = Mathf.Clamp(XAngle + delta, upperVerticalLimit, lowerVerticalLimit);

            Vector3 targetEuler = neck.localEulerAngles.With(x: XAngle);
            neck.localEulerAngles = targetEuler;
        }

        public void DisableRot() {
            verticalRotationAllowed = false;
            horizontalRotationAllowed = false;
        }
        public void EnableRot() {
            verticalRotationAllowed = true;
            horizontalRotationAllowed = true;
        }

        public void DisableVerticalRot() => verticalRotationAllowed = false;
        public void EnableVerticalRot() => verticalRotationAllowed = true;

        public void ResetSensitivityToInitial() => sensitivity = initialSensitivity;

        public void SetVerticalLimits(float upperLimit, float lowerLimit) {
            upperVerticalLimit = upperLimit;
            lowerVerticalLimit = lowerLimit;
        }

        public void ResetVerticalLimitsToInitial() {
            upperVerticalLimit = initialUpperVerticalLimit;
            lowerVerticalLimit = initialLowerVerticalLimit;
        }
    }
}
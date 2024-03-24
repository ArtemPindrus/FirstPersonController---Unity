using UnityEngine;
using Extensions;
using Input;
using System.Collections.Generic;
using Additional;

namespace FirstPersonController {
    public class MouseLook : MonoBehaviour {
        [field: SerializeField, Range(0, 100)] public float Sensitivity { get; private set; } = 1;

        [SerializeField] private bool lockCursor;
        
        

        [Header("Vertical Rotation:")]
            [SerializeField] private bool verticalRotationAllowed = true;
            [SerializeField, Range(-360f, 360f), Tooltip("negative x = look up")] private float upperVerticalLimit = -70f;
            [SerializeField, Range(-360f, 360f), Tooltip("positive x = look down")] private float lowerVerticalLimit = 70f;

        [Header("Horizontal Rotation:")]
            [SerializeField] private bool horizontalRotationAllowed = true;


        [SerializeField] private Transform neck;
        private Transform _player;
        private MyInput.PlayerMovementActions _playerActions;


        private float initialUpperVerticalLimit;
        private float initialLowerVerticalLimit;
        private float initialSensitivity;

        private List<FloatClass> sensitivityReducers; 
        public float XAngle { get; private set; }

        private void Awake() {
            if (lockCursor) Cursor.lockState = CursorLockMode.Locked;

            _player = transform;
            _playerActions = MyInput.Instance.PlayerMovement;
            _playerActions.LookAround.started += LookAround_started;

            initialLowerVerticalLimit = lowerVerticalLimit;
            initialUpperVerticalLimit = upperVerticalLimit;
            initialSensitivity = Sensitivity;

            sensitivityReducers = new();
        }

        private void LookAround_started(UnityEngine.InputSystem.InputAction.CallbackContext context) {
            float targetSensitivity = Sensitivity;

            foreach (var reducer in sensitivityReducers) targetSensitivity -= reducer.Value;
            targetSensitivity = Mathf.Clamp01(targetSensitivity);

            Vector2 mouseDelta = context.ReadValue<Vector2>();
            mouseDelta *= targetSensitivity / 10;


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

        public void ResetSensitivityToInitial() => Sensitivity = initialSensitivity;

        public void SetVerticalLimits(float upperLimit, float lowerLimit) {
            upperVerticalLimit = upperLimit;
            lowerVerticalLimit = lowerLimit;
        }

        public void ResetVerticalLimitsToInitial() {
            upperVerticalLimit = initialUpperVerticalLimit;
            lowerVerticalLimit = initialLowerVerticalLimit;
        }

        public FloatClass AddSensitivityReducer(float initialValue) {
            FloatClass reducer = new(initialValue);
            sensitivityReducers.Add(reducer);

            return reducer;
        }
    }
}
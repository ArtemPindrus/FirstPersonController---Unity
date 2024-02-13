using UnityEngine;

namespace FirstPersonPlayer {
    public class MouseLook : MonoBehaviour {
        [SerializeField, Range(0, 100)] private float sensitivity = 1;

        [SerializeField] private bool lockCursor;
        [SerializeField] private bool verticalRotationAllowed = true;
        [SerializeField] private bool horizontalRotationAllowed = true;

        [SerializeField, Range(-360f, 360f)] private float upperVerticalLimit = -70f;
        [SerializeField, Range(-360f, 360f)] private float lowerVerticalLimit = 70f;
        [SerializeField, Tooltip("Leave empty to ignore")] private float horizontalLimit;


        public Transform VerticalTransform { get; private set; }
        private Transform horizontalRotTransf;
        private CustomLerpManager lerpManager;


        private float initialUpperVerticalLimit;
        private float initialLowerVerticalLimit;
        private float initialSensitivity;


        private float xAngle = 0;

        private void Awake() {
            if (lockCursor) Cursor.lockState = CursorLockMode.Locked;

            VerticalTransform = GetComponentInChildren<Camera>().transform.parent;
            lerpManager = FindAnyObjectByType<CustomLerpManager>();

            initialSensitivity = sensitivity;

            horizontalRotTransf = transform;

            initialLowerVerticalLimit = lowerVerticalLimit;
            initialUpperVerticalLimit = upperVerticalLimit;
        }

        private void Update() {
            if (horizontalRotationAllowed) {
                float horizontalRot = Input.GetAxis("Mouse X") * sensitivity;
                RotateHorizontally(horizontalRot);
            }

            if (verticalRotationAllowed) {
                float verticalRot = Input.GetAxis("Mouse Y") * sensitivity;
                RotateVertically(-verticalRot);
            }


            //funcs
            void RotateHorizontally(float delta) {
                Vector3 currentRot = horizontalRotTransf.localEulerAngles;

                Vector3 targetEuler = currentRot;
                targetEuler.y += delta;
                if (horizontalLimit != 0) targetEuler.y = Mathf.Clamp(targetEuler.y, -horizontalLimit, horizontalLimit);

                horizontalRotTransf.localEulerAngles = targetEuler;
            }
            void RotateVertically(float delta) {
                xAngle = Mathf.Clamp(xAngle + delta, upperVerticalLimit, lowerVerticalLimit);

                Vector3 targetEuler = VerticalTransform.localEulerAngles;
                targetEuler.x = xAngle;
                VerticalTransform.localEulerAngles = targetEuler;
            }
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

        public void ResetSensitivity() => sensitivity = initialSensitivity;

        public CustomLerpManager.LerpingSingle LerpVerticalRotation(Quaternion targetRot, float lerpingTime) {
            return lerpManager.LerpTo(VerticalTransform, targetRot, lerpingTime);
        }

        public CustomLerpManager.LerpingSingle LerpHorizontalTransformToZeros(float lerpingTime) {
            return lerpManager.LerpTo(horizontalRotTransf, Quaternion.identity, lerpingTime);
        }

        public void SetCustomHorizontalRotationTransform(Transform targetTransform, float horizontalLimit) {
            this.horizontalLimit = horizontalLimit;
            horizontalRotTransf = targetTransform;
        }


        public void ResetHorizontalRotationTransform() {
            horizontalRotTransf = transform;
            horizontalLimit = 0;
        }

        public void SetVerticalLimits(float upperLimit, float lowerLimit) {
            upperVerticalLimit = upperLimit;
            lowerVerticalLimit = lowerLimit;
        }

        public void ResetVerticalLimits() {
            upperVerticalLimit = initialUpperVerticalLimit;
            lowerVerticalLimit = initialLowerVerticalLimit;
        }
    }
}
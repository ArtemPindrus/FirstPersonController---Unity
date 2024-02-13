using UnityEngine;
using Lerping;

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
            [SerializeField, Range(-360f, 360f), Tooltip("Leave empty to ignore")] private float horizontalLimit;


        private Transform neck;
        private Transform player;


        private float initialUpperVerticalLimit;
        private float initialLowerVerticalLimit;
        private float initialSensitivity;


        public float XAngle { get; private set; }

        private void Awake() {
            if (lockCursor) Cursor.lockState = CursorLockMode.Locked;

            neck = GetComponentInChildren<Camera>().transform.parent;
            player = transform;

            initialLowerVerticalLimit = lowerVerticalLimit;
            initialUpperVerticalLimit = upperVerticalLimit;
            initialSensitivity = sensitivity;
        }

        private void Update() {
            if (horizontalRotationAllowed) {
                float horizontalRot = Input.GetAxis("Mouse X") * sensitivity; //TODO: DeltaTime?
                RotateHorizontally(horizontalRot);
            }

            if (verticalRotationAllowed) {
                float verticalRot = Input.GetAxis("Mouse Y") * sensitivity;
                RotateVertically(-verticalRot);
            }


            //funcs
            void RotateHorizontally(float delta) {
                Vector3 currentRot = player.localEulerAngles;

                Vector3 targetEuler = new(currentRot.x, currentRot.y+delta, currentRot.z);
                if (horizontalLimit != 0) targetEuler.y = Mathf.Clamp(targetEuler.y, -horizontalLimit, horizontalLimit);

                player.localEulerAngles = targetEuler;
            }
            void RotateVertically(float delta) {
                XAngle = Mathf.Clamp(XAngle + delta, upperVerticalLimit, lowerVerticalLimit);

                Vector3 targetEuler = neck.localEulerAngles;
                targetEuler.x = XAngle;
                neck.localEulerAngles = targetEuler;
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

        public void LerpVerticalRotation(float targetXAngle, float lerpingTime, bool reenableVerRotOnFinish) {
            DisableVerticalRot();
            LerpFloat lerpXAngle = LerpFloat.Initialize(XAngle, targetXAngle, SetXAngle, lerpingTime);
            if (reenableVerRotOnFinish) lerpXAngle.LerpFinished += EnableVerticalRot;

            void SetXAngle(float newX) {
                XAngle = newX;
            }
        }

        //public void SetCustomHorizontalRotationTransform(Transform targetTransform, float horizontalLimit) {
        //    this.horizontalLimit = horizontalLimit;
        //    player = targetTransform;
        //}


        //public void ResetHorizontalRotationTransform() {
        //    player = transform;
        //    horizontalLimit = 0;
        //}

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
using UnityEngine;
using System;

namespace FirstPersonPlayer {
    public class Leaning : MonoBehaviour {
        private Transform neck;
        private PlayerInput playerInput;

        [SerializeField, Tooltip("(seconds) to lean to one side"), Range(0, 60)] private float requiredTime;
        [SerializeField, Tooltip("(units)"), Range(0, 1)] private float positionDelta;
        [SerializeField, Tooltip("(degrees)"), Range(0, 90)] private float rotationDelta = 25;

        private float elapsedTime;

        private float CurrentLean {
            get {
                return neck.transform.localPosition.x switch {
                    > 0 => 1,
                    < 0 => -1,
                    _ => 0
                };
            }
        }

        private void Awake() {
            neck = Camera.main.transform.parent;

            playerInput = new();
            playerInput.Player.Enable();
        }

        float direction = 1;
        private void Update() {
            if (!enabled) return;

            //change elapsedTime
            float input = playerInput.Player.Lean.ReadValue<float>();

            if (input == 0 || input == -CurrentLean) elapsedTime -= Time.deltaTime;
            else elapsedTime += Time.deltaTime;

            elapsedTime = Mathf.Clamp(elapsedTime, 0, requiredTime);

            //change direction
            if (CurrentLean == 0 && input != 0) direction = input;

            //calculate t and lean
            float percent = elapsedTime / requiredTime;
            float t = percent * percent * (3f - 2f * percent);

            LeanPos(positionDelta * direction);
            LeanRot(rotationDelta * -direction);

            ////fncs
            void LeanPos(float target) {
                Vector3 newPos = new(Mathf.Lerp(0, target, t), neck.transform.localPosition.y, neck.transform.localPosition.z);
                neck.localPosition = newPos;
            }
            void LeanRot(float target) {
                Quaternion newRot = Quaternion.Lerp(Quaternion.Euler(neck.rotation.x, 0, 0), Quaternion.Euler(neck.rotation.x, 0, target), t);
                neck.localRotation = newRot;
            }
        }
    }
}
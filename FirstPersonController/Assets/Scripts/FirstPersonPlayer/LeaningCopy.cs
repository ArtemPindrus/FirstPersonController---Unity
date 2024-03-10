using UnityEngine;
using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;

namespace FirstPersonPlayer {
    public class LeaningCopy : MonoBehaviour {
        [SerializeField, Tooltip("(seconds) to lean to one side"), Range(0, 60)] private float requiredTime;
        [SerializeField, Tooltip("(units)"), Range(0, 1)] private float positionDelta;
        [SerializeField, Tooltip("(degrees)"), Range(0, 90)] private float rotationDelta = 25;

        private Transform neck;
        private PlayerInput playerInput;
        private MouseLook mouseLook;

        private TweenerCore<Vector3, Vector3, VectorOptions> positionTween;
        private TweenerCore<Vector3, float, FloatOptions> rotationTween;

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
            mouseLook = GetComponent<MouseLook>();

            playerInput = PlayerInputSingleton.Instance;

            positionTween = DOTween.To(
                () => neck.localPosition, 
                x => neck.localPosition = x, 
                new(0, neck.localPosition.y, 
                neck.localPosition.z), 
                requiredTime
            ).SetEase(Ease.InOutSine).SetAutoKill(false);

            //rotationTween = DOTween.To(
            //    () => neck.localRotation,
            //    x => neck.localRotation = x,
            //);
        }

        float direction = 0;
        private void Update() {
            if (!enabled) return;
            Debug.Log($"Active: {positionTween.active}");
            Debug.Log($"IsBackwards: {positionTween.IsBackwards()}");

            //find direction
            float input = playerInput.Player.Lean.ReadValue<float>();
            if (input == 0) direction = 0;
            else if (CurrentLean == 0 || CurrentLean == input) direction = input;
            else if (CurrentLean != 0) direction = 0;
            

            if (CurrentLean == 0) {
                positionTween.ChangeEndValue(new(0, neck.localPosition.y, neck.localPosition.z));
                positionTween.PlayForward();
            } else if (direction == 0) positionTween.PlayBackwards();
            
            if (direction != 0) SetEnd(positionDelta * direction);

            void SetEnd(float x) {
                Vector3 end = new(x, neck.localPosition.y, neck.localPosition.z);
                if (positionTween.endValue != end) positionTween.ChangeEndValue(end);
            }
        }
    }
}
using UnityEngine;
using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using Extensions;

namespace FirstPersonPlayer {
    public class Leaning : MonoBehaviour {
        [SerializeField, Tooltip("(seconds) to lean to one side"), Range(0, 60)] private float requiredTime;
        [SerializeField, Tooltip("(units)"), Range(0, 1)] private float positionDelta;
        [SerializeField, Tooltip("(degrees)"), Range(0, 90)] private float rotationDelta = 25;

        [SerializeField] private Transform neck;
        [SerializeField] private Transform neckZRotator;
        private @InputSystem.PlayerActions playerInput;

        private TweenerCore<Vector3, Vector3, VectorOptions> positionTween;
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> rotationTween;

        private float CurrentLean {
            get => neck.transform.localPosition.x.SignZero();
        }

        private void Awake() {
            playerInput = InputSystem.Instance.Player;

            positionTween = neck.DOLocalMove(new(0, neck.localPosition.y, neck.localPosition.z), requiredTime)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(false);

            rotationTween = neckZRotator.DOLocalRotate(new(0, 0, 0), requiredTime)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(false);
        }

        private void Update() {
            if (!enabled) return;


            float input = playerInput.Lean.ReadValue<float>();

            if (CurrentLean == 0) {
                if (input == 0) SetTweensEnds(0, 0);
                else SetTweensEnds(positionDelta * input, -rotationDelta * input);
            } else {
                if (input == CurrentLean) SetTweensEnds(positionDelta * input, -rotationDelta * input);
                else {
                    positionTween.PlayBackwards();
                    rotationTween.PlayBackwards();
                }
            }
            

            void SetTweensEnds(float position, float angle) {
                Vector3 positionEnd = neck.localPosition.With(x: position);
                Vector3 angleEnd = new(0, 0, angle);

                if (positionTween.endValue != positionEnd) positionTween.ChangeEndValue(positionEnd);
                if (rotationTween.endValue != angleEnd) rotationTween.ChangeEndValue(angleEnd);

                positionTween.PlayForward();
                rotationTween.PlayForward();
            }
        }
    }
}
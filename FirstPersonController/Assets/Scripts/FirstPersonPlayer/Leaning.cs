using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using Extensions;
using Input;

namespace FirstPersonController {
    public class Leaning : MonoBehaviour {
        [SerializeField, Tooltip("(seconds) to lean to one side"), Range(0, 60)] private float requiredTime;
        [SerializeField, Tooltip("(units)"), Range(0, 1)] private float positionDelta;
        [SerializeField, Tooltip("(degrees)"), Range(0, 90)] private float rotationDelta = 25;

        [SerializeField] private Transform neck;
        [SerializeField] private Transform neckZRotator;

        private InputAsset.PlayerActions _playerInput;

        private TweenerCore<Vector3, Vector3, VectorOptions> _positionTween;
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _rotationTween;

        private float CurrentLean => neck.transform.localPosition.x.SignZero();

        private void Awake() {
            Vector3 currentNeckLocalPos = neck.localPosition;
            _playerInput = InputAsset.Instance.Player;

            _positionTween = neck.DOLocalMove(currentNeckLocalPos.With(x: 0), requiredTime)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(false);

            _rotationTween = neckZRotator.DOLocalRotate(new(0, 0, 0), requiredTime)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(false);
        }

        private void Update() {
            if (!enabled) return;


            float input = _playerInput.Lean.ReadValue<float>();

            if (CurrentLean == 0) {
                if (input == 0) SetTweensEnds(0, 0);
                else SetTweensEnds(positionDelta * input, -rotationDelta * input);
            } else {
                if (input == CurrentLean) SetTweensEnds(positionDelta * input, -rotationDelta * input);
                else {
                    _positionTween.PlayBackwards();
                    _rotationTween.PlayBackwards();
                }
            }
            

            void SetTweensEnds(float position, float angle) {
                Vector3 positionEnd = neck.localPosition.With(x: position);
                Vector3 angleEnd = new(0, 0, angle);

                if (_positionTween.endValue != positionEnd) _positionTween.ChangeEndValue(positionEnd);
                if (_rotationTween.endValue != angleEnd) _rotationTween.ChangeEndValue(angleEnd);

                _positionTween.PlayForward();
                _rotationTween.PlayForward();
            }
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using DG.Tweening.Core;
using Extensions;
using Scripts;
using DG.Tweening.Plugins.Options;
using Input;

namespace FirstPersonPlayer {
    public enum CrouchState { Standing, Uncrouching, Crouched, Crouching, Under }

    [RequireComponent(typeof(CharacterController))]
    public class Crouching : MonoBehaviour {
        [field: SerializeField, Range(0f, 10f), Tooltip("Time in seconds to crouch")] public float CrouchingTime { get; private set; } = 0.5f;
        [field: SerializeField, Range(0, 2), Tooltip("How tall is the player in decrouched state. \nSets to CCT.height if not assigned.")] public float DecrouchedHeight { get; private set; }
        [field: SerializeField, Tooltip("During Under crouch state the step offset is nulled out. After escaping it sets to the given value here. If 0 sets to the CCT.stepOffset on startup.")]
            public float BasicStepOffset { get; private set; }
        [SerializeField, Range(0f, 1f), Tooltip("How much of decrouched heigh is a crouching height")] private float crouchedHeightMult = 0.5f;
        [SerializeField, Range(0.01f, 1f), Tooltip("While uncrouching the player will stop if he encounters obstacle above within the given distance. \n" +
            "Note that the value doesn't change during the runtime!")] 
            private float overhead = 0.1f;

        public float CrouchedHeight { get; private set; }


        public CharacterController CharController { get; private set; }
        public CrouchState CrouchState { get; set; } = CrouchState.Standing;

        private InputAsset.PlayerActions _playerInput;
        private BoxCast _boxCastUp;
        private TweenerCore<float, float, FloatOptions> _heightTween;
        private static readonly Ease _crouchingEase = Ease.InOutSine;

        private float _previousHeight;



        public bool IsInCrouchUnderState => CrouchState == CrouchState.Crouched || CrouchState == CrouchState.Under;
        private bool _crouchMuted;


        private void Start() {
            CharController = GetComponent<CharacterController>();
            if (DecrouchedHeight == 0) DecrouchedHeight = CharController.height;
            if (BasicStepOffset == 0) BasicStepOffset = CharController.stepOffset;
            CrouchedHeight = crouchedHeightMult * DecrouchedHeight;

            //input
            _playerInput = InputAsset.Instance.Player;
            _playerInput.Crouch.performed += HandleCrouch_Performed;

            //initialize boxcast
            GameObject empty = new("BoxCastAbove") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.zero + Vector3.up * (CharController.height / 2 - 0.05f) //-0.05 to eliminate problems with detection of objects that are too close
                }
            };

            float radius = CharController.radius;
            _boxCastUp = empty.AddComponent<BoxCast>()
                .Initialize(new(radius, 0.01f, radius), Vector3.up, overhead + 0.05f);
        }
        private void HandleCrouch_Performed(InputAction.CallbackContext _) {
            if (CharController.isGrounded) {
                if (CrouchState == CrouchState.Standing || CrouchState == CrouchState.Uncrouching) {
                    SetToCrouching();
                } else if ((CrouchState == CrouchState.Crouched || CrouchState == CrouchState.Crouching) && !_boxCastUp.HitLastFrame) {
                    SetToDecrouching();
                }
            }
        }

        private void Update() {
            _previousHeight = CharController.height;

            if (_crouchMuted && CrouchState == CrouchState.Under) return;
            else if (CrouchState == CrouchState.Crouching && CharController.height == CrouchedHeight) CrouchState = CrouchState.Crouched;
            else if (CrouchState == CrouchState.Uncrouching) {
                if (CharController.height == DecrouchedHeight) SetToStanding();
                else if (_boxCastUp.HitLastFrame) MuteCrouchUntil(() => !_boxCastUp.HitLastFrame);
            }
        }

        /// <summary>
        /// Sets the new height of the CCT with adjusting the center so the childrens save their relative position
        /// </summary>
        /// <param name="newHeight"></param>
        private void SetHeight(float newHeight) {
            float height = CharController.height;
            _previousHeight = height;

            height = newHeight;
            CharController.height = height;

            //adjust center
            float heightDelta = _previousHeight - height;
            Vector3 targetCenter = CharController.center.Add(y: heightDelta / 2);
            CharController.center = targetCenter;

            //adjust y locPosition
            CharController.enabled = false;
            transform.Translate(-Vector3.up * heightDelta);
            CharController.enabled = true;
        }


        private void SetToStanding() { 
            _heightTween.Pause();

            CrouchState = CrouchState.Standing;
        }
        private void SetToDecrouching() {
            _heightTween = DOTween.To(
                () => CharController.height,
                SetHeight,
                DecrouchedHeight,
                CrouchingTime
            ).SetEase(_crouchingEase);
            CrouchState = CrouchState.Uncrouching;
        }
        private void SetToCrouching() {
            _heightTween = DOTween.To(
                () => CharController.height,
                SetHeight,
                CrouchedHeight,
                CrouchingTime
            ).SetEase(_crouchingEase);
            CrouchState = CrouchState.Crouching;
        }

        private void MuteCrouchUntil(Func<bool> condition) {
            _heightTween.Pause();
            _crouchMuted = true;

            BehaviorOnConditionManager.ConstructBehaviorOnCondition(this, condition, Resume);

            void Resume() {
                _crouchMuted = false;
                _heightTween.Play();
            }
        }

#nullable enable
        /// <summary>
        /// Interpolates current height to the target value and back to initial height to simulate knees bend
        /// </summary>
        /// <param name="heightMultiplier">How much of initial height is a target value</param>
        /// <param name="time">Time that will be applied to both interpolations</param>
        /// <param name="onCrouchAchieved"></param>
        /// <param name="onFinished">Callback for finished bend</param>
        public void Bend(float heightMultiplier, float time, TweenCallback? onCrouchAchieved, TweenCallback? onFinished) {
            if (CrouchState != CrouchState.Standing) throw new Exception("Current crouch state isn't Standing!");

            heightMultiplier = Mathf.Clamp01(heightMultiplier);
            float targetLowerHeight = DecrouchedHeight * heightMultiplier;

            var tweenSequence = DOTween.Sequence();
            var bend = DOTween.To(() => CharController.height, SetHeight, targetLowerHeight, time).SetEase(Ease.InOutSine);

            if (onCrouchAchieved != null) bend.OnComplete(onCrouchAchieved);

            tweenSequence.Append(bend);
            tweenSequence.Append(
                DOTween.To(() => CharController.height, SetHeight, DecrouchedHeight, time)
                    .SetEase(Ease.InSine)
            );

            if (onFinished != null) tweenSequence.OnComplete(onFinished);
        }
#nullable disable

        public Tween ReassignHeightTween(float targetHeight, float duration, bool killPrevious = false) {
            if (killPrevious) _heightTween.Kill();

            _heightTween = DOTween.To(
                () => CharController.height,
                SetHeight,
                targetHeight,
                duration
            ).SetEase(_crouchingEase);

            return _heightTween;
        }
    }
}
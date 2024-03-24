using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using DG.Tweening.Core;
using Extensions;
using DG.Tweening.Plugins.Options;
using Input;
using PhysicsCasting;
using Work;
using Additional;

namespace FirstPersonController {
    public enum CrouchState { Standing, Uncrouching, Crouched, Lowering, Prone }

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(BasicMovement))]
    public class Crouching : MonoBehaviour {
        [Header("Heights:")]
            [SerializeField, Range(0, 2)] private float standingHeight;
            [SerializeField, Range(0, 2)] private float crouchedHeight;
            [SerializeField, Range(0, 2)] private float proneHeight;
            [SerializeField, Range(0.01f, 1f), Tooltip("While uncrouching the player will stop if he encounters obstacle above within the given distance. \n" +
                                                       "Note that the value doesn't change during the runtime!")]
                private float overhead = 0.1f;

        [Header("Times: (seconds)")]
            [SerializeField, Range(0f, 10f)] private float crouchingTime = 0.5f;
            [SerializeField, Range(0.01f, 10)] private float proneTime;

        [Header("Speed Reductions:")]
            [SerializeField, Range(0, 100)] private float crouchedSpeedReduction;
            [SerializeField, Range(0, 100)] private float proneSpeedReduction;

        public CharacterController CharController { get; private set; }
        public CrouchState CurrentCrouchState {
            get {
                if (CharController.height == standingHeight) return CrouchState.Standing;
                else if (CharController.height == crouchedHeight) return CrouchState.Crouched;
                else if (heightTween.endValue == standingHeight) return CrouchState.Uncrouching;
                else if (heightTween.endValue == crouchedHeight) return CrouchState.Lowering;
                else if (CharController.height == proneHeight || heightTween.endValue == proneHeight) return CrouchState.Prone;
                else throw new Exception("Current Crouch State failed to be evaluated");
            }
        }

        private BasicMovement movement;
        private MyInput.PlayerMovementActions _playerInput;

        private TweenerCore<float, float, FloatOptions> heightTween;
        private FloatClass crouchingSpeedReducer;
        private static readonly Ease crouchingEase = Ease.InOutSine;

        private BoxCast boxCastUp;

        private float previousHeight;

        private void Start() {
            CharController = GetComponent<CharacterController>();
            if (standingHeight == 0) throw new Exception(nameof(standingHeight) + " is not assigned!");
            if (crouchedHeight == 0) throw new Exception(nameof(crouchedHeight) + " is not assigned!");
            if (proneHeight == 0) throw new Exception(nameof(proneHeight) + " is not assigned!");

            if (crouchedHeight >= standingHeight) throw new Exception(nameof(crouchedHeight) + " should be less then " + nameof(standingHeight));
            if (proneHeight >= crouchedHeight) throw new Exception(nameof(proneHeight) + " should be less then " + nameof(crouchedHeight));


            movement = GetComponent<BasicMovement>();
            crouchingSpeedReducer = movement.AddSpeedReducer(0);

            //input
            _playerInput = MyInput.Instance.PlayerMovement;
            _playerInput.Crouch.performed += HandleCrouch_Performed;

            //initialize boxcast
            GameObject boxCastAboveObject = new("BoxCastAbove") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.up * (CharController.height / 2 - 0.05f) //-0.05 to eliminate problems with detection of objects that are too close
                }
            };

            float radius = CharController.radius;
            boxCastUp = boxCastAboveObject.AddComponent<BoxCast>()
                .Initialize(new(radius, 0.01f, radius), Vector3.up, overhead + 0.05f);
        }

        private void HandleCrouch_Performed(InputAction.CallbackContext _) {
            if (CharController.isGrounded) {
                if (CurrentCrouchState == CrouchState.Standing || CurrentCrouchState == CrouchState.Uncrouching) {
                    SetToCrouching();
                } else if ((CurrentCrouchState == CrouchState.Crouched || CurrentCrouchState == CrouchState.Lowering) && !boxCastUp.HitLastFrame) {
                    SetToUncrouching();
                }
            }
        }

        private void Update() {
            previousHeight = CharController.height;

            if (CurrentCrouchState == CrouchState.Uncrouching && boxCastUp.HitLastFrame) MuteCrouchWhile(() => boxCastUp.HitLastFrame && CurrentCrouchState == CrouchState.Uncrouching);
        }

        /// <summary>
        /// Sets the new height of the CCT with adjusting the center so the childrens save their relative position
        /// </summary>
        /// <param name="newHeight"></param>
        private void SetHeight(float newHeight) {
            float height = CharController.height;
            previousHeight = height;

            height = newHeight;
            CharController.height = height;

            //adjust center
            float heightDelta = previousHeight - height;
            Vector3 targetCenter = CharController.center.Add(y: heightDelta / 2);
            CharController.center = targetCenter;

            //adjust y locPosition
            CharController.enabled = false;
            transform.Translate(-Vector3.up * heightDelta);
            CharController.enabled = true;
        }

        private void SetToUncrouching() {
            ReassignHeightTween(standingHeight, crouchingTime, true);
            crouchingSpeedReducer.Change(0);
        }
        public void SetToCrouching() {
            ReassignHeightTween(crouchedHeight, crouchingTime, true);
            crouchingSpeedReducer.Change(crouchedSpeedReduction);

            Func<bool> whileCondition = () => CurrentCrouchState != CrouchState.Standing;
            movement.MuteRunningWhile(whileCondition);
            movement.MuteJumpingWhile(whileCondition);
        }
        public void SetToProne() {
            ReassignHeightTween(proneHeight, proneTime, true);

            crouchingSpeedReducer.Change(proneSpeedReduction);
        }

        /// <summary>
        /// Crouching is guaranteed to be muted while the condition is true. Condition add up.
        /// </summary>
        /// <param name="whileCondition"></param>
        private void MuteCrouchWhile(Func<bool> whileCondition) {
            WorkManager.ConstructDoWhileInUpdate(this, whileCondition,
                new Action[] { () => heightTween.Pause() },
                () => heightTween.Play());
        }

        public Tween ReassignHeightTween(float targetHeight, float duration, bool killPrevious = false) {
            if (killPrevious) heightTween.Kill();

            heightTween = DOTween.To(
                () => CharController.height,
                SetHeight,
                targetHeight,
                duration
            ).SetEase(crouchingEase);

            return heightTween;
        }
    }
}
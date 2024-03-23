using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using DG.Tweening.Core;
using Extensions;
using DG.Tweening.Plugins.Options;
using Input;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.FirstPersonPlayer;
using Casting;
using Work;

namespace FirstPersonController {
    public enum CrouchState { Standing, Uncrouching, Crouched, Crouching, Under }

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(BasicMovement))]
    public class Crouching : MonoBehaviour {
#warning MESS!
        #region CrouchingVariables
        [field: Header("Crouching")]


        [Range(0, 2), Tooltip("How tall is the player in decrouched state. \nSets to CCT.height if not assigned.")]
        public float decrouchedHeight;

        [SerializeField, Range(0f, 1f), Tooltip("How much of decrouched heigh is a crouching height")]
        private float crouchedHeightMult = 0.5f;

        [SerializeField, Range(0, 100)] private float crouchingSpeedReduction;

        [field: SerializeField, Range(0f, 10f), Tooltip("Time in seconds to crouch")]
        public float CrouchingTime { get; private set; } = 0.5f;

        [SerializeField, Range(0.01f, 1f), Tooltip("While uncrouching the player will stop if he encounters obstacle above within the given distance. \n" +
            "Note that the value doesn't change during the runtime!")]
        private float overhead = 0.1f;

        #endregion

        [field: SerializeField, Tooltip("During Under crouch state the step offset is nulled out. After escaping it sets to the given value here. If 0 sets to the CCT.stepOffset on startup.")]
        public float BasicStepOffset { get; private set; }


        #region ExtraCrouchVariables
        [Header("Extra crouch:")]
        [SerializeField, Range(0.01f, 10)] private float extraCrouchTime;
        [SerializeField, Range(0, 100)] private float extraCrouchSpeedReduction;

        #endregion

        public float CrouchedHeight { get; private set; }


        public CharacterController CharController { get; private set; }
        public CrouchState CrouchState { get; set; } = CrouchState.Standing;

        private BasicMovement movement;
        private InputAsset.PlayerActions _playerInput;
        private BoxCast _boxCastUp;
        private TweenerCore<float, float, FloatOptions> heightTween;
        private FloatClass crouchingSpeedReducer;
        private static readonly Ease crouchingEase = Ease.InOutSine;

        private float _previousHeight;



        public bool IsInCrouchUnderState => CrouchState == CrouchState.Crouched || CrouchState == CrouchState.Under;


        private void Start() {
            CharController = GetComponent<CharacterController>();
            if (decrouchedHeight == 0) decrouchedHeight = CharController.height;
            if (BasicStepOffset == 0) BasicStepOffset = CharController.stepOffset;
            CrouchedHeight = crouchedHeightMult * decrouchedHeight;

            movement = GetComponent<BasicMovement>();
            activeCrouchable = new();
            crouchingSpeedReducer = movement.AddSpeedReducer(0);

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
                    SetToUncrouching();
                }
            }
        }

        private void Update() {
            if (activeCrouchable.Count != 0) {
                CrouchableUnder lowest = LowestHeightCrouchable;
                if (heightTween.endValue != lowest.TargetHeight) ExtraCrouch(lowest);
            }

            _previousHeight = CharController.height;

            if (CrouchState == CrouchState.Under) return;
            else if (CrouchState == CrouchState.Crouching && CharController.height == CrouchedHeight) SetToCrouched();
            else if (CrouchState == CrouchState.Uncrouching) {
                if (CharController.height == decrouchedHeight) SetToStanding();
                else if (_boxCastUp.HitLastFrame) MuteCrouchWhile(() => _boxCastUp.HitLastFrame && CrouchState == CrouchState.Uncrouching);
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
            CrouchState = CrouchState.Standing;
            crouchingSpeedReducer.Change(0);
        }
        private void SetToCrouched() {
            CrouchState = CrouchState.Crouched;
            crouchingSpeedReducer.Change(crouchingSpeedReduction);
        }

        private void SetToUncrouching() {
            ReassignHeightTween(decrouchedHeight, CrouchingTime, true);
            crouchingSpeedReducer.Change(0);

            CrouchState = CrouchState.Uncrouching;
        }
        private void SetToCrouching() {
            ReassignHeightTween(CrouchedHeight, CrouchingTime, true);
            crouchingSpeedReducer.Change(crouchingSpeedReduction);

            WorkManager.ConstructDoWhileInUpdate(this, 
                () => CrouchState != CrouchState.Uncrouching && CrouchState != CrouchState.Standing, 
                new Action[] { () => movement.MuteRunning(), () => movement.MuteJumping() }, 
                () => movement.ResumeRunning(), () => movement.ResumeJumping()
            ); //mute running and jumping until uncrouching

            CrouchState = CrouchState.Crouching;
        }

        [Obsolete("Use MuteCrouchWhile instead. With this one it's possible to unmute crouch before condition.")]
        private void MuteCrouchUntil(Func<bool> condition) {
            heightTween.Pause();

            WorkManager.ConstructDoWhenInUpdate(this, condition, Resume);

            void Resume() {
                heightTween.Play();
            }
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
            float targetLowerHeight = decrouchedHeight * heightMultiplier;

            var tweenSequence = DOTween.Sequence();
            var bend = DOTween.To(() => CharController.height, SetHeight, targetLowerHeight, time).SetEase(Ease.InOutSine);

            if (onCrouchAchieved != null) bend.OnComplete(onCrouchAchieved);

            tweenSequence.Append(bend);
            tweenSequence.Append(
                DOTween.To(() => CharController.height, SetHeight, decrouchedHeight, time)
                    .SetEase(Ease.InSine)
            );

            if (onFinished != null) tweenSequence.OnComplete(onFinished);
        }
#nullable disable

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


        private List<CrouchableUnder> activeCrouchable;
        private CrouchableUnder LowestHeightCrouchable {
            get {
                return activeCrouchable.Count switch {
                    1 => activeCrouchable[0],
                    >1 => activeCrouchable.OrderBy(x => x.TargetHeight).First(),
                    _ => throw new Exception(nameof(activeCrouchable) + " is empty!")
                };
            }
        }

        public void OnTriggerStay(Collider other) {
            if (other.TryGetComponent(out CrouchableUnder crouchable) && !crouchable.Muted && IsInCrouchUnderState) {
                activeCrouchable.Add(crouchable);
                crouchable.Mute();
            }
        }

        public void OnTriggerExit(Collider other) {
            if (other.TryGetComponent(out CrouchableUnder crouchable) && crouchable.Muted) {
                crouchable.Unmute();
                activeCrouchable.Remove(crouchable);
            }
        }

        private WorkManager.WorkInUpdate previousExtraCrouchWork;
        private void ExtraCrouch(CrouchableUnder component) {
            crouchingSpeedReducer.Change(extraCrouchSpeedReduction);
            CharController.stepOffset = 0;

            ReassignHeightTween(component.TargetHeight, extraCrouchTime, true);
            CrouchState = CrouchState.Under;

            if (previousExtraCrouchWork != null && !previousExtraCrouchWork.Completed) previousExtraCrouchWork.Kill();
            previousExtraCrouchWork = WorkManager.ConstructDoWhenInUpdate(this, PlayerLeftColliderBounds, ReactToLeftBoundaries);

            bool PlayerLeftColliderBounds() => !component.TriggerCollider.bounds.Intersects(CharController.bounds);
            void ReactToLeftBoundaries() {
                ReassignHeightTween(CrouchedHeight, extraCrouchTime, true).OnComplete(ReactToCompletedDecrouchFromUnder);

                component.Unmute();
            }
            void ReactToCompletedDecrouchFromUnder() {
                SetToCrouched();
                CharController.stepOffset = BasicStepOffset;
            }
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using DG.Tweening.Core;
using Extensions;
using Scripts;
using DG.Tweening.Plugins.Options;

namespace FirstPersonPlayer {
    public enum CrouchState { Decrouched, Decrouching, Crouched, Crouching, Under }

    [RequireComponent(typeof(CharacterController))]
    public class Crouching : MonoBehaviour {
        [SerializeField, Range(0f, 10f), Tooltip("Time in seconds to crouch")] private float crouchingTime = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("How much of initial heigh is a crouching height")] private float crouchedHeightMult = 0.5f;
        [SerializeField, Range(0.01f, 1f), Tooltip("While uncrouching the player will stop if he encounters obstacle above within the given distance. \n" +
            "Note that the value doesn't change during the runtime!")] 
            private float overhead = 0.1f;

        public float InitialHeight { get; private set; }
        private float crouchedHeight;


        private CharacterController charController;
        private @InputSystem.PlayerActions playerInput;
        private BoxCast boxCastUp;
        private TweenerCore<float, float, FloatOptions> heightTween;

        private float previousHeight;

        public CrouchState CrouchState { get; private set; } = CrouchState.Decrouched;


        public bool IsInCrouchUnderState => CrouchState == CrouchState.Crouched || CrouchState == CrouchState.Under;
        private bool crouchMuted;


        private void Start() {
            charController = GetComponent<CharacterController>();
            InitialHeight = charController.height;

            //input
            playerInput = InputSystem.Instance.Player;
            playerInput.Crouch.performed += HandleCrouch_Performed;

            //initialize tween
            crouchedHeight = crouchedHeightMult * charController.height;
            heightTween = DOTween.To(() => charController.height, SetHeight, crouchedHeight, crouchingTime)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(false).Pause();

            //initialize boxcast
            GameObject empty = new("BoxCastAbove");
            empty.transform.parent = transform;
            empty.transform.localPosition = Vector3.zero + Vector3.up * (charController.height / 2 - 0.05f); //-0.05 to eliminate problems with detection of objects that are too close

            boxCastUp = empty.AddComponent<BoxCast>()
                .Initialize(new(charController.radius, 0.01f, charController.radius), Vector3.up, overhead + 0.05f, false);
        }
        private void HandleCrouch_Performed(InputAction.CallbackContext _) {
            if (charController.isGrounded) {
                if (CrouchState == CrouchState.Decrouched || CrouchState == CrouchState.Decrouching) {
                    SetToCrouching();
                } else if ((CrouchState == CrouchState.Crouched || CrouchState == CrouchState.Crouching) && !boxCastUp.HitLastFrame) {
                    SetToDecrouching();
                }
            }
        }

        private void Update() {
            previousHeight = charController.height;

            if (crouchMuted && CrouchState == CrouchState.Under) return;
            else if (CrouchState == CrouchState.Crouching && charController.height == crouchedHeight) CrouchState = CrouchState.Crouched;
            else if (CrouchState == CrouchState.Decrouching) {
                if (charController.height == InitialHeight) SetToDecrouched();
                else if (boxCastUp.HitLastFrame) MuteCrouchUntil(() => !boxCastUp.HitLastFrame);
            }
        }

        /// <summary>
        /// Sets the new height of the CCT with adjusting the center so the childrens save their relative position
        /// </summary>
        /// <param name="newHeight"></param>
        private void SetHeight(float newHeight) {
            previousHeight = charController.height;

            charController.height = newHeight;

            //adjust center
            float heightDelta = previousHeight - charController.height;
            Vector3 targetCenter = charController.center.Add(y: heightDelta / 2);
            charController.center = targetCenter;

            //adjust y locPosition
            charController.enabled = false;
            transform.Translate(-Vector3.up * heightDelta);
            charController.enabled = true;
        }


        private void SetToDecrouched() { 
            heightTween.Pause();

            CrouchState = CrouchState.Decrouched;
        }
        private void SetToDecrouching() {
            heightTween.ChangeValues(charController.height, InitialHeight, crouchingTime).Play();
            CrouchState = CrouchState.Decrouching;
        }
        private void SetToCrouching() {
            heightTween.ChangeValues(charController.height, crouchedHeight, crouchingTime).Play();
            CrouchState = CrouchState.Crouching;
        }

        private void MuteCrouchUntil(Func<bool> condition) {
            heightTween.Pause();
            crouchMuted = true;

            ConditionActionManager.ConstructConditionActions(this, condition, Unmute);

            void Unmute() {
                crouchMuted = false;
                heightTween.Play();
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
            if (CrouchState != CrouchState.Decrouched) throw new Exception("Current crouch state isn't Decrouched");

            heightMultiplier = Mathf.Clamp01(heightMultiplier);
            float targetLowerHeight = InitialHeight * heightMultiplier;

            var tweenSequence = DOTween.Sequence();
            var bend = DOTween.To(() => charController.height, SetHeight, targetLowerHeight, time).SetEase(Ease.InOutSine);

            if (onCrouchAchieved != null) bend.OnComplete(onCrouchAchieved);

            tweenSequence.Append(bend);
            tweenSequence.Append(
                DOTween.To(() => charController.height, SetHeight, InitialHeight, time)
                    .SetEase(Ease.InSine)
            );

            if (onFinished != null) tweenSequence.OnComplete(onFinished);
        }
#nullable disable

        /// <summary>
        /// For CrouchableUnder only
        /// </summary>
        /// <param name="targetHeight"></param>
        /// <param name="condition"></param>
        public void CrouchToUntil(float targetHeight, Func<bool> condition, Action crouchableUnmute) {
            float heightOnCall = charController.height;
            float stepOffsetOnCall = charController.stepOffset;

            charController.stepOffset = 0;

            heightTween.ChangeValues(charController.height, targetHeight, crouchingTime * 2).Play();
            CrouchState = CrouchState.Under;

            ConditionActionManager.DeleteConditionActionsOfContainer(this);
            ConditionActionManager.ConstructConditionActions(this, condition, ReactToCondition);

            void ReactToCondition() {
                heightTween.ChangeValues(charController.height, heightOnCall, crouchingTime * 2).OnComplete(ReactToCompletion).Play();

                void ReactToCompletion() {
                    CrouchState = CrouchState.Crouched;
                    charController.stepOffset = stepOffsetOnCall;
                    heightTween.onComplete -= ReactToCompletion;
                    crouchableUnmute();
                }
            }
        }
    }
}
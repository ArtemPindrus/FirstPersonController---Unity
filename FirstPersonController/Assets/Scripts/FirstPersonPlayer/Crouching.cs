using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;

namespace FirstPersonPlayer {
    [RequireComponent(typeof(CharacterController))]
    public class Crouching : MonoBehaviour {
        [SerializeField, Range(0f, 10f), Tooltip("Time in seconds to crouch")] private float crouchingTime = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("How much of initial heigh is a crouching height")] private float crouchingHeightMult = 0.5f;
        [SerializeField, Range(0.01f, 1f), Tooltip("While uncrouching the player will stop if he encounters obstacle above within the given distance. \n" +
            "Note that the value doesn't change during the runtime!")] 
            private float overhead = 0.1f;

        public float InitialHeight { get; private set; }
        public bool IsCrouching { get; private set; } = false;


        private CharacterController charController;
        private PlayerInput playerInput;
        private BoxCast rayFromAbove;
        private TweenerCore<float, float, FloatOptions> heightTween;

        private float previousHeight;


        private void Start() {
            charController = GetComponent<CharacterController>();
            InitialHeight = charController.height;

            //input
            playerInput = PlayerInputSingleton.Instance;
            playerInput.Player.Crouch.performed += HandleCrouch;

            //initialize tween
            float crouchingHeight = crouchingHeightMult * charController.height;
            heightTween = DOTween.To(() => charController.height, SetHeight, crouchingHeight, crouchingTime)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(false);
            SetToDecrouch();

            //initialize boxcast
            GameObject empty = new("BoxCastAbove");
            empty.transform.parent = transform;
            empty.transform.localPosition = Vector3.zero + Vector3.up * (charController.height / 2);

            rayFromAbove = empty.AddComponent<BoxCast>()
                .Initialize(new(charController.radius, 0.01f, charController.radius), Vector3.up, overhead, false);
        }
        private void HandleCrouch(InputAction.CallbackContext _) {
            if (charController.isGrounded) IsCrouching = !IsCrouching;
        }

        private void Update() {
            previousHeight = charController.height;

            if (IsCrouching) SetToCrouch();
            else if (!IsCrouching) {
                if (!rayFromAbove.HitLastFrame) SetToDecrouch();
                else SetToInactive();
            }
        }

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


        private void SetToDecrouch() => heightTween.PlayBackwards();
        private void SetToCrouch() => heightTween.PlayForward();
        private void SetToInactive() => heightTween.Pause();

#nullable enable
        /// <summary>
        /// Interpolates current height to the target value and back to initial height to simulate knees bend
        /// </summary>
        /// <param name="heightMultiplier">How much of initial height is a target value</param>
        /// <param name="time">Time that will be applied to both interpolations</param>
        /// <param name="onCrouchAchieved"></param>
        /// <param name="onFinished">Callback for finished bend</param>
        public void Bend(float heightMultiplier, float time, TweenCallback? onCrouchAchieved, TweenCallback? onFinished) {
            heightTween.Pause();

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

            tweenSequence.OnComplete(OnSequenceCompletion);
            if (onFinished != null) tweenSequence.OnComplete(onFinished);


            void OnSequenceCompletion() => heightTween.Play();
        }
#nullable disable
    }
}
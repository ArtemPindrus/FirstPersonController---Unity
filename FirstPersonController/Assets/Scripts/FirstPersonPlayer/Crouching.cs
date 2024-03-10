using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace FirstPersonPlayer {
    [RequireComponent(typeof(CharacterController))]
    public class Crouching : MonoBehaviour {
        [SerializeField, Range(0f, 10f), Tooltip("Time in seconds to crouch")] private float crouchingTime = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("How much of initial heigh is a crouching height")] private float crouchingHeightMult = 0.5f;


        public bool IsCrouching { get; private set; } = false;


        private CharacterController charController;
        private PlayerInput playerInput;
        private BoxCast rayFromAbove;
        private TweenerCore<float, float, FloatOptions> heightTween;

        private float previousHeight;


        private void Start() {
            charController = GetComponent<CharacterController>();

            //input
            playerInput = PlayerInputSingleton.Instance;
            playerInput.Player.Crouch.performed += HandleCrouch;

            //initialize lerping manager
            float crouchingHeight = crouchingHeightMult * charController.height;
            heightTween = DOTween.To(() => charController.height, SetHeight, crouchingHeight, crouchingTime).SetEase(Ease.InOutSine).SetAutoKill(false);
            SetToDecrouch();

            //initialize boxcast
            GameObject empty = new("BoxCastAbove");
            empty.transform.parent = transform;
            empty.transform.localPosition = Vector3.zero + Vector3.up * (charController.height / 2);

            rayFromAbove = empty.AddComponent<BoxCast>().Initialize(new(charController.radius, 0.01f, charController.radius), Vector3.up, 0.1f, false);
        }

        private void SetHeight(float newHeight) {
            previousHeight = charController.height;

            charController.height = newHeight;

            //adjust center
            float heightDelta = previousHeight - charController.height;
            Vector3 currentCenter = charController.center;
            currentCenter.y += heightDelta / 2;
            charController.center = currentCenter;

            //adjust y locPosition
            charController.enabled = false;
            transform.Translate(-Vector3.up * heightDelta);
            charController.enabled = true;
        }


        private void SetToDecrouch() => heightTween.PlayBackwards();
        private void SetToCrouch() => heightTween.PlayForward();
        private void SetToInactive() => heightTween.Pause();


        private void Update() {
            previousHeight = charController.height;

            if (IsCrouching) SetToCrouch();
            else if (!IsCrouching) {
                if (!rayFromAbove.HitLastFrame) SetToDecrouch();
                else SetToInactive();
            }
        }

        private void HandleCrouch(InputAction.CallbackContext _) { 
            if (charController.isGrounded) IsCrouching = !IsCrouching;
        }
    }
}
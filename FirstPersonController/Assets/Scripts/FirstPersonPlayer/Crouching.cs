using Lerping;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstPersonPlayer {
    [RequireComponent(typeof(CharacterController))]
    public class Crouching : MonoBehaviour {
        [SerializeField, Range(0f, 10f), Tooltip("Time in seconds to crouch")] private float crouchingTime = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("How much of initial heigh is a crouching height")] private float crouchingHeightMult = 0.5f;


        public bool IsCrouching { get; private set; } = false;


        private CharacterController charController;
        private PlayerInput playerInput;
        private BoxCast rayFromAbove;
        private LerpFloat lerping;


        private float previousHeight;


        private void Start() {
            charController = GetComponent<CharacterController>();

            //input
            playerInput = new();
            playerInput.Player.Enable();
            playerInput.Player.Crouch.performed += HandleCrouch;

            //initialize lerping manager
            float initialHeight = charController.height;
            float crouchingHeight = crouchingHeightMult * initialHeight;
            lerping = LerpFloat.Initialize(initialHeight, crouchingHeight, SetHeight, crouchingTime, false);

            //initialize boxcast
            GameObject empty = new("BoxCastAbove");
            empty.transform.parent = transform;
            empty.transform.localPosition = Vector3.zero + Vector3.up * (charController.height / 2);

            rayFromAbove = empty.AddComponent<BoxCast>();
            rayFromAbove.Initialize(new(charController.radius, 0.01f, charController.radius), Vector3.up, 0.1f, true);


            //fncs
            void SetHeight(float newHeight) {
                previousHeight = charController.height;

                charController.height = newHeight;

                //adjust centre
                float heightDelta = previousHeight - charController.height;
                Vector3 currentCenter = charController.center;
                currentCenter.y += heightDelta / 2;
                charController.center = currentCenter;

                //adjust y locPosition
                charController.enabled = false;
                transform.Translate(-Vector3.up * heightDelta);
                charController.enabled = true;
            }
        }

        private void Update() {
            previousHeight = charController.height;

            if (IsCrouching) lerping.SetDirection(LerpDirection.Addition);
            else if (!IsCrouching) {
                if (!rayFromAbove.HitLastFrame) lerping.SetDirection(LerpDirection.Substraction);
                else lerping.SetDirection(LerpDirection.Inactive);
            }
        }

        private void HandleCrouch(InputAction.CallbackContext _) { 
            if (charController.isGrounded) IsCrouching = !IsCrouching;
        }
    }
}
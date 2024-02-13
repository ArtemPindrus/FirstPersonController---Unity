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


        private float initialHeight;
        private float crouchingHeight;
        private float previousHeight;

        private float timeElapsed = 0;

        private void Start() {
            charController = GetComponent<CharacterController>();

            playerInput = new();
            playerInput.Player.Enable();
            playerInput.Player.Crouch.performed += HandleCrouch;

            initialHeight = charController.height;
            crouchingHeight = crouchingHeightMult * initialHeight;

            //initialize boxcast
            GameObject empty = new("BoxCastAbove");
            empty.transform.parent = transform;
            empty.transform.localPosition = Vector3.zero + Vector3.up * (charController.height / 2);

            rayFromAbove = empty.AddComponent<BoxCast>();
            rayFromAbove.Initialize(new(charController.radius, 0.1f, charController.radius), Vector3.up, 0.01f);
        }

        private void Update() {
            previousHeight = charController.height;

            //change time
            if (IsCrouching) timeElapsed += Time.deltaTime;
            else if (!IsCrouching && !rayFromAbove.HitLastFrame) timeElapsed -= Time.deltaTime;
            timeElapsed = Mathf.Clamp(timeElapsed, 0, crouchingTime);

            //calculate new height
            float percent = timeElapsed / crouchingTime;
            float t = percent * percent * (3f - 2f * percent);

            charController.height = Mathf.Lerp(initialHeight, crouchingHeight, t);

            //handle center change
            float heightDelta = previousHeight - charController.height;
            Vector3 currentCenter = charController.center;
            currentCenter.y += heightDelta / 2;
            charController.center = currentCenter;

            //correct transfrom position on uncrouch 
            if (heightDelta > 0) {
                charController.enabled = false;
                transform.Translate(-Vector3.up * heightDelta);
                charController.enabled = true;
            }
        }

        private void HandleCrouch(InputAction.CallbackContext _) { 
            if (charController.isGrounded) IsCrouching = !IsCrouching;
        }
    }
}
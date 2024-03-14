using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace FirstPersonPlayer {

    public enum JumpState { None, BendingKnees, Midair }

    [RequireComponent(typeof(Crouching))]

    public class BasicMovement : MonoBehaviour {
        [Header("Walking:")]
            [SerializeField, Tooltip("(units/second)"), Range(0, 10)] private float walkingSpeed = 1f;
            [SerializeField, Tooltip("(units/secondSqr)"), Range(0, 20)] private float acceleration = 2f;
            [SerializeField, Tooltip("(units/secondSqr)"), Range(0, 20)] private float deceleration = 4f;

        [field: Header("Running:")]
            [field: SerializeField] public bool RunningIsAllowed { get; private set; } = true;
            [SerializeField, Range(0, 10)] private float runningSpeedIncrease = 0.5f;

        [field: Header("Jump:")]
            [field: SerializeField] public bool JumpingIsAllowed { get; private set; } = true;
            [SerializeField, Range(0, 10)] private float jumpPower = 4;
            [SerializeField, Range(0, 100), Tooltip("CCT velocity magnitude must be lower then the given value to perform knees bend.")] 
                private float limitBendVelMgn = 1f;
            [SerializeField, Range(0f, 1f), Tooltip("How much of CCT initial height is the target to bend to")] 
                private float jumpBendHeightMultiplier = 0.98f;
            [SerializeField, Range(0, 5), Tooltip("Time to bend knees for jump")] private float jumpBendTime = 0.4f;

        [Header("Crouching:")]
            [SerializeField, Range(0, 10)] private float crouchingSpeedDecrease = 0.3f;
            [SerializeField, Range(0, 10), Tooltip("Amount of speed decrease when player is under an object (extra crouching)")] private float underSpeedDecrease = 0.6f;


        private CharacterController charController;
        private Crouching crouching;
        private InputSystem.PlayerActions playerInput;

        private float initialWalkingSpeed;

        private JumpState jumpState;

        private void Awake() {
            charController = GetComponent<CharacterController>();
            crouching = GetComponent<Crouching>();

            playerInput = InputSystem.Instance.Player;

            initialWalkingSpeed = walkingSpeed;
            playerInput.Jump.performed += HandleJump;
        }

        private float verticalVelocity;

        public Vector2 CurrentMoveInput => playerInput.Move.ReadValue<Vector2>();
        private float CurrentMovementSpeed { get; set; }
        private Vector3 movementDirection;

        private void Update() {
            //Find the displacement
            UpdateVerticalVelocity();
            UpdateMovementSpeed();
            UpdateMovementDirection();

            Vector3 displacement = Time.deltaTime* CurrentMovementSpeed * movementDirection;
            displacement.y = verticalVelocity * Time.deltaTime;

            //move
            charController.Move(displacement);
            if (charController.isGrounded && jumpState == JumpState.Midair) jumpState = JumpState.None;


            //fncs
            void UpdateMovementSpeed() {
                if (jumpState != JumpState.None) return;

                float targetMovementSpeed = 0;
                if (CurrentMoveInput != Vector2.zero) {
                    if (RunningIsAllowed && crouching.CrouchState == CrouchState.Decrouched && playerInput.Run.IsPressed()) targetMovementSpeed = walkingSpeed + runningSpeedIncrease;
                    else targetMovementSpeed = walkingSpeed;

                    if (crouching.CrouchState == CrouchState.Under) targetMovementSpeed -= underSpeedDecrease;
                    else if (crouching.CrouchState != CrouchState.Decrouched) targetMovementSpeed -= crouchingSpeedDecrease;
                }

                if (targetMovementSpeed < 0) targetMovementSpeed = 0;

                float frameSpeedChange = CurrentMovementSpeed < targetMovementSpeed ? acceleration : deceleration;
                frameSpeedChange *= Time.deltaTime;

                CurrentMovementSpeed = Mathf.MoveTowards(CurrentMovementSpeed, targetMovementSpeed, frameSpeedChange);
            }
            void UpdateMovementDirection() {
                if (jumpState != JumpState.None) return;

                if (CurrentMoveInput == Vector2.zero && CurrentMovementSpeed == 0) movementDirection = Vector3.zero;
                else if (CurrentMoveInput != Vector2.zero && charController.isGrounded && jumpState == JumpState.None) {
                    movementDirection = transform.TransformDirection(new(CurrentMoveInput.x, 0, CurrentMoveInput.y));
                }
            }
            void UpdateVerticalVelocity() {
                if (charController.isGrounded && jumpState == JumpState.None) verticalVelocity = -1;
                else if (!charController.isGrounded || jumpState == JumpState.Midair) verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }
        }

        public void HandleJump(InputAction.CallbackContext context) {
            if (!JumpingIsAllowed) return;
            if (crouching.CrouchState == CrouchState.Decrouched) {
                if (charController.velocity.magnitude <= limitBendVelMgn) {
                    crouching.Bend(jumpBendHeightMultiplier, jumpBendTime, Jump, null);
                    jumpState = JumpState.BendingKnees;
                } else Jump();
            }


            void Jump() {
                verticalVelocity = jumpPower;
                jumpState = JumpState.Midair;
            }
        }

        public void ResetWalkingSpeed() => walkingSpeed = initialWalkingSpeed;
    }
}
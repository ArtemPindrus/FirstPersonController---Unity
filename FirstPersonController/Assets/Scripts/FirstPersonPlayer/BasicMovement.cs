using UnityEngine;
using Extensions;
using System;
using Assets.Scripts.Extensions;

namespace FirstPersonPlayer {

    public enum JumpState { None, BendingKnees, Midair, Unbend }

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
            [SerializeField, Range(0f, 1f), Tooltip("How much of CCT initial height is the target to bend to")] 
                private float jumpBendHeightMultiplier = 0.98f;
            [SerializeField, Range(0, 5), Tooltip("Time to bend knees for jump")] private float jumpBendTime = 0.4f;

        [Header("Crouching:")]
            [SerializeField, Range(0, 10)] private float crouchingSpeedDecrease = 0.3f;


        private CharacterController charController;
        private Crouching crouching;
        private PlayerInput playerInput;

        private float initialWalkingSpeed;

        private JumpState jumpState;

        private void Awake() {
            charController = GetComponent<CharacterController>();
            crouching = GetComponent<Crouching>();

            playerInput = PlayerInputSingleton.Instance;

            initialWalkingSpeed = walkingSpeed;
            charController.OnGrounded(HandleOnGrounded);

            void HandleOnGrounded() => jumpState = JumpState.None;
        }

        private float verticalVelocity;
        private float currentMovementSpeed;
        private Vector3 movementDirection;

        private void Update() {
            Vector2 currentMoveInput = playerInput.Player.Move.ReadValue<Vector2>();

            //Find the displacement
            UpdateVerticalVelocity();
            UpdateMovementSpeed();
            UpdateMovementDirection();

            Vector3 displacement = Time.deltaTime* currentMovementSpeed * movementDirection;
            displacement.y = verticalVelocity * Time.deltaTime;

            //move
            charController.Move(displacement);


            //fncs
            void UpdateMovementSpeed() {
                if (jumpState != JumpState.None) return;

                float targetMovementSpeed = 0;
                if (currentMoveInput != Vector2.zero) {
                    if (RunningIsAllowed && !crouching.IsCrouching && playerInput.Player.Run.IsPressed()) targetMovementSpeed = walkingSpeed + runningSpeedIncrease;
                    else targetMovementSpeed = walkingSpeed;

                    if (crouching.IsCrouching) targetMovementSpeed -= crouchingSpeedDecrease;
                }

                float frameSpeedChange = currentMovementSpeed < targetMovementSpeed ? acceleration : deceleration;
                frameSpeedChange *= Time.deltaTime;

                currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, targetMovementSpeed, frameSpeedChange);
            }
            void UpdateMovementDirection() {
                if (jumpState != JumpState.None) return;

                if (currentMoveInput == Vector2.zero && currentMovementSpeed == 0) movementDirection = Vector3.zero;
                else if (currentMoveInput != Vector2.zero && charController.isGrounded && jumpState == JumpState.None) {
                    movementDirection = transform.TransformDirection(new(currentMoveInput.x, 0, currentMoveInput.y));
                }
            }
            void UpdateVerticalVelocity() {
                if (charController.isGrounded && jumpState == JumpState.None) {
                    if (JumpingIsAllowed && playerInput.Player.Jump.WasPressedThisFrame() && !crouching.IsCrouching) Jump();
                    else verticalVelocity = -1;
                } else if (!charController.isGrounded || jumpState == JumpState.Midair) verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }
        }

        public void Jump() {
            jumpState = JumpState.BendingKnees;

            crouching.Bend(jumpBendHeightMultiplier, jumpBendTime, HendleCrouchAchieved, HendleUnbend);
            

            void HendleCrouchAchieved() {
                verticalVelocity = jumpPower;
                jumpState = JumpState.Midair;
            }
            void HendleUnbend() => jumpState = JumpState.Unbend;
        }

        public void ResetWalkingSpeed() => walkingSpeed = initialWalkingSpeed;
    }
}
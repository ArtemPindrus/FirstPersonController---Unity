using UnityEngine;
using Extensions;
using System;


namespace FirstPersonPlayer {
    [RequireComponent(typeof(Crouching))]
    public class BasicMovement : MonoBehaviour {
        [Header("Walking:")]
            [SerializeField, Tooltip("(units/second)"), Range(0, 10)] private float walkingSpeed = 1f;
            [SerializeField, Tooltip("(units/secondSqr)"), Range(0,10)] private float acceleration = 2f;
            [SerializeField, Tooltip("(units/secondSqr)"), Range(0, 10)] private float deceleration = 4f;

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

        private bool isJumping;

        private void Awake() {
            charController = GetComponent<CharacterController>();
            crouching = GetComponent<Crouching>();

            playerInput = PlayerInputSingleton.Instance;

            initialWalkingSpeed = walkingSpeed;
        }

        private float verticalVelocity;
        private float currentMovementSpeed;
        private Vector2 CurrentMoveInput => playerInput.Player.Move.ReadValue<Vector2>();
#nullable enable
        private Vector3? directionOnOffGround = null;
#nullable disable


        private void Update() {
            //Update VerticalVelocity
            if (charController.isGrounded) {
                if (JumpingIsAllowed && !isJumping && playerInput.Player.Jump.WasPressedThisFrame() && !crouching.IsCrouching) Jump();
                else if (!isJumping) verticalVelocity = -1;
            } else verticalVelocity += Physics.gravity.y * Time.deltaTime;

            //Find the displacement
            UpdateMovementSpeed();

            Vector3 direction;
            if (CurrentMoveInput != Vector2.zero && charController.isGrounded) {
                directionOnOffGround = null;
                direction = transform.TransformDirection(new(CurrentMoveInput.x, 0, CurrentMoveInput.y));
            } else {
                if (directionOnOffGround == null) directionOnOffGround = transform.TransformDirection(new(CurrentMoveInput.x, 0, CurrentMoveInput.y));
                direction = directionOnOffGround.Value;
            }

            Vector3 displacement = Time.deltaTime * currentMovementSpeed * direction;


            displacement.y = verticalVelocity * Time.deltaTime;

            //move
            charController.Move(displacement);



            //fncs
            void UpdateMovementSpeed() {
                float targetMovementSpeed = 0;
                if (CurrentMoveInput != Vector2.zero) {
                    if (RunningIsAllowed && !crouching.IsCrouching && playerInput.Player.Run.IsPressed()) targetMovementSpeed = walkingSpeed + runningSpeedIncrease;
                    else targetMovementSpeed = walkingSpeed;

                    if (crouching.IsCrouching) targetMovementSpeed -= crouchingSpeedDecrease;
                }

                if (currentMovementSpeed < targetMovementSpeed) {
                    float frameAcceleration = acceleration * Time.deltaTime;

                    currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, targetMovementSpeed, frameAcceleration);
                } else if (currentMovementSpeed > targetMovementSpeed) {
                    float frameDeceletation = deceleration * Time.deltaTime;

                    currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, targetMovementSpeed, frameDeceletation);
                }
            }
        }

        public void Jump() {
            isJumping = true;
            crouching.Bend(jumpBendHeightMultiplier, jumpBendTime, ReactToCrouch, ReactToUncrouch);
            

            void ReactToCrouch() => verticalVelocity = jumpPower;
            void ReactToUncrouch() => isJumping = false;
        }

        public void ResetWalkingSpeed() => walkingSpeed = initialWalkingSpeed;
    }
}
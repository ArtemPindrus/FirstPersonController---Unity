using UnityEngine;

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

        [Header("Crouching:")]
            [SerializeField, Range(0, 10)] private float crouchingSpeedDecrease = 0.3f;


        private CharacterController charController;
        private Crouching crouching;
        private PlayerInput playerInput;

        private float initialWalkingSpeed;

        private void Awake() {
            charController = GetComponent<CharacterController>();
            crouching = GetComponent<Crouching>();

            playerInput = PlayerInputSingleton.Instance;

            //charController.detectCollisions = false;

            initialWalkingSpeed = walkingSpeed;
        }

        private float verticalVelocity;
        private float currentMovementSpeed;

        private void Update() {
            Vector2 currentMoveInput = playerInput.Player.Move.ReadValue<Vector2>();

            UpdateMovementSpeed();

            //Update VerticalVelocity
            if (charController.isGrounded) {
                if (JumpingIsAllowed && playerInput.Player.Jump.WasPressedThisFrame() && !crouching.IsCrouching) verticalVelocity = jumpPower;
                else verticalVelocity = -1;
            } else verticalVelocity += Physics.gravity.y * Time.deltaTime;

            //Find the displacement
            Vector3 displacement;
            if (currentMoveInput != Vector2.zero && charController.isGrounded) {
                Vector3 direction = transform.TransformDirection(new(currentMoveInput.x, 0, currentMoveInput.y));

                displacement = Time.deltaTime * currentMovementSpeed * direction;
            } else {
                Vector3 direction = charController.velocity;
                direction.y = 0;
                direction.Normalize();

                displacement = Time.deltaTime * currentMovementSpeed * direction;
            }

            displacement.y = verticalVelocity * Time.deltaTime;

            //move
            charController.Move(displacement);



            //fncs
            void UpdateMovementSpeed() {
                float targetMovementSpeed = 0;
                if (currentMoveInput != Vector2.zero) {
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

        public void ResetWalkingSpeed() => walkingSpeed = initialWalkingSpeed;
    }
}
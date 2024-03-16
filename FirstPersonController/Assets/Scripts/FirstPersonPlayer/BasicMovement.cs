using UnityEngine;
using UnityEngine.InputSystem;
using Input;

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


        private CharacterController _charController;
        private Crouching _crouching;
        private InputAsset.PlayerActions _playerInput;

        public Vector2 CurrentMoveInput => _playerInput.Move.ReadValue<Vector2>();
        private float CurrentMovementSpeed { get; set; }
        private float _verticalVelocity;
        private Vector3 _movementDirection;


        private JumpState _jumpState;

        private void Awake() {
            _charController = GetComponent<CharacterController>();
            _crouching = GetComponent<Crouching>();

            _playerInput = InputAsset.Instance.Player;

            _playerInput.Jump.performed += HandleJump;
        }





        private void Update() {
            //Find the displacement
            UpdateVerticalVelocity();
            UpdateMovementSpeed();
            UpdateMovementDirection();

            Vector3 displacement = Time.deltaTime* CurrentMovementSpeed * _movementDirection;
            displacement.y = _verticalVelocity * Time.deltaTime;

            //move
            _charController.Move(displacement);
            if (_charController.isGrounded && _jumpState == JumpState.Midair) _jumpState = JumpState.None;


            //fncs
            void UpdateMovementSpeed() {
                if (_jumpState != JumpState.None) return;

                float targetMovementSpeed = 0;
                if (CurrentMoveInput != Vector2.zero) {
                    if (RunningIsAllowed && _crouching.CrouchState == CrouchState.Standing && _playerInput.Run.IsPressed()) targetMovementSpeed = walkingSpeed + runningSpeedIncrease;
                    else targetMovementSpeed = walkingSpeed;

                    if (_crouching.CrouchState == CrouchState.Under) targetMovementSpeed -= underSpeedDecrease;
                    else if (_crouching.CrouchState != CrouchState.Standing) targetMovementSpeed -= crouchingSpeedDecrease;
                }

                if (targetMovementSpeed < 0) targetMovementSpeed = 0;

                float frameSpeedChange = CurrentMovementSpeed < targetMovementSpeed ? acceleration : deceleration;
                frameSpeedChange *= Time.deltaTime;

                CurrentMovementSpeed = Mathf.MoveTowards(CurrentMovementSpeed, targetMovementSpeed, frameSpeedChange);
            }
            void UpdateMovementDirection() {
                if (_jumpState != JumpState.None) return;

                if (CurrentMoveInput == Vector2.zero && CurrentMovementSpeed == 0) _movementDirection = Vector3.zero;
                else if (CurrentMoveInput != Vector2.zero && _charController.isGrounded && _jumpState == JumpState.None) {
                    _movementDirection = transform.TransformDirection(new(CurrentMoveInput.x, 0, CurrentMoveInput.y));
                }
            }
            void UpdateVerticalVelocity() {
                if (_charController.isGrounded && _jumpState == JumpState.None) _verticalVelocity = -1;
                else if (!_charController.isGrounded || _jumpState == JumpState.Midair) _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }
        }

        public void HandleJump(InputAction.CallbackContext context) {
            if (!JumpingIsAllowed) return;
            if (_jumpState == JumpState.None && _crouching.CrouchState == CrouchState.Standing && _charController.isGrounded) {
                if (_charController.velocity.magnitude <= limitBendVelMgn) {
                    _crouching.Bend(jumpBendHeightMultiplier, jumpBendTime, Jump, null);
                    _jumpState = JumpState.BendingKnees;
                } else Jump();
            }


            void Jump() {
                _verticalVelocity = jumpPower;
                _jumpState = JumpState.Midair;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using Input;
using System.Collections.Generic;
using Additional;

namespace FirstPersonController {
    [RequireComponent(typeof(CharacterController))]
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


        private CharacterController charController;
        private MyInput.PlayerMovementActions _playerInput;

        public Vector2 CurrentMoveInput => _playerInput.Move.ReadValue<Vector2>();
        private float CurrentMovementSpeed { get; set; }
        private float verticalVelocity;
        private Vector3 _movementDirection;



        private List<FloatClass> speedReducers;

        private void Awake() {
            charController = GetComponent<CharacterController>();

            _playerInput = MyInput.Instance.PlayerMovement;

            speedReducers = new();

            _playerInput.Jump.performed += HandleJump;
        }

        private void Update() {
            //Find the displacement
            UpdateVerticalVelocity();
            UpdateMovementSpeed();
            UpdateMovementDirection();

            Vector3 displacement = Time.deltaTime* CurrentMovementSpeed * _movementDirection;
            displacement.y = verticalVelocity * Time.deltaTime;

            //move
            charController.Move(displacement);


            //fncs
            void UpdateMovementSpeed() {
                float targetMovementSpeed = 0;
                if (CurrentMoveInput != Vector2.zero) {
                    if (RunningIsAllowed && _playerInput.Run.IsPressed()) targetMovementSpeed = walkingSpeed + runningSpeedIncrease;
                    else targetMovementSpeed = walkingSpeed;
                }

                foreach (var reduction in speedReducers) targetMovementSpeed -= reduction.Value;
                if (targetMovementSpeed < 0) targetMovementSpeed = 0;

                float frameSpeedChange = CurrentMovementSpeed < targetMovementSpeed ? acceleration : deceleration;
                frameSpeedChange *= Time.deltaTime;

                CurrentMovementSpeed = Mathf.MoveTowards(CurrentMovementSpeed, targetMovementSpeed, frameSpeedChange);
            }
            void UpdateMovementDirection() {
                if (CurrentMoveInput == Vector2.zero && CurrentMovementSpeed == 0) _movementDirection = Vector3.zero;
                else if (CurrentMoveInput != Vector2.zero && charController.isGrounded) {
                    _movementDirection = transform.TransformDirection(new(CurrentMoveInput.x, 0, CurrentMoveInput.y));
                }
            }
            void UpdateVerticalVelocity() {
                if (charController.isGrounded && verticalVelocity < -1) verticalVelocity = -1;
                else if (!charController.isGrounded) verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }
        }

        public void HandleJump(InputAction.CallbackContext context) {
            if (JumpingIsAllowed && charController.isGrounded) {
                verticalVelocity = jumpPower;
            }
        }

        public FloatClass AddSpeedReducer(float value) {
            FloatClass floatClass = new(value);
            speedReducers.Add(floatClass);

            return floatClass;
        }

        public void MuteRunning() => RunningIsAllowed = false;
        public void ResumeRunning() => RunningIsAllowed = true;

        public void MuteJumping() => JumpingIsAllowed = false;
        public void ResumeJumping() => JumpingIsAllowed = true;
    }
}
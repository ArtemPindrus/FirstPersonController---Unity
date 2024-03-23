using Casting;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DragAndDropSystem {
    [RequireComponent(typeof(Rigidbody))]
    public class Draggable : MonoBehaviour {
        [field: SerializeField] public bool IsRotatable { get; private set; }
        [field: SerializeField] public bool IsMovableUpwards { get; private set; }

        private Transform playerCamera;
        public Rigidbody RB { get; private set; }
        private RigidBodyData startingRBData;

        private float draggingSpeed;
        private float maxDistanceFromCamera;
        private float rotationSensitivity;
#nullable enable
        private DragNDropObjects? dragger;
#nullable disable

        public bool IsBeingDragged { get; private set; }
        public bool IsBeingRotated { get; private set; }

        private void Awake() {
            RB = GetComponent<Rigidbody>();
            playerCamera = Camera.main.transform;

            startingRBData = new(RB.constraints, RB.interpolation);
        }

        private void Update() {
            if (IsBeingDragged) {
                if (Vector3.Distance(playerCamera.position, transform.position) > maxDistanceFromCamera) Drop();

                Vector3 direction = (playerCamera.position + playerCamera.forward) - transform.position;
                Debug.DrawLine(transform.position, transform.position + direction);

                Vector3 newVelocity = direction * draggingSpeed;
                if (!IsMovableUpwards) newVelocity.y = RB.velocity.y;

                RB.velocity = newVelocity;

                if (IsBeingRotated) {
                    Vector2 mouseDelta = Mouse.current.delta.value;
                    Vector2 rotationDelta = mouseDelta * rotationSensitivity;

                    transform.Rotate(Vector3.up, rotationDelta.x, Space.World);
                    transform.Rotate(dragger.transform.right, rotationDelta.y, Space.World);
                }
            }
        }

        public void PickUp(float draggingSpeed, float maxDistanceFromCamera, DragNDropObjects dragger) {
            Debug.Log("Picked");

            IsBeingDragged = true;

            this.draggingSpeed = draggingSpeed;
            this.maxDistanceFromCamera = maxDistanceFromCamera;
            this.dragger = dragger;

            RB.freezeRotation = true;
            RB.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Drop() {
            Debug.Log("Dropped");

            IsBeingDragged = false;

            draggingSpeed = 0;
            maxDistanceFromCamera = 0;
            dragger = null;

            RB.constraints = startingRBData.Constraints;
            RB.interpolation = startingRBData.Interpolation;

            StopRotation();
        }

        public void StartRotating(float rotationSensitivity) {
            IsBeingRotated = true;

            this.rotationSensitivity = rotationSensitivity;

            RB.interpolation = RigidbodyInterpolation.None;
        }

        public void StopRotation() {
            IsBeingRotated = false;

            rotationSensitivity = 0;

            if (IsBeingDragged) RB.interpolation = RigidbodyInterpolation.Interpolate;
            else RB.interpolation = startingRBData.Interpolation;
        }

        public void Throw(Vector3 throwForce) {
            Drop();

            RB.AddForce(throwForce, ForceMode.Impulse);
        }
    }
}
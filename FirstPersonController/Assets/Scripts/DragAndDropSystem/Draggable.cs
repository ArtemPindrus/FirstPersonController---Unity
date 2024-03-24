using PhysicsCasting;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

namespace DragAndDropSystem {
    [RequireComponent(typeof(Rigidbody))]
    public class Draggable : MonoBehaviour {
        [field: SerializeField] public bool IsRotatable { get; private set; }
        [field: SerializeField] public bool IsMovableUpwards { get; private set; }
        private bool sweepTestingForDragger; //recommended on. Prevents penetration of objects and CCT

        private Transform playerCamera;
        private Rigidbody rb;
        private RigidBodyData startingRBData;

        private float draggingSpeed;
        private float maxDistanceFromCamera;
        private float rotationSensitivity;
#nullable enable
        private DragNDropObjects? dragger;
#nullable disable

        public bool IsBeingDragged { get; private set; }
        public bool IsBeingRotated { get; private set; }
        public float Mass => rb.mass;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            playerCamera = Camera.main.transform;

            startingRBData = new(rb.constraints, rb.interpolation, rb.useGravity);
        }

        private void Update() {
            if (IsBeingDragged) {
                if (Vector3.Distance(playerCamera.position, transform.position) > maxDistanceFromCamera) {
                    dragger.AnnulateCurrentlyDraggable();
                    Drop();
                    return;
                }
                if (dragger.DropOnOppositeFacing) {
                    Vector3 directionToObject = (transform.position - playerCamera.transform.position).normalized;
                    if (Vector3.Dot(directionToObject, playerCamera.transform.forward) < 0) {
                        dragger.AnnulateCurrentlyDraggable();
                        Drop();
                        return;
                    }
                }



                Vector3 direction = (playerCamera.position + playerCamera.forward) - transform.position;
                Debug.DrawLine(transform.position, transform.position + direction);

                if (sweepTestingForDragger && SweepTestForDragger(direction)) return; 
                

                Vector3 newVelocity = direction * draggingSpeed;
                if (!IsMovableUpwards) newVelocity.y = rb.velocity.y;

                rb.velocity = newVelocity;

                if (IsBeingRotated) Rotate();
            }

            void Rotate() {
                Vector2 mouseDelta = Mouse.current.delta.value;
                Vector2 rotationDelta = mouseDelta * rotationSensitivity;

                transform.Rotate(Vector3.up, rotationDelta.x, Space.World);
                transform.Rotate(dragger.transform.right, rotationDelta.y, Space.World);
            }
            bool SweepTestForDragger(Vector3 direction) {
                RaycastHit[] hits = rb.SweepTestAll(direction, 0.2f);

                if (hits.Any(x => x.collider.TryGetComponent(out DragNDropObjects dragger) && dragger == this.dragger)) {
                    Debug.Log("dragger found");
                    rb.velocity = Vector3.zero;
                    return true;
                }

                return false;
            }
        }

        public void PickUp(float draggingSpeed, float maxDistanceFromCamera, DragNDropObjects dragger, bool sweepTestingForDragger) {
            Debug.Log("Picked");

            IsBeingDragged = true;

            this.draggingSpeed = draggingSpeed;
            this.maxDistanceFromCamera = maxDistanceFromCamera;
            this.dragger = dragger;
            this.sweepTestingForDragger = sweepTestingForDragger;

            rb.freezeRotation = true;
            if (IsMovableUpwards) rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Drop() {
            Debug.Log("Dropped");

            IsBeingDragged = false;

            draggingSpeed = 0;
            maxDistanceFromCamera = 0;
            dragger = null;

            rb.constraints = startingRBData.Constraints;
            rb.useGravity = startingRBData.UseGravity;
            rb.interpolation = startingRBData.Interpolation;

            StopRotation();
        }

        public void StartRotating(float rotationSensitivity) {
            IsBeingRotated = true;

            this.rotationSensitivity = rotationSensitivity;

            rb.interpolation = RigidbodyInterpolation.None;
        }

        public void StopRotation() {
            IsBeingRotated = false;

            rotationSensitivity = 0;

            if (IsBeingDragged) rb.interpolation = RigidbodyInterpolation.Interpolate;
            else rb.interpolation = startingRBData.Interpolation;
        }

        public void Throw(Vector3 throwForce) {
            Drop();

            rb.AddForce(throwForce, ForceMode.Impulse);
        }
    }
}
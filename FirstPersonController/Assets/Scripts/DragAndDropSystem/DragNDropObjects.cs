using PhysicsCasting;
using FirstPersonController;
using Input;
using UnityEngine;
using Work;
using System;
using Additional;

namespace DragAndDropSystem {
    [RequireComponent(typeof(MouseLook))]
    [RequireComponent(typeof(BasicMovement))]
    public class DragNDropObjects : MonoBehaviour {
        [SerializeField, Tooltip("Ray from the camera forward. Distance will be used for maximum grabbing distance.")] private RayCastInUpdate cameraRay;
        [SerializeField, Range(0, 100)] private float speedOfDragging;
        [SerializeField, Range(0, 100)] private float rotationSensitivity;
        [SerializeField, Range(0, 100)] private float throwForce;
        [SerializeField, Range(0, 100), Tooltip("How much the mass of the objects affect movement speed and mouse sensitivity")] private float draggableMassMultiplier;

        [Header("Dropping:")]
            [SerializeField, Range(0, 100), Tooltip("Addition to the grabbing distance past which the object will be dropped.")] private float dropDistanceAddition;
            [field: SerializeField, Tooltip("Drop the object when dragging and not facing it?")] public bool DropOnOppositeFacing { get; private set; } = true;

        [Header("Penetration prevention (recommended on for all):")]
            [SerializeField, Tooltip("Creates additional collider, values of which will always equal to CCT internal collider. Prevents object penetration")] private bool createAdditionalCapsuleCollider;
            [SerializeField, Tooltip("Tests for dragger in the way of draggable movement. Stops objects if there is one. Prevents object penetration")] private bool sweepTestingForDragger;


        private MyInput.DragAndDropSystemActions dragNDropActions;
        private MouseLook mouse;
        private CharacterController characterController;

        private FloatClass movementSpeedReducer;
        private FloatClass sensitivityReducer;


#nullable enable
        private Draggable? currentlyDraggable;
#nullable disable

        private void Start() {
            dragNDropActions = MyInput.Instance.DragAndDropSystem;
            mouse = GetComponent<MouseLook>();
            characterController = GetComponent<CharacterController>();

            movementSpeedReducer = GetComponent<BasicMovement>().AddSpeedReducer(0);
            sensitivityReducer = GetComponent<MouseLook>().AddSensitivityReducer(0);

            dragNDropActions.Drag.performed += Drag_performed;
            dragNDropActions.Drag.canceled += Drag_canceled;

            dragNDropActions.RotateDraggable.performed += RotateDraggable_performed;
            dragNDropActions.RotateDraggable.canceled += RotateDraggable_canceled;

            dragNDropActions.Throw.performed += Throw_performed;

            if (createAdditionalCapsuleCollider) CreateAdditionalCollider();

            void CreateAdditionalCollider() {
                CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
                WorkManager.ConstructDoWhileInUpdate(this,
                    () => true,
                    () => {
                        collider.height = characterController.height;
                        collider.radius = characterController.radius;
                        collider.center = characterController.center;
                    }
                );
            }
        }

        private void Throw_performed(UnityEngine.InputSystem.InputAction.CallbackContext _) {
            currentlyDraggable.Throw(cameraRay.transform.TransformDirection(cameraRay.Direction) * throwForce);
            AnnulateCurrentlyDraggable();
        }

        private void RotateDraggable_canceled(UnityEngine.InputSystem.InputAction.CallbackContext _) {
            if (currentlyDraggable != null && currentlyDraggable.IsBeingRotated) {
                currentlyDraggable.StopRotation();
            }
        }

        private void RotateDraggable_performed(UnityEngine.InputSystem.InputAction.CallbackContext _) {
            if (currentlyDraggable != null) {
                currentlyDraggable.StartRotating(rotationSensitivity);
                WorkManager.ConstructDoWhileInUpdate(this,
                    () => dragNDropActions.RotateDraggable.IsPressed() && currentlyDraggable != null,
                    new Action[] { () => mouse.DisableRot() },
                    () => mouse.EnableRot()
                );
            }
        }

        private void Drag_canceled(UnityEngine.InputSystem.InputAction.CallbackContext _) {
            if (currentlyDraggable != null) {
                currentlyDraggable.Drop();
                AnnulateCurrentlyDraggable();
            }
        }

        private void Drag_performed(UnityEngine.InputSystem.InputAction.CallbackContext _) {
            Collider cameraRayCollider = cameraRay.Hit.collider;
            if (cameraRayCollider != null && cameraRayCollider.TryGetComponent(out Draggable draggable)) {
                draggable.PickUp(speedOfDragging, cameraRay.Distance + dropDistanceAddition, this, sweepTestingForDragger);
                currentlyDraggable = draggable;

                movementSpeedReducer.Change(draggable.Mass * draggableMassMultiplier);
                sensitivityReducer.Change(draggable.Mass * draggableMassMultiplier);
            }
        }

        public void AnnulateCurrentlyDraggable() {
            currentlyDraggable = null;
            movementSpeedReducer.Change(0);
            sensitivityReducer.Change(0);
        }
    }
}
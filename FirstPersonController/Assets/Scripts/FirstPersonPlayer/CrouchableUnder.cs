using DG.Tweening;
using UnityEngine;

namespace FirstPersonController {
    public class CrouchableUnder : MonoBehaviour {
        [field: Header("Options:")]
            [field: SerializeField] public float TargetHeight { get; private set; }
            [field: SerializeField] public Collider TriggerCollider { get; private set; }

        [Header("Debug:")]
            [SerializeField] private bool debug;
            [SerializeField] private Vector3 position;

        public bool Muted { get; private set; }

        private void Awake() {
            if (TargetHeight == 0) throw new System.Exception("Set the target height!");
            if (TriggerCollider == null || !TriggerCollider.isTrigger) throw new System.Exception("Set the isTriggerCollider!");
        }

        private void OnDrawGizmos() {
            if (debug) Gizmos.DrawCube(transform.TransformPoint(position), new(1, TargetHeight, 1));
        }

        public void Mute() => Muted = true;
        public void Unmute() => Muted = false;
    }
}

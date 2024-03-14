using FirstPersonPlayer;
using UnityEngine;

namespace Scripts {
    internal class CrouchableUnder : MonoBehaviour {
        [SerializeField] private float targetHeight;
        [SerializeField] private Collider thisCollider;

        [Header("Debug:")]
        [SerializeField] private bool debug;
        [SerializeField] private Vector3 position;

        private static CrouchableUnder previousMuted;
        private bool muted;

        private static float? currentLowestHeight;

        private void Awake() {
            if (targetHeight == 0) throw new System.Exception("Set the target height!");
            if (thisCollider == null || !thisCollider.isTrigger) throw new System.Exception("Set the isTriggerCollider!");
        }

        private void OnTriggerStay(Collider other) {
            if (other.TryGetComponent(out Crouching crouching) && crouching.IsInCrouchUnderState && !muted) {
                if (currentLowestHeight == null ||
                    (currentLowestHeight != null && targetHeight < currentLowestHeight)) {
                    if (previousMuted != null) previousMuted.Unmute();

                    currentLowestHeight = targetHeight;
                    crouching.CrouchToUntil(
                        targetHeight, 
                        () => !thisCollider.bounds.Intersects(crouching.GetComponent<Collider>().bounds), 
                        () => {
                            muted = false;
                            previousMuted = null;
                            currentLowestHeight = null;
                        }
                    );
                    
                    muted = true;
                    previousMuted = this;
                }
            }
        }

        private void OnDrawGizmos() {
            if (debug) Gizmos.DrawCube(transform.TransformPoint(position), new(1, targetHeight, 1));
        }

        public void Unmute() => muted = false;
    }
}

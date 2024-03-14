using FirstPersonPlayer;
using UnityEngine;

namespace Scripts {
    internal class CrouchableUnder : MonoBehaviour {
        [SerializeField] private float targetHeight;
        [SerializeField] private Collider thisCollider;

        private bool muted;

        private void Awake() {
            if (targetHeight == 0) throw new System.Exception("Set the target height!");
            if (thisCollider == null || !thisCollider.isTrigger) throw new System.Exception("Set the isTriggerCollider!");
        }

        private void OnTriggerStay(Collider other) {
            if (other.TryGetComponent(out Crouching crouching) && crouching.CrouchState == CrouchState.Crouched && !muted) {
                crouching.CrouchToUntil(targetHeight, () => !thisCollider.bounds.Intersects(crouching.GetComponent<Collider>().bounds), () => muted = false);
                muted = true;
            }
        }
    }
}

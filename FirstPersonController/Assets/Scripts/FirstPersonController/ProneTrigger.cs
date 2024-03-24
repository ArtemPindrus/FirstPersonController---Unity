using System;
using UnityEngine;

namespace FirstPersonController {
    [RequireComponent(typeof(Collider))]
    public class ProneTrigger : MonoBehaviour {
        private static int activeCount;
        private bool muted;


        private void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent(out Crouching _) && !muted) {
                activeCount++;
                muted = true;
            }
        }

        private void OnTriggerStay(Collider other) {
            if (other.TryGetComponent(out Crouching player) && player.CurrentCrouchState == CrouchState.Crouched) {
                player.SetToProne();
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.TryGetComponent(out Crouching player)) {
                if (player.CurrentCrouchState == CrouchState.Prone && activeCount == 1) player.SetToCrouching();

                activeCount--;
                muted = false;
            }
        }
    }
}

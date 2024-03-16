using DG.Tweening;
using FirstPersonPlayer;
using UnityEngine;

namespace Scripts {
    internal class CrouchableUnder : MonoBehaviour {
        [SerializeField] private float targetHeight;
        [SerializeField] private Collider thisCollider;

        [Header("Debug:")]
            [SerializeField] private bool debug;
            [SerializeField] private Vector3 position;

        private static CrouchableUnder _previousMuted;
        private bool _muted;

        private static float? _currentLowestHeight;

        private void Awake() {
            if (targetHeight == 0) throw new System.Exception("Set the target height!");
            if (thisCollider == null || !thisCollider.isTrigger) throw new System.Exception("Set the isTriggerCollider!");
        }

        private void OnTriggerStay(Collider other) {
            if (!_muted && other.TryGetComponent(out Crouching player) && player.IsInCrouchUnderState) {
                if (_currentLowestHeight == null ||
                    (_currentLowestHeight != null && targetHeight < _currentLowestHeight)) {
                    if (_previousMuted != null) _previousMuted.Unmute();

                    _currentLowestHeight = targetHeight;
                    MakePlayerCrouchUnder(player);

                    _muted = true;
                    _previousMuted = this;
                }
            }
        }

        private void MakePlayerCrouchUnder(Crouching player) {
            player.CharController.stepOffset = 0;

            player.ReassignHeightTween(targetHeight, player.CrouchingTime * 2, true);
            player.CrouchState = CrouchState.Under;

            BehaviorOnConditionManager.DeleteConditionActionsOfContainer(player);
            BehaviorOnConditionManager.ConstructBehaviorOnCondition(player, PlayerLeftColliderBounds, ReactToLeftBoundaries);

            bool PlayerLeftColliderBounds() => !thisCollider.bounds.Intersects(player.GetComponent<Collider>().bounds);
            void ReactToLeftBoundaries() {
                player.ReassignHeightTween(player.CrouchedHeight, player.CrouchingTime * 2, true).OnComplete(ReactToCompletedDecrouchFromUnder);

                _muted = false;
                _currentLowestHeight = null;
            }
            void ReactToCompletedDecrouchFromUnder() {
                player.CrouchState = CrouchState.Crouched;
                player.CharController.stepOffset = player.BasicStepOffset;
            }
        }

        private void OnDrawGizmos() {
            if (debug) Gizmos.DrawCube(transform.TransformPoint(position), new(1, targetHeight, 1));
        }

        private void Unmute() => _muted = false;
    }
}

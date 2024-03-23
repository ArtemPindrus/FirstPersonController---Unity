using UnityEngine;

namespace Casting {
    public class RayCastInUpdate : MonoBehaviour {
        [field: SerializeField] public Vector3 Direction { get; private set; }
        [field: SerializeField] public float Distance { get; private set; }

        [SerializeField] private bool debug;

        public RaycastHit Hit { get; private set; }

        private void Update() {
            Physics.Raycast(transform.position, transform.TransformDirection(Direction), out RaycastHit hit, Distance);

            Hit = hit;
        }

        private void OnDrawGizmos() {
            Vector3 currentPos = transform.position;

            if (debug) Gizmos.DrawLine(currentPos, currentPos + Direction * Distance);
        }
    }
}
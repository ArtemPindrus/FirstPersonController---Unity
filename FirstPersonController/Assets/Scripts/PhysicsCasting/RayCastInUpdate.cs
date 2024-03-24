using System;
using UnityEngine;

namespace PhysicsCasting {
    public class RayCastInUpdate : MonoBehaviour {
        [field: SerializeField] public Vector3 Direction { get; private set; }
        [field: SerializeField] public float Distance { get; private set; }

        [SerializeField] private bool debug;

        public event Action<GameObject> HitLastFrame;

        public RaycastHit Hit { get; private set; }

        private void Update() {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Direction), out RaycastHit hit, Distance)) {
                HitLastFrame?.Invoke(hit.collider.gameObject);
            }

            Hit = hit;
        }

        private void OnDrawGizmos() {
            Vector3 currentPos = transform.position;

            if (debug) Gizmos.DrawLine(currentPos, currentPos + Direction * Distance);
        }

        public RayCastInUpdate Initialize(Vector3 direction, float distance, bool debug) {
            Direction = direction;
            Distance = distance;
            this.debug = debug;

            return this;
        }
    }
}
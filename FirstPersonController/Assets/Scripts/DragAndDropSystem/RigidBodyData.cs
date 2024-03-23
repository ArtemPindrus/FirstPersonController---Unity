using UnityEngine;

namespace DragAndDropSystem {
    public readonly struct RigidBodyData {
        public RigidbodyConstraints Constraints { get; }
        public RigidbodyInterpolation Interpolation { get; }

        public RigidBodyData(RigidbodyConstraints constraints, RigidbodyInterpolation interpolation) {
            Constraints = constraints;
            Interpolation = interpolation;
        }
    }
}

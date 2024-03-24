using UnityEngine;

namespace DragAndDropSystem {
    public readonly struct RigidBodyData {
        public RigidbodyConstraints Constraints { get; }
        public RigidbodyInterpolation Interpolation { get; }
        public bool UseGravity { get; }

        public RigidBodyData(RigidbodyConstraints constraints, RigidbodyInterpolation interpolation, bool useGravity) {
            Constraints = constraints;
            Interpolation = interpolation;
            UseGravity = useGravity;
        }
    }
}

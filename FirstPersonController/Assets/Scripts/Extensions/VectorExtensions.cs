using UnityEngine;

namespace Extensions {
    public static class VectorExtensions {
        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null) {
            Vector3 result = v;
            if (x != null) result.x = x.Value;
            if (y != null) result.y = y.Value;
            if (z != null) result.z = z.Value;

            return result;
        }

        public static Vector3 Add(this Vector3 v, float? x = null, float? y = null, float? z = null) {
            Vector3 result = v;
            if (x != null) result.x += x.Value;
            if (y != null) result.y += y.Value;
            if (z != null) result.z += z.Value;

            return result;
        }
    }
}

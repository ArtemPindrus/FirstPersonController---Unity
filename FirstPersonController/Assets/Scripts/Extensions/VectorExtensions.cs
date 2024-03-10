using UnityEngine;

namespace Extensions {
    public static class VectorExtensions {
        /// <summary>
        /// Returns modified copy of the Vector3 with components changed to given values
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null) {
            Vector3 result = v;
            if (x != null) result.x = x.Value;
            if (y != null) result.y = y.Value;
            if (z != null) result.z = z.Value;

            return result;
        }

        /// <summary>
        /// Returns modified copy of the Vector3 with components + given values
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 Add(this Vector3 v, float x = 0, float y = 0, float z = 0) {
            Vector3 result = v;
            result.x += x;
            result.y += y;
            result.z += z;

            return result;
        }
    }
}

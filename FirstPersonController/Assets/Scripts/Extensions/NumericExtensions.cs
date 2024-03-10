using System;

namespace Extensions {
    public static class NumericExtensions {
        /// <summary>
        /// Just like Math.Sign() but returns 0 if the given value is zero
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float SignZero(this float number) => number switch {
            0 => 0,
            > 0 => 1,
            < 0 => -1,
            _ => throw new ArgumentException("Sign for the given number is not defined!"),
        };
    }
}

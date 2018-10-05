using System;
using System.Collections.Generic;

namespace Hidwizards.IOWrapper.Libraries.PovHelper
{
    public static class PovHelper
    {
        public static List<int> PovDirections = new List<int> { 0, 9000, 18000, 27000 };

        /// <summary>
        /// Decides whether one angle matches another angle, with a tolerance
        /// Basically allows you to get the State (1 or 0) of a virtual "Button" that is a POV direction
        /// </summary>
        /// <param name="a">the first angle, in degrees</param>
        /// <param name="b">the second angle, in degrees</param>
        /// <param name="povTolerance">the tolerance, in degrees</param>
        /// <returns>1 for matches, 0 for does not match</returns>
        public static int ValueFromAngle(int a, int b, int povTolerance = 4500)
        {
            if (a == -1)
                return 0;
            var diff = AngleDiff(a, b);
            return diff <= povTolerance ? 1 : 0;
        }

        /// <summary>
        /// Calculates the difference (with wrap-around) between two angles
        /// </summary>
        /// <param name="a">the first angle, in degrees</param>
        /// <param name="b">the second angle, in degrees</param>
        /// <returns></returns>
        public static int AngleDiff(int a, int b)
        {
            var result1 = a - b;
            if (result1 < 0)
                result1 += 36000;

            var result2 = b - a;
            if (result2 < 0)
                result2 += 36000;

            return Math.Min(result1, result2);
        }

        public static bool ValueMatchesAngle(int value, int angle, int povTolerance = 4500)
        {
            if (value == -1)
                return false;
            var diff = AngleDiff(value, angle);
            return value != -1 && (diff <= povTolerance);
        }

        public static int StateFromAngle(int value, int angle, int povTolerance = 4500)
        {
            return Convert.ToInt32(ValueMatchesAngle(value, angle, povTolerance));
        }
    }

    public class PovState
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}

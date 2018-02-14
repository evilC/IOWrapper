using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IProvider.Helpers
{
    public static class POVHelper
    {
        /// <summary>
        /// Decides whether one angle matches another angle, with a tolerance
        /// Basically allows you to get the State (1 or 0) of a virtual "Button" that is a POV direction
        /// </summary>
        /// <param name="a">the first angle, in degrees</param>
        /// <param name="b">the second angle, in degrees</param>
        /// <param name="povTolerance">the tolerance, in degrees</param>
        /// <returns>1 for matches, 0 for does not match</returns>
        public static int ValueFromAngle(int a, int b, int povTolerance = 90)
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
    }
}

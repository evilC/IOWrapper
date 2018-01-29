using Providers;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX_DirectInput
{
    static class Lookups
    {
        public static int ValueFromAngle(int value, int angle, int povTolerance = 90)
        {
            if (value == -1)
                return 0;
            var diff = AngleDiff(value, angle);
            return diff <= povTolerance ? 1 : 0;
        }

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

        // Maps SharpDX "Offsets" (Input Identifiers) to both iinput type and input index (eg x axis to axis 1)
        public static Dictionary<BindingType, List<JoystickOffset>> directInputMappings = new Dictionary<BindingType, List<JoystickOffset>>(){
                {
                    BindingType.Axis, new List<JoystickOffset>()
                    {
                        JoystickOffset.X,
                        JoystickOffset.Y,
                        JoystickOffset.Z,
                        JoystickOffset.RotationX,
                        JoystickOffset.RotationY,
                        JoystickOffset.RotationZ,
                        JoystickOffset.Sliders0,
                        JoystickOffset.Sliders1
                    }
                },
                {
                    BindingType.Button, new List<JoystickOffset>()
                    {
                        JoystickOffset.Buttons0, JoystickOffset.Buttons1, JoystickOffset.Buttons2, JoystickOffset.Buttons3, JoystickOffset.Buttons4,
                        JoystickOffset.Buttons5, JoystickOffset.Buttons6, JoystickOffset.Buttons7, JoystickOffset.Buttons8, JoystickOffset.Buttons9, JoystickOffset.Buttons10,
                        JoystickOffset.Buttons11, JoystickOffset.Buttons12, JoystickOffset.Buttons13, JoystickOffset.Buttons14, JoystickOffset.Buttons15, JoystickOffset.Buttons16,
                        JoystickOffset.Buttons17, JoystickOffset.Buttons18, JoystickOffset.Buttons19, JoystickOffset.Buttons20, JoystickOffset.Buttons21, JoystickOffset.Buttons22,
                        JoystickOffset.Buttons23, JoystickOffset.Buttons24, JoystickOffset.Buttons25, JoystickOffset.Buttons26, JoystickOffset.Buttons27, JoystickOffset.Buttons28,
                        JoystickOffset.Buttons29, JoystickOffset.Buttons30, JoystickOffset.Buttons31, JoystickOffset.Buttons32, JoystickOffset.Buttons33, JoystickOffset.Buttons34,
                        JoystickOffset.Buttons35, JoystickOffset.Buttons36, JoystickOffset.Buttons37, JoystickOffset.Buttons38, JoystickOffset.Buttons39, JoystickOffset.Buttons40,
                        JoystickOffset.Buttons41, JoystickOffset.Buttons42, JoystickOffset.Buttons43, JoystickOffset.Buttons44, JoystickOffset.Buttons45, JoystickOffset.Buttons46,
                        JoystickOffset.Buttons47, JoystickOffset.Buttons48, JoystickOffset.Buttons49, JoystickOffset.Buttons50, JoystickOffset.Buttons51, JoystickOffset.Buttons52,
                        JoystickOffset.Buttons53, JoystickOffset.Buttons54, JoystickOffset.Buttons55, JoystickOffset.Buttons56, JoystickOffset.Buttons57, JoystickOffset.Buttons58,
                        JoystickOffset.Buttons59, JoystickOffset.Buttons60, JoystickOffset.Buttons61, JoystickOffset.Buttons62, JoystickOffset.Buttons63, JoystickOffset.Buttons64,
                        JoystickOffset.Buttons65, JoystickOffset.Buttons66, JoystickOffset.Buttons67, JoystickOffset.Buttons68, JoystickOffset.Buttons69, JoystickOffset.Buttons70,
                        JoystickOffset.Buttons71, JoystickOffset.Buttons72, JoystickOffset.Buttons73, JoystickOffset.Buttons74, JoystickOffset.Buttons75, JoystickOffset.Buttons76,
                        JoystickOffset.Buttons77, JoystickOffset.Buttons78, JoystickOffset.Buttons79, JoystickOffset.Buttons80, JoystickOffset.Buttons81, JoystickOffset.Buttons82,
                        JoystickOffset.Buttons83, JoystickOffset.Buttons84, JoystickOffset.Buttons85, JoystickOffset.Buttons86, JoystickOffset.Buttons87, JoystickOffset.Buttons88,
                        JoystickOffset.Buttons89, JoystickOffset.Buttons90, JoystickOffset.Buttons91, JoystickOffset.Buttons92, JoystickOffset.Buttons93, JoystickOffset.Buttons94,
                        JoystickOffset.Buttons95, JoystickOffset.Buttons96, JoystickOffset.Buttons97, JoystickOffset.Buttons98, JoystickOffset.Buttons99, JoystickOffset.Buttons100,
                        JoystickOffset.Buttons101, JoystickOffset.Buttons102, JoystickOffset.Buttons103, JoystickOffset.Buttons104, JoystickOffset.Buttons105, JoystickOffset.Buttons106,
                        JoystickOffset.Buttons107, JoystickOffset.Buttons108, JoystickOffset.Buttons109, JoystickOffset.Buttons110, JoystickOffset.Buttons111, JoystickOffset.Buttons112,
                        JoystickOffset.Buttons113, JoystickOffset.Buttons114, JoystickOffset.Buttons115, JoystickOffset.Buttons116, JoystickOffset.Buttons117, JoystickOffset.Buttons118,
                        JoystickOffset.Buttons119, JoystickOffset.Buttons120, JoystickOffset.Buttons121, JoystickOffset.Buttons122, JoystickOffset.Buttons123, JoystickOffset.Buttons124,
                        JoystickOffset.Buttons125, JoystickOffset.Buttons126, JoystickOffset.Buttons127
                    }
                },
                {
                    //BindingType.POV, new List<JoystickOffset>()
                    //{
                    //    JoystickOffset.PointOfViewControllers0,
                    //    JoystickOffset.PointOfViewControllers1,
                    //    JoystickOffset.PointOfViewControllers2,
                    //    JoystickOffset.PointOfViewControllers3
                    //}
                    BindingType.POV, new List<JoystickOffset>()
                    {
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3
                    }
                }
            };

        public static List<string> povDirections = new List<string>() { "Up", "Right", "Down", "Left" };

    }
}

using HidWizards.IOWrapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.DirectInput;

namespace TestApp.Library
{
    /// <summary>
    /// A lookup table of BindingDescriptors
    /// 
    /// Describes various bindings (Buttons, Axes etc)
    /// </summary>
    class Bindings
    {

        /// <summary>
        /// Generic Bindings that may work with various Providers
        /// </summary>
        public static class Generic
        #region Generic
        {
            public static BindingDescriptor Button1 = new BindingDescriptor { Index = 0, Type = BindingType.Button };
            public static BindingDescriptor Button2 = new BindingDescriptor { Index = 1, Type = BindingType.Button };
            public static BindingDescriptor Button3 = new BindingDescriptor { Index = 2, Type = BindingType.Button };
            public static BindingDescriptor Button4 = new BindingDescriptor { Index = 3, Type = BindingType.Button };
            public static BindingDescriptor Button5 = new BindingDescriptor { Index = 4, Type = BindingType.Button };
            public static BindingDescriptor Button6 = new BindingDescriptor { Index = 5, Type = BindingType.Button };
            public static BindingDescriptor Button7 = new BindingDescriptor { Index = 6, Type = BindingType.Button };
            public static BindingDescriptor Button8 = new BindingDescriptor { Index = 7, Type = BindingType.Button };
            public static BindingDescriptor Button9 = new BindingDescriptor { Index = 8, Type = BindingType.Button };
            public static BindingDescriptor Button10 = new BindingDescriptor { Index = 9, Type = BindingType.Button };
            public static BindingDescriptor Axis1 = new BindingDescriptor { Index = 0, Type = BindingType.Axis };
            public static BindingDescriptor Axis2 = new BindingDescriptor { Index = 1, Type = BindingType.Axis };
            public static BindingDescriptor Axis3 = new BindingDescriptor { Index = 2, Type = BindingType.Axis };
            public static BindingDescriptor Axis4 = new BindingDescriptor { Index = 3, Type = BindingType.Axis };
            public static BindingDescriptor Axis5 = new BindingDescriptor { Index = 4, Type = BindingType.Axis };
            public static BindingDescriptor Axis6 = new BindingDescriptor { Index = 5, Type = BindingType.Axis };
            public static BindingDescriptor AxisX = Axis1;
            public static BindingDescriptor AxisY = Axis2;
            public static BindingDescriptor POV1Up = new BindingDescriptor { Index = 0, SubIndex = 0, Type = BindingType.POV };
            public static BindingDescriptor POV1Right = new BindingDescriptor { Index = 0, SubIndex = 1, Type = BindingType.POV };
            public static BindingDescriptor POV1Down = new BindingDescriptor { Index = 0, SubIndex = 2, Type = BindingType.POV };
            public static BindingDescriptor POV1Left = new BindingDescriptor { Index = 0, SubIndex = 3, Type = BindingType.POV };
            public static BindingDescriptor POV2Up = new BindingDescriptor { Index = 1, SubIndex = 0, Type = BindingType.POV };
            public static BindingDescriptor POV2Right = new BindingDescriptor { Index = 1, SubIndex = 1, Type = BindingType.POV };
            public static BindingDescriptor POV2Down = new BindingDescriptor { Index = 1, SubIndex = 2, Type = BindingType.POV };
            public static BindingDescriptor POV2Left = new BindingDescriptor { Index = 1, SubIndex = 3, Type = BindingType.POV };
            public static BindingDescriptor DpadUp = new BindingDescriptor { Index = 0, SubIndex = 0, Type = BindingType.POV };
            public static BindingDescriptor DpadRight = new BindingDescriptor { Index = 1, SubIndex = 0, Type = BindingType.POV };
            public static BindingDescriptor Ds4Gyro = new BindingDescriptor { Index = 0, SubIndex = 2, Type = BindingType.Axis }; // DS4 Gyro
        }
        #endregion

        #region DI

        public static class DirectInput
        {
            public static BindingDescriptor Button1 = new BindingDescriptor { Index = (int) JoystickOffset.Buttons0, Type = BindingType.Button };
            public static BindingDescriptor Button2 = new BindingDescriptor { Index = (int) JoystickOffset.Buttons1, Type = BindingType.Button };
            public static BindingDescriptor Button3 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons2, Type = BindingType.Button };
            public static BindingDescriptor Button4 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons3, Type = BindingType.Button };
            public static BindingDescriptor Button5 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons4, Type = BindingType.Button };
            public static BindingDescriptor Button6 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons5, Type = BindingType.Button };
            public static BindingDescriptor Button7 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons6, Type = BindingType.Button };
            public static BindingDescriptor Button8 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons7, Type = BindingType.Button };
            public static BindingDescriptor Button9 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons8, Type = BindingType.Button };
            public static BindingDescriptor Button10 = new BindingDescriptor { Index = (int)JoystickOffset.Buttons9, Type = BindingType.Button };
            public static BindingDescriptor Axis1 = new BindingDescriptor { Index = (int)JoystickOffset.X, Type = BindingType.Axis };
            public static BindingDescriptor Axis2 = new BindingDescriptor { Index = (int)JoystickOffset.Y, Type = BindingType.Axis };
            public static BindingDescriptor AxisX = Axis1;
            public static BindingDescriptor AxisY = Axis2;
        }
        #endregion

        /// <summary>
        /// Interception Provider bindings
        /// </summary>
        public static class Interception
        #region Interception
        {
            public static class Keyboard
            {
                public static BindingDescriptor Esc = Generic.Button1;
                public static BindingDescriptor One = Generic.Button2;
                public static BindingDescriptor Two = Generic.Button3;
                public static BindingDescriptor Alt = new BindingDescriptor {Index = 55, SubIndex = 0, Type = BindingType.Button};
                public static BindingDescriptor RightAlt = new BindingDescriptor {Index = 311, SubIndex = 0, Type = BindingType.Button};
                public static BindingDescriptor RightShift = new BindingDescriptor {Index = 53, SubIndex = 0, Type = BindingType.Button};
                public static BindingDescriptor Shift = new BindingDescriptor {Index = 41, SubIndex = 0, Type = BindingType.Button};
                public static BindingDescriptor Delete = new BindingDescriptor {Index = 338, SubIndex = 0, Type = BindingType.Button};
                public static BindingDescriptor NumDelete = new BindingDescriptor {Index = 82, SubIndex = 0, Type = BindingType.Button};
                public static BindingDescriptor Up = new BindingDescriptor {Index = 327, SubIndex = 0, Type = BindingType.Button};
                public static BindingDescriptor NumUp = new BindingDescriptor {Index = 71, SubIndex = 0, Type = BindingType.Button};
            }

            public static class MouseButton
            {
                public static BindingDescriptor LButton = Generic.Button1;
                public static BindingDescriptor RButton = Generic.Button2;
                public static BindingDescriptor MButton = Generic.Button3;
                public static BindingDescriptor XButton1 = Generic.Button4;
                public static BindingDescriptor XButton2 = Generic.Button5;
                public static BindingDescriptor WheelUp = Generic.Button6;
                public static BindingDescriptor WheelDown = Generic.Button7;
                public static BindingDescriptor WheelLeft = Generic.Button8;
                public static BindingDescriptor WheelRight = Generic.Button9;
            }

            public static class MouseAxis
            {
                public static BindingDescriptor X = Generic.Axis1;
                public static BindingDescriptor Y = Generic.Axis2;
            }
        }
        #endregion


        #region ViGEm
        public static class ViGEm
        {
            #region Xbox
            public static class Xbox
            {
                public static class Buttons
                {
                    public static BindingDescriptor A = Generic.Button1;
                    public static BindingDescriptor B = Generic.Button2;
                    public static BindingDescriptor X = Generic.Button3;
                    public static BindingDescriptor Y = Generic.Button4;
                }

                public static class Axes
                {
                    public static BindingDescriptor LSX = Generic.Axis1;
                    public static BindingDescriptor LSY = Generic.Axis2;
                    public static BindingDescriptor RSX = Generic.Axis3;
                    public static BindingDescriptor RSY = Generic.Axis4;
                    public static BindingDescriptor LT = Generic.Axis5;
                    public static BindingDescriptor RT = Generic.Axis6;
                }
            }
            #endregion

            #region DS4
            public static class DS4
            {
                public static class Buttons
                {
                    public static BindingDescriptor Cross = Generic.Button1;
                    public static BindingDescriptor Circle = Generic.Button2;
                }
                #endregion
            }
        }
        #endregion
    }
}

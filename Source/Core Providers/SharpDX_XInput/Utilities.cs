using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    public static class Utilities
    {
        public static Dictionary<BindingType, List<GamepadButtonFlags>> xinputButtonIdentifiers = new Dictionary<BindingType, List<GamepadButtonFlags>>
        {
            { BindingType.Button, new List<GamepadButtonFlags>
            {
                GamepadButtonFlags.A, GamepadButtonFlags.B, GamepadButtonFlags.X, GamepadButtonFlags.Y
                , GamepadButtonFlags.LeftShoulder, GamepadButtonFlags.RightShoulder
                , GamepadButtonFlags.LeftThumb, GamepadButtonFlags.RightThumb
                , GamepadButtonFlags.Back, GamepadButtonFlags.Start
            }},
            {
                BindingType.POV, new List<GamepadButtonFlags>
                {
                    GamepadButtonFlags.DPadUp, GamepadButtonFlags.DPadRight, GamepadButtonFlags.DPadDown, GamepadButtonFlags.DPadLeft
                }
            }
        };

        public static List<string> buttonNames = new List<string> { "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start" };
        public static List<string> axisNames = new List<string> { "LX", "LY", "RX", "RY", "LT", "RT" };
        public static List<string> povNames = new List<string> { "Up", "Right", "Down", "Left" };

        public static BindingType GetButtonPovBindingTypeFromFlag(GamepadButtonFlags flag)
        {
            return flag < GamepadButtonFlags.DPadUp || flag > GamepadButtonFlags.DPadRight ? BindingType.Button : BindingType.POV;
        }

        public static BindingType GetButtonPovBindingTypeFromIndex(int index)
        {
            return index < 10 ? BindingType.Button : BindingType.POV;
        }
    }
}

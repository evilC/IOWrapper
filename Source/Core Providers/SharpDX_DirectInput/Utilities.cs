using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    public static class Utilities
    {
        public static BindingType OffsetToType(JoystickOffset offset)
        {
            int index = (int)offset;
            if (index <= (int)JoystickOffset.Sliders1) return BindingType.Axis;
            if (index <= (int)JoystickOffset.PointOfViewControllers3) return BindingType.POV;
            return BindingType.Button;
        }

        public static string JoystickToHandle(Joystick joystick)
        {
            return $"VID_{joystick.Properties.VendorId:X4}&PID_{joystick.Properties.ProductId:X4}";
        }

        public static bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == DeviceType.Joystick
                   || deviceInstance.Type == DeviceType.Gamepad
                   || deviceInstance.Type == DeviceType.FirstPerson
                   || deviceInstance.Type == DeviceType.Flight
                   || deviceInstance.Type == DeviceType.Driving
                   || deviceInstance.Type == DeviceType.Supplemental;
        }

    }
}

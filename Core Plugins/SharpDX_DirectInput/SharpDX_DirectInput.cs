using SharpDX.DirectInput;
using System.ComponentModel.Composition;
using PluginContracts;
using System;
using System.Collections.Generic;

namespace SharpDX_DirectInput
{
    [Export(typeof(IPlugin))]
    public class SharpDX_DirectInput : IPlugin
    {
        static private DirectInput directInput;
        private dynamic tmpCallback;

        public SharpDX_DirectInput()
        {
            directInput = new DirectInput();
        }

        #region IPlugin Members

        public string PluginName { get { return typeof(SharpDX_DirectInput).Namespace; } }

        public DeviceReport GetInputList()
        {
            var dr = new DeviceReport();

            var devices = directInput.GetDevices();
            foreach (var deviceInstance in devices)
            {
                if (!IsStickType(deviceInstance))
                    continue;
                var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);
                joystick.Acquire();
                dr.Devices.Add(new IOWrapperDevice()
                {
                    DeviceHandle = deviceInstance.InstanceGuid.ToString(),
                    DeviceName = deviceInstance.ProductName,
                    PluginName = PluginName,
                    API = "DirectInput",
                    ButtonCount = (uint)joystick.Capabilities.ButtonCount
                });
                joystick.Unacquire();
            }
            return dr;
        }

        public bool SubscribeButton(string deviceHandle, uint buttonId, dynamic callback)
        {
            return false;
        }
        #endregion

        #region Helper Methods
        private bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == SharpDX.DirectInput.DeviceType.Joystick
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Gamepad
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.FirstPerson
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Flight
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Driving
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Supplemental;
        }
        #endregion
    }
}
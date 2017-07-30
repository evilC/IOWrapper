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

        public SharpDX_DirectInput()
        {
            directInput = new DirectInput();
        }

        #region IPlugin Members

        public string PluginName { get { return typeof(SharpDX_DirectInput).Namespace; } }

        public DeviceReport GetInputList()
        {
            var dr = new DeviceReport();
            dr.Devices.Add(new IOWrapperDevice()
            {
                DeviceHandle = "VID1234/PIDBEAD/0",
                PluginName = PluginName,
                API = "DirectInput",
                ButtonCount = 128
            });
            dr.Devices.Add(new IOWrapperDevice()
            {
                DeviceHandle = "VID1234/PIDBEAD/1",
                PluginName = PluginName,
                API = "DirectInput",
                ButtonCount = 32
            });
            return dr;
        }
        #endregion
    }
}
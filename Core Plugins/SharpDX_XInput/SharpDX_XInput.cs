using System;
using System.ComponentModel.Composition;
using PluginContracts;
using System.Collections.Generic;

namespace SharpDX_XInput
{
    [Export(typeof(IPlugin))]
    public class SharpDX_XInput : IPlugin
    {
        #region IPlugin Members

        public string PluginName { get { return typeof(SharpDX_XInput).Namespace; } }

        public DeviceReport GetInputList()
        {
            var dr = new DeviceReport();
            dr.Devices.Add(new IOWrapperDevice()
            {
                DeviceHandle = "0",
                PluginName = PluginName,
                API = "XInput",
                ButtonCount = 11,
                ButtonNames = new List<string>() { "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start", "Xbox" }
            });
            return dr;
        }

        public bool SubscribeButton(string deviceHandle, uint buttonId, dynamic callback)
        {
            return false;
        }
        #endregion
    }
}

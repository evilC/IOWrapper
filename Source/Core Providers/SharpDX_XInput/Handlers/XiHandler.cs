using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_XInput.Handlers
{
    public class XiHandler : ApiHandler
    {
        private readonly List<XiDeviceHandler> _bindModeHandlers = new List<XiDeviceHandler>();

        public XiHandler(ProviderDescriptor providerDescriptor) : base(providerDescriptor)
        {
        }

        public override void SetDetectionMode(DetectionMode mode, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            switch (mode)
            {
                case DetectionMode.Bind:
                    _bindModeCallback = callback ?? throw new Exception("Bind Mode requested but no callback passed");
                    // Enter Bind Mode
                    // Find a list of all connected devices, and for each...
                    // ... Check if it already has a DeviceHandler, and if so, swap it to Bind Mode
                    // Else new up a DeviceHandler and set it to Bind Mode
                    var deviceHandle = "xb360";
                    for (int i = 0; i < 4; i++)
                    {
                        if (BindingDictionary.ContainsKey(deviceHandle) && BindingDictionary[deviceHandle].ContainsKey(i))
                        {
                            BindingDictionary[deviceHandle][i].SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        }
                        else
                        {
                            var deviceDescriptor = new DeviceDescriptor { DeviceHandle = deviceHandle, DeviceInstance = i };
                            var deviceHandler = new XiDeviceHandler(deviceDescriptor);
                            deviceHandler.SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                            _bindModeHandlers.Add(deviceHandler);
                        }
                    }
                    break;
                case DetectionMode.Subscription:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override DeviceHandler CreateDeviceHandler(DeviceDescriptor deviceDescriptor)
        {
            return new XiDeviceHandler(deviceDescriptor);
        }
    }
}

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
        private readonly Dictionary<int, XiDeviceHandler> _tempBindModeDevices = new Dictionary<int, XiDeviceHandler>();

        public XiHandler(ProviderDescriptor providerDescriptor) : base(providerDescriptor)
        {
        }

        public override void SetDetectionMode(DetectionMode mode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            const string deviceHandle = "xb360";
            var i = deviceDescriptor.DeviceInstance;
            switch (mode)
            {
                case DetectionMode.Bind:
                    _bindModeCallback = callback ?? throw new Exception("Bind Mode requested but no callback passed");

                    if (SubscribedDevices.ContainsKey(deviceDescriptor))
                    {
                        SubscribedDevices[deviceDescriptor].SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                    }
                    else
                    {
                        var deviceHandler = new XiDeviceHandler(new DeviceDescriptor { DeviceHandle = deviceHandle, DeviceInstance = i });
                        deviceHandler.SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        _tempBindModeDevices.Add(i, deviceHandler);
                    }
                    break;
                case DetectionMode.Subscription:
                    if (SubscribedDevices.ContainsKey(deviceDescriptor))
                    {
                        SubscribedDevices[deviceDescriptor].SetDetectionMode(DetectionMode.Subscription, BindModeCallback);
                    }
                    else
                    {
                        if (_tempBindModeDevices.ContainsKey(i))
                        {
                            _tempBindModeDevices[i].Dispose();
                            _tempBindModeDevices.Remove(i);
                        }
                    }
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX_DirectInput.Wrappers;

namespace SharpDX_DirectInput.Handlers
{
    /// <inheritdoc />
    /// <summary>
    /// Handles input detection for this provider
    /// ToDo: Handle device unplug / replug better
    /// Will probably need some exterior mechanism (USB HID?) to detect plug / unplug
    /// Mechanism probably belongs in the core, but this provider would need to support it
    /// As an interim measure, we could probably have a "Refresh" button in the UI
    /// </summary>
    internal class DiHandler : ApiHandler
    {
        private readonly Dictionary<DeviceDescriptor ,DiDeviceHandler> _tempBindModeDevices = new Dictionary<DeviceDescriptor, DiDeviceHandler>();

        public DiHandler(ProviderDescriptor providerDescriptor) : base(providerDescriptor)
        {
        }

        public override void SetDetectionMode(DetectionMode mode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            SubscribedDevices.TryGetValue(deviceDescriptor, out var subscribedDevice);

            DeviceHandler tempBindModeDevice = null;
            if (_tempBindModeDevices.ContainsKey(deviceDescriptor))
            {
                tempBindModeDevice = _tempBindModeDevices[deviceDescriptor];
            }

            if (mode == DetectionMode.Subscription)
            {
                if (subscribedDevice == null)
                {
                    if (tempBindModeDevice != null)
                    {
                        tempBindModeDevice.Dispose();
                        _tempBindModeDevices.Remove(deviceDescriptor);
                    }
                }
                else
                {
                    subscribedDevice.SetDetectionMode(DetectionMode.Subscription);
                }
            }
            else
            {
                _bindModeCallback = callback ?? throw new Exception("Bind Mode requested but no callback passed");

                // Enter Bind Mode
                if (subscribedDevice == null)
                {
                    if (tempBindModeDevice == null)
                    {
                        var deviceHandler = new DiDeviceHandler(deviceDescriptor);
                        deviceHandler.SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        _tempBindModeDevices.Add(deviceDescriptor, deviceHandler);
                    }
                }
                else
                {
                    subscribedDevice.SetDetectionMode(DetectionMode.Bind);
                }
            }
        }

        public override DeviceHandler CreateDeviceHandler(DeviceDescriptor deviceDescriptor)
        {
            return new DiDeviceHandler(deviceDescriptor);
        }
    }
}

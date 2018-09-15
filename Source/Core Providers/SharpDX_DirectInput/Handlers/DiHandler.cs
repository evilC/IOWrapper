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

                /*
                foreach (var connectedDeviceList in DiWrapper.Instance.ConnectedDevices)
                {
                    var connectedHandle = connectedDeviceList.Key;
                    // Iterate through each instance of the device handle
                    for (var i = 0; i < connectedDeviceList.Value.Count; i++)
                    {
                        if (SubscribedDevices.ContainsKey(connectedHandle) &&
                            SubscribedDevices[connectedHandle].ContainsKey(i))
                        {
                            // This device has bindings, so set the existing handler to Subscription Mode
                            SubscribedDevices[connectedHandle][i].SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        }
                    }
                }

                // Shut down all the temporary Bind Mode handlers
                foreach (var bindModeHandler in _tempBindModeDevices)
                {
                    bindModeHandler.Dispose();
                }
                _tempBindModeDevices.Clear();
                */
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

                /*
                _bindModeCallback = callback;
                // Enter Bind Mode
                // Iterate the list of all connected devices, and for each...
                // ... Check if it already has a DeviceHandler, and if so, swap it to Bind Mode
                // Else new up a DeviceHandler and set it to Bind Mode
                foreach (var connectedDeviceList in DiWrapper.Instance.ConnectedDevices)
                {
                    var connectedHandle = connectedDeviceList.Key;
                    // Iterate through each instance of the device handle
                    for (var i = 0; i < connectedDeviceList.Value.Count; i++)
                    {
                        if (SubscribedDevices.ContainsKey(connectedHandle) &&
                            SubscribedDevices[connectedHandle].ContainsKey(i))
                        {
                            // This device has bindings, so set the existing handler to Bind Mode
                            SubscribedDevices[connectedHandle][i].SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        }
                        else
                        {
                            // This device has no bindings, so new up a handler and set it to bind mode
                            //ToDo: This code does not try to use the same DeviceInstance order as is reported in the Descriptors. It is just placeholder code
                            var deviceHandler = new DiDeviceHandler(new DeviceDescriptor { DeviceHandle = connectedHandle, DeviceInstance = i });
                            deviceHandler.SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                            _tempBindModeDevices.Add(deviceHandler);
                        }
                    }
                }
                */
            }
        }

        public override DeviceHandler CreateDeviceHandler(DeviceDescriptor deviceDescriptor)
        {
            return new DiDeviceHandler(deviceDescriptor);
        }
    }
}

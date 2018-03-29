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
        private readonly List<DiDeviceHandler> _bindModeHandlers = new List<DiDeviceHandler>();

        public DiHandler(ProviderDescriptor providerDescriptor) : base(providerDescriptor)
        {
        }

        public override void SetDetectionMode(DetectionMode mode, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            if (mode == DetectionMode.Subscription)
            {
                foreach (var connectedDeviceList in DiWrapper.Instance.ConnectedDevices)
                {
                    var connectedHandle = connectedDeviceList.Key;
                    // Iterate through each instance of the device handle
                    for (var i = 0; i < connectedDeviceList.Value.Count; i++)
                    {
                        if (BindingDictionary.ContainsKey(connectedHandle) &&
                            BindingDictionary[connectedHandle].ContainsKey(i))
                        {
                            // This device has bindings, so set the existing handler to Subscription Mode
                            BindingDictionary[connectedHandle][i].SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        }
                    }
                }

                // Shut down all the temporary Bind Mode handlers
                foreach (var bindModeHandler in _bindModeHandlers)
                {
                    bindModeHandler.Dispose();
                }
                _bindModeHandlers.Clear();
            }
            else
            {
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
                        if (BindingDictionary.ContainsKey(connectedHandle) &&
                            BindingDictionary[connectedHandle].ContainsKey(i))
                        {
                            // This device has bindings, so set the existing handler to Bind Mode
                            BindingDictionary[connectedHandle][i].SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        }
                        else
                        {
                            // This device has no bindings, so new up a handler and set it to bind mode
                            //ToDo: This code does not try to use the same DeviceInstance order as is reported in the Descriptors. It is just placeholder code
                            var deviceDescriptor = new DeviceDescriptor { DeviceHandle = connectedHandle, DeviceInstance = i };
                            var deviceHandler = new DiDeviceHandler(deviceDescriptor);
                            deviceHandler.SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                            _bindModeHandlers.Add(deviceHandler);
                        }
                    }
                }
            }
        }

        public override DeviceHandler CreateDeviceHandler(DeviceDescriptor deviceDescriptor)
        {
            return new DiDeviceHandler(deviceDescriptor);
        }
    }
}

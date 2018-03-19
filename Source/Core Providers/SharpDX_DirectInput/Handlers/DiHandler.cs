using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX_DirectInput.Helpers;

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
        public static DirectInput DiInstance { get; } = new DirectInput();
        private readonly List<DiDeviceHandler> _bindModeHandlers = new List<DiDeviceHandler>();

        public DiHandler(ProviderDescriptor providerDescriptor) : base(providerDescriptor)
        {
        }

        public override void SetDetectionMode(DetectionMode mode, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            if (mode == DetectionMode.Subscription)
            {
                throw new NotImplementedException();
            }
            else
            {
                _bindModeCallback = callback;
                // Enter Bind Mode
                // Find a list of all connected devices, and for each...
                // ... Check if it already has a DeviceHandler, and if so, swap it to Bind Mode
                // Else new up a DeviceHandler and set it to Bind Mode
                var connectedHandles = Lookups.GetConnectedHandles();

                foreach (var connectedHandle in connectedHandles)
                {
                    var guids = Lookups.GetDeviceOrders(connectedHandle);
                    for (var i = 0; i < guids.Count; i++)
                    {
                        if (!DiInstance.IsDeviceAttached(guids[i]))
                        {
                            continue;
                        }

                        if (BindingDictionary.ContainsKey(connectedHandle) &&
                            BindingDictionary[connectedHandle].ContainsKey(i))
                        {
                            BindingDictionary[connectedHandle][i].SetDetectionMode(DetectionMode.Bind, BindModeCallback);
                        }
                        else
                        {
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

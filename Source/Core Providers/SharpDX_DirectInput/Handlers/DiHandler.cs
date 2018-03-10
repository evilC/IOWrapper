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
        private readonly List<DiDevicePoller> _devicePollers = new List<DiDevicePoller>();

        public override DeviceHandler CreateDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new DiDeviceHandler(subReq);
        }

        public void SetBindModeState(bool state)
        {
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
                    var deviceDescriptor = new DeviceDescriptor {DeviceHandle = connectedHandle, DeviceInstance = i};
                    _devicePollers.Add(new DiDevicePoller(deviceDescriptor, ProcessPollResult));
                }
            }
        }

        public void ProcessPollResult(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor, int state)
        {
            Console.WriteLine($"IOWrapper| Activity seen from handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}" +
                              $", Type: {bindingDescriptor.Type}, Index: {bindingDescriptor.Index}, State: {state}");
        }
    }
}

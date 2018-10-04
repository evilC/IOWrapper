using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    public class DiDeviceLibrary : IDeviceLibrary<Guid>
    {
        private ConcurrentDictionary<string, List<Guid>> ConnectedDevices = new ConcurrentDictionary<string, List<Guid>>();
        public static DirectInput DiInstance = new DirectInput();

        public DiDeviceLibrary()
        {
            RefreshConnectedDevices();
        }

        public DeviceDescriptor GetDeviceDescriptor(Guid deviceGuid)
        {
            throw new NotImplementedException();
        }

        public Guid GetDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (ConnectedDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var instances) &&
                instances.Count >= deviceDescriptor.DeviceInstance)
            {
                return instances[deviceDescriptor.DeviceInstance];
            }

            return Guid.Empty;
        }

        private void RefreshConnectedDevices()
        {
            ConnectedDevices = new ConcurrentDictionary<string, List<Guid>>();
            var diDeviceInstances = DiInstance.GetDevices();
            foreach (var device in diDeviceInstances)
            {
                if (!Utilities.IsStickType(device))
                    continue;
                var joystick = new Joystick(DiInstance, device.InstanceGuid);
                var handle = Utilities.JoystickToHandle(joystick);
                if (!ConnectedDevices.ContainsKey(handle))
                {
                    ConnectedDevices[handle] = new List<Guid>();
                }
                ConnectedDevices[handle].Add(device.InstanceGuid);
            }
        }

        private Guid DeviceDescriptorToInstanceGuid(DeviceDescriptor deviceDescriptor)
        {
            if (ConnectedDevices.ContainsKey(deviceDescriptor.DeviceHandle)
                && ConnectedDevices[deviceDescriptor.DeviceHandle].Count >= deviceDescriptor.DeviceInstance)
            {
                return ConnectedDevices[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
            //return Guid.Empty;
        }


    }
}

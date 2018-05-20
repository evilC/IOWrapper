using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput.Wrappers
{
    public class DiWrapper
    {
        public ConcurrentDictionary<string, List<Guid>> ConnectedDevices { get; private set; } = new ConcurrentDictionary<string, List<Guid>>();

        #region Singleton setup
        private static DiWrapper _instance;
        public static DiWrapper Instance => _instance ?? (_instance = new DiWrapper());
        public static DirectInput DiInstance { get; } = new DirectInput();
        #endregion

        public DiWrapper()
        {
            RefreshConnectedDevices();
        }

        private void RefreshConnectedDevices()
        {
            ConnectedDevices = new ConcurrentDictionary<string, List<Guid>>();
            var diDeviceInstances = DiInstance.GetDevices();
            foreach (var device in diDeviceInstances)
            {
                if (!Lookups.IsStickType(device))
                    continue;
                var joystick = new Joystick(DiInstance, device.InstanceGuid);
                var handle = Lookups.JoystickToHandle(joystick);
                if (!ConnectedDevices.ContainsKey(handle))
                {
                    ConnectedDevices[handle] = new List<Guid>();
                }
                ConnectedDevices[handle].Add(device.InstanceGuid);
            }
        }

        public Guid DeviceDescriptorToInstanceGuid(DeviceDescriptor deviceDescriptor)
        {
            if (ConnectedDevices.ContainsKey(deviceDescriptor.DeviceHandle)
                && ConnectedDevices[deviceDescriptor.DeviceHandle].Count >= deviceDescriptor.DeviceInstance)
            {
                return ConnectedDevices[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
            //return Guid.Empty;
        }

        //public DeviceDescriptor DeviceGuidToDeviceDescriptor(Guid deviceGuid)
        //{
        //    foreach (var connectedDevice in ConnectedDevices)
        //    {
        //        if (connectedDevice.Value.Contains(deviceGuid))
        //        {
        //            return new DeviceDescriptor {DeviceHandle = connectedDevice.Key, DeviceInstance = connectedDevice.Value.IndexOf(deviceGuid)};
        //        }
        //    }
        //    throw new Exception($"Could not find device GUID {deviceGuid} in Connected Devices list");
        //}
    }
}

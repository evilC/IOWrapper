using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_SpaceMouse.DeviceLibrary
{
    public class SmDeviceLibrary : IInputDeviceLibrary<string>
    {
        private readonly ProviderDescriptor _providerDescriptor;
        private ConcurrentDictionary<string, List<string>> _connectedDevices = new ConcurrentDictionary<string, List<string>>();

        public SmDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            RefreshConnectedDevices();
        }

        public void RefreshConnectedDevices()
        {
            _connectedDevices = new ConcurrentDictionary<string, List<string>>();

            var devices = HidDevices.Enumerate(0x046d, 0xc62b);
            var handle = "VID_046D&PID_C62B";
            foreach (var device in devices)
            {
                if (!_connectedDevices.ContainsKey(handle))
                {
                    _connectedDevices.TryAdd(handle, new List<string>());
                }
                _connectedDevices[handle].Add(device.DevicePath);
            }
        }

        public string GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_connectedDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var device)
                && device.Count >= deviceDescriptor.DeviceInstance)
            {
                return device[deviceDescriptor.DeviceInstance];
            }

            return string.Empty;
        }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return null;
        }

        public BindingReport GetInputBindingReport(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            return null;
        }
    }
}

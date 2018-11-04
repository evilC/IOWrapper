using System;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Midi.DeviceLibraries
{
    public partial class MidiDeviceLibrary : IInputOutputDeviceLibrary<int>
    {
        public int GetOutputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_connectedOutputDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var instances) &&
                instances.Count >= deviceDescriptor.DeviceInstance)
            {
                return instances[deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find output device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
        }

        public ProviderReport GetOutputList()
        {
            return _outputProviderReport;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            throw new NotImplementedException();
        }

    }
}

using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceLibrary
{
    public interface IDeviceLibrary<out TDeviceIdentifier>
    {
        /// <summary>
        /// Translates a <see cref="DeviceDescriptor"/> into a native handle to the device
        /// </summary>
        /// <param name="deviceDescriptor">A DeviceDescriptor describing the device</param>
        /// <returns>A native handle to the device</returns>
        TDeviceIdentifier GetDeviceIdentifier(DeviceDescriptor deviceDescriptor);


        void RefreshConnectedDevices();
    }

    public interface IInputDeviceLibrary<out TDeviceIdentifier> : IDeviceLibrary<TDeviceIdentifier>
    {
        ProviderReport GetInputList();
        DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor);
    }
}

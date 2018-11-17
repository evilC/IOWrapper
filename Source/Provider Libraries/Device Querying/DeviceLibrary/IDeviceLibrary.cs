using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceLibrary
{
    public interface IDeviceLibrary<out TDeviceIdentifier>
    {
        void RefreshConnectedDevices();
    }

    public interface IInputDeviceLibrary<out TDeviceIdentifier> : IDeviceLibrary<TDeviceIdentifier>
    {
        /// <summary>
        /// Translates a <see cref="DeviceDescriptor"/> into a native handle to the device
        /// </summary>
        /// <param name="deviceDescriptor">A DeviceDescriptor describing the device</param>
        /// <returns>A native handle to the device</returns>
        TDeviceIdentifier GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor);

        ProviderReport GetInputList();
        DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor);
        BindingReport GetInputBindingReport(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor);
    }

    public interface IOutputDeviceLibrary<out TDeviceIdentifier> : IDeviceLibrary<TDeviceIdentifier>
    {
        /// <summary>
        /// Translates a <see cref="DeviceDescriptor"/> into a native handle to the device
        /// </summary>
        /// <param name="deviceDescriptor">A DeviceDescriptor describing the device</param>
        /// <returns>A native handle to the device</returns>
        TDeviceIdentifier GetOutputDeviceIdentifier(DeviceDescriptor deviceDescriptor);

        ProviderReport GetOutputList();
        DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor);
    }

    public interface IInputOutputDeviceLibrary<out T> : IInputDeviceLibrary<T>, IOutputDeviceLibrary<T>
    {

    }
}

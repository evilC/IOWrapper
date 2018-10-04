using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Devices
{
    public interface IDeviceLibrary<TDeviceIdentifier>
    {
        TDeviceIdentifier GetDeviceIdentifier(DeviceDescriptor deviceDescriptor);
    }

    public interface IInputDeviceLibrary<TDeviceIdentifier> : IDeviceLibrary<TDeviceIdentifier>
    {
        ProviderReport GetInputList();
        DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor);
    }
}

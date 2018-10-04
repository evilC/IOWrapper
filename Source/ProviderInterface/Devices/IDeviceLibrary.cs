using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Devices
{
    public interface IDeviceLibrary<TDeviceIdentifier>
    {
        DeviceDescriptor GetDeviceDescriptor(TDeviceIdentifier device);
        TDeviceIdentifier GetDeviceIdentifier(DeviceDescriptor deviceDescriptor);
    }

    public interface IInputDeviceLibrary<TDeviceIdentifier> : IDeviceLibrary<TDeviceIdentifier>
    {
        ProviderReport GetInputList();
        DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq);
    }
}

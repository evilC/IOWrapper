using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Devices
{
    public interface IDeviceManager<TDevice>
    {
        DeviceDescriptor GetDeviceDescriptor(TDevice device);
        TDevice GetDevice(DeviceDescriptor deviceDescriptor);
    }
}

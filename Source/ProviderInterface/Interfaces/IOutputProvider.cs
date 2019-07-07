using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Provider supports Sending of Output
    /// </summary>
    public interface IOutputProvider : IProvider
    {
        ProviderReport GetOutputList();
        DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor);
        void SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        void UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        void SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state);
    }
}
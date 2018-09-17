using System;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface
{
    public interface IProvider : IDisposable
    {
        string ProviderName { get; }
        bool IsLive { get; }

        void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor , Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null);
        void RefreshLiveState();
        void RefreshDevices();
    }

    public interface IInputProvider : IProvider
    {
        ProviderReport GetInputList();
        DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq);
        bool SubscribeInput(InputSubscriptionRequest subReq);
        bool UnsubscribeInput(InputSubscriptionRequest subReq);
    }

    public interface IOutputProvider : IProvider
    {
        ProviderReport GetOutputList();
        DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq);
        bool SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state);
    }

    public interface IBindModeProvider : IProvider
    {

    }
}

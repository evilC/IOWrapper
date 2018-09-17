using System;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface
{
    public interface IProvider : IDisposable
    {
        /// <summary>
        /// The name of the provider
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// True if the Provider thinks everything has loaded OK
        /// </summary>
        bool IsLive { get; }

        /// <summary>
        /// Instruct the Provider to attempt to load dependencies etc
        /// </summary>
        void RefreshLiveState();

        /// <summary>
        /// Refresh the list of available devices
        /// </summary>
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
        void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null);
    }
}

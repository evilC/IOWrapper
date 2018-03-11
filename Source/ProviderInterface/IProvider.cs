using System;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface
{
    public enum DetectionMode { None, Subscription, Bind }

    public interface IProvider : IDisposable
    {
        string ProviderName { get; }
        bool IsLive { get; }

        ProviderReport GetInputList();
        ProviderReport GetOutputList();
        DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq);
        DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq);

        bool SetProfileState(Guid profileGuid, bool state);
        bool SubscribeInput(InputSubscriptionRequest subReq);
        bool UnsubscribeInput(InputSubscriptionRequest subReq);
        bool SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        //bool SetOutputButton(string dev, uint button, bool state);
        bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
        void EnableBindMode(Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback);
        void DisableBindMode();
        void RefreshLiveState();
        void RefreshDevices();
    }
}

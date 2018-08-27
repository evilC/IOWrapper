using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface;

namespace SpaceMouse
{
    [Export(typeof(IProvider))]
    public class SpaceMouse : IProvider
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string ProviderName { get { return typeof(SpaceMouse).Namespace; } }
        public bool IsLive { get; }
        public ProviderReport GetInputList()
        {
            return new ProviderReport();
        }

        public ProviderReport GetOutputList()
        {
            return new ProviderReport();
        }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            throw new NotImplementedException();
        }

        public void SetDetectionMode(DetectionMode detectionMode, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            throw new NotImplementedException();
        }

        public void RefreshLiveState()
        {
            throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            throw new NotImplementedException();
        }

        public void SetDetectionMode(DetectionMode detectionMode, Action callback = null)
        {
            throw new NotImplementedException();
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            throw new NotImplementedException();
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            subReq.Callback(100);
            return true;
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
}

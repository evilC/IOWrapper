using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface;

namespace Core_HidSharp
{
    [Export(typeof(IProvider))]
    public class Core_HidSharp : IProvider
    {
        private List<HidSharpDevice> _devices = new List<HidSharpDevice>();

        public Core_HidSharp()
        {
        }


        public void Dispose()
        {

        }

        public string ProviderName { get { return typeof(Core_HidSharp).Namespace; } }
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
            var device = new HidSharpDevice(subReq.DeviceDescriptor);
            _devices.Add(device);
            device.SubscribeInput(subReq);
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

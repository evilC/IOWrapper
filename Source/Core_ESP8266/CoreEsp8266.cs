using System;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_ESP8266
{
    [Export(typeof(IProvider))]
    public class CoreEsp8266 : IOutputProvider
    {
        public string ProviderName => "Core_ESP8266";
        public bool IsLive => true;
        public void RefreshLiveState()
        {
            throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            // TODO Heartbeat existing devices
            throw new NotImplementedException();
        }

        public ProviderReport GetOutputList()
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

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}

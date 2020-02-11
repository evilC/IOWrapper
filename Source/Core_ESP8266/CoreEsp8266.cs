using System;
using System.ComponentModel.Composition;
using Core_ESP8266.Managers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_ESP8266
{
    [Export(typeof(IProvider))]
    public class CoreEsp8266 : IOutputProvider
    {
        public string ProviderName => "Core_ESP8266";
        public bool IsLive => true;

        private UdpManager UdpManager { get; set; }
        private DiscoveryManager DiscoveryManager { get; set; }


        public CoreEsp8266()
        {
            UdpManager = new UdpManager();
            DiscoveryManager = new DiscoveryManager(UdpManager);
        }

        public void RefreshLiveState()
        {
            throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            // TODO Heartbeat existing devices
        }

        public ProviderReport GetOutputList()
        {
            return new ProviderReport()
            {
                Title = "Core ESP8266",
                API = ProviderName,
                Description = "Connect to external ESP8266 modules",
                Devices = DiscoveryManager.DeviceReports,
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName
                }
            };
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            return true;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return DiscoveryManager.DeviceReports.Find(d =>
                    d.DeviceDescriptor.DeviceHandle.Equals(deviceDescriptor.DeviceHandle)
                );
        }

        public void Dispose()
        {
            UdpManager?.Dispose();
        }
    }
}

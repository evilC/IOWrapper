using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        private DiscoveryManager DiscoveryManager { get; }
        private DescriptorManager DescriptorManager { get; }

        public CoreEsp8266()
        {
            UdpManager = new UdpManager();
            DiscoveryManager = new DiscoveryManager(UdpManager);
            DescriptorManager = new DescriptorManager(UdpManager);
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
                Devices = DiscoveryManager.DeviceInfos.Select(di => di.Value.DeviceReport).ToList(),
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName
                }
            };
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            DescriptorManager.WriteOutput(subReq, bindingDescriptor, state);
            return true;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            var deviceInfo = DiscoveryManager.FindDeviceInfo(subReq.DeviceDescriptor.DeviceHandle);
            if (deviceInfo == null) return false;
            return DescriptorManager.StartOutputDevice(deviceInfo);
        }
        
        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            var deviceInfo = DiscoveryManager.FindDeviceInfo(subReq.DeviceDescriptor.DeviceHandle);
            if (deviceInfo == null) return false;
            return DescriptorManager.StopOutputDevice(deviceInfo);
        }


        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return DiscoveryManager.FindDeviceInfo(deviceDescriptor.DeviceHandle).DeviceReport;
        }

        public void Dispose()
        {
            UdpManager?.Dispose();
        }
    }
}

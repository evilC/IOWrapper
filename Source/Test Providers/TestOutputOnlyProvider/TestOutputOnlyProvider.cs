using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.Core.Exceptions;
using HidWizards.IOWrapper.DataTransferObjects;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace TestOutputOnlyProvider
{
    [Export(typeof(IProvider))]
    public class TestOutputOnlyProvider : IOutputProvider
    {
        public string ProviderName { get { return typeof(TestOutputOnlyProvider).Namespace; } }
        public bool IsLive { get; }

        public void RefreshLiveState()
        {
            
        }

        public void RefreshDevices()
        {
            
        }

        private Logger _logger;

        public TestOutputOnlyProvider()
        {
            _logger = new Logger(ProviderName);
        }

        public void Dispose()
        {
            
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return null;
        }

        public void SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            if (subReq.DeviceDescriptor.DeviceHandle == "DoesNotExist")
            {
                throw new ProviderExceptions.DeviceDescriptorNotFoundException(subReq.DeviceDescriptor);
            }
            else if (subReq.DeviceDescriptor.DeviceHandle == "NotLiveProvider")
            {
                throw new ProviderExceptions.ProviderNotLiveException();
            }
        }

        public void UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            if (subReq.DeviceDescriptor.DeviceHandle == "DoesNotExist")
            {
                throw new ProviderExceptions.DeviceDescriptorNotFoundException(subReq.DeviceDescriptor);
            }
        }

        public void SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            if (subReq.DeviceDescriptor.DeviceHandle == "DoesNotExist")
            {
                throw new ProviderExceptions.DeviceDescriptorNotFoundException(subReq.DeviceDescriptor);
            }
        }
    }
}

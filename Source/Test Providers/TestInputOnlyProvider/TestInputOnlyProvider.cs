using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;

namespace TestInputOnlyProvider
{
    [Export(typeof(IProvider))]
    public class TestInputOnlyProvider : IInputProvider
    {
        public string ProviderName { get { return typeof(TestInputOnlyProvider).Namespace; } }
        private Logger _logger;

        public TestInputOnlyProvider()
        {
            _logger = new Logger(ProviderName);
        }

        public void Dispose()
        {
            
        }

        public bool IsLive { get; }
        public void RefreshLiveState()
        {
            
        }

        public void RefreshDevices()
        {
            
        }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return null;
        }
    }
}

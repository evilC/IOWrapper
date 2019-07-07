using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace HidWizards.IOWrapper.Core.Exceptions
{
    public class ProviderExceptions
    {
        [Serializable]
        public class ProviderExceptionBase : Exception
        {
            public IProvider Provider { get; set; }

            public ProviderExceptionBase(string message, IProvider provider)
                : base($"Provider {provider.ProviderName} threw exception: {message}")
            {
                Provider = provider;
            }
        }

        [Serializable]
        public class DeviceDescriptorNotFoundException : ProviderExceptionBase
        {
            public DeviceDescriptor DeviceDescriptor { get; set; }

            public DeviceDescriptorNotFoundException(IProvider provider, DeviceDescriptor deviceDescriptor)
                : base($"Device {deviceDescriptor.ToString()} not found", provider)
            {
                DeviceDescriptor = deviceDescriptor;
            }
        }

        [Serializable]
        public class UnsubscribeInputFailedException : ProviderExceptionBase
        {
            public InputSubscriptionRequest SubReq { get; set; }

            public UnsubscribeInputFailedException(IProvider provider, InputSubscriptionRequest subReq)
                : base($"Unsubscribe Failed for SubReq {subReq.ToString()}", provider)
            {
                SubReq = subReq;
            }
        }
    }
}

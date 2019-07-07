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
        public class DeviceDescriptorNotFoundException : Exception
        {
            public DeviceDescriptor DeviceDescriptor { get; set; }

            public DeviceDescriptorNotFoundException(DeviceDescriptor deviceDescriptor)
                : base($"Device {deviceDescriptor.ToString()} not found")
            {
                DeviceDescriptor = deviceDescriptor;
            }
        }
    }
}

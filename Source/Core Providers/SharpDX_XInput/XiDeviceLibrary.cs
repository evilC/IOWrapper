using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;

namespace SharpDX_XInput
{
    class XiDeviceLibrary : IInputDeviceLibrary<int>
    {
        public DeviceDescriptor GetDeviceDescriptor(int device)
        {
            throw new NotImplementedException();
        }

        public int GetDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            throw new NotImplementedException();
        }

        public ProviderReport GetInputList()
        {
            throw new NotImplementedException();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
}

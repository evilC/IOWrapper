using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;

namespace SharpDX_XInput
{
    class XiDeviceLibrary : IDeviceLibrary<int>
    {
        public DeviceDescriptor GetDeviceDescriptor(int device)
        {
            throw new NotImplementedException();
        }

        public int GetDevice(DeviceDescriptor deviceDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}

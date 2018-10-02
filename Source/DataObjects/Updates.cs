using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidWizards.IOWrapper.DataTransferObjects
{
    public struct BindingUpdate
    {
        public BindingDescriptor Binding { get; set; }
        public int Value { get; set; }
    }

    public struct BindModeUpdate
    {
        public DeviceDescriptor Device { get; set; }
        public BindingDescriptor Binding { get; set; }
        public int Value { get; set; }
    }
}

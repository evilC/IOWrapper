using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidWizards.IOWrapper.DataTransferObjects
{
    /// <summary>
    /// The primary purpose of a <see cref="BindingUpdate"/> is to provide a format which will allow us to detect, as soon as possible...
    /// ... whether a TRawUpdate is interesting or not to the system.
    /// Typically, the <see cref="BindingDescriptor"/> in the <see cref="BindingUpdate"/> will be fully-formed.
    /// However, when working with "Logical" (derived) values (eg POVs which report as Angle in DirectInput)...
    /// ... the <see cref="BindingDescriptor"/>'s Index would be the POV number and Value would hold the POV angle
    /// In this case, when this later gets translated into a full <see cref="BindingDescriptor"/>...
    /// ... SubIndex would hold the Direction and value would hold 1/0
    /// </summary>
    public struct BindingUpdate
    {
        public BindingDescriptor Binding { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// In Bind Mode, when an update happens, the front end needs all the descriptors for that input plus the value
    /// </summary>
    public struct BindModeUpdate
    {
        public DeviceDescriptor Device { get; set; }
        public BindingReport Binding { get; set; }
        public int Value { get; set; }
    }
}

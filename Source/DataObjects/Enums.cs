using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidWizards.IOWrapper.DataTransferObjects
{
    /// <summary>
    /// Describes what kind of input or output you are trying to read or emulate
    /// </summary>
    public enum BindingType { Axis, Button, POV }

    /// <summary>
    /// Describes the reporting style of a Binding
    /// Only used for the back-end to report to the front-end how to work with the binding
    /// </summary>
    public enum BindingCategory { Momentary, Event, Signed, Unsigned, Delta, Midi }
    //public enum AxisCategory { Signed, Unsigned, Delta }
    //public enum ButtonCategory { Momentary, Event }
    //public enum POVCategory { POV1, POV2, POV3, POV4 }

    public enum DetectionMode { Subscription, Bind }
}

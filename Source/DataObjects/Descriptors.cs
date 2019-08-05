using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidWizards.IOWrapper.DataTransferObjects
{
    // Descriptors are used to identify various aspects of a Binding
    // These classes control routing of the subscription request

    /// <summary>
    /// Identifies the Provider responsible for handling the Binding
    /// </summary>
    public class ProviderDescriptor
    {
        /// <summary>
        /// The API implementation that handles this input
        /// This should be unique
        /// </summary>
        public string ProviderName { get; set; }
    }

    /// <summary>
    /// Identifies a device within a Provider
    /// </summary>
    public struct DeviceDescriptor
    {
        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; set; }

        public int DeviceInstance { get; set; }

        public override string ToString()
        {
            return $"Handle {DeviceHandle}, Instance {DeviceInstance}";
        }
    }

    /// <summary>
    /// Identifies a Binding within a Device
    /// </summary>
    public struct BindingDescriptor
    {
        /// <summary>
        /// The Type of the Binding - ie Button / Axis / POV
        /// </summary>
        public BindingType Type { get; set; }

        /// <summary>
        /// The Type-specific Index of the Binding
        /// This is often a Sparse Index (it may often be a BitMask value) ...
        /// ... as it is often refers to an enum value in a Device Report
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The Type-specific SubIndex of the Binding
        /// This is typically unused, but if used generally represents a derived or optional value
        /// For example:
        ///     With each POV reporting natively as an Angle (Like an Axis)
        ///     But in IOWrapper, bindings are to a *Direction* of a POV (As if it were a button)
        ///     So we need to specify the angle of that direction in SubIndex...
        ///     ... as well as the POV# in Index. Directinput supports 4 POVs
        /// </summary>
        public int SubIndex { get; set; }
    }

    /// <summary>
    /// Identifies the Subscriber
    /// </summary>
    public class SubscriptionDescriptor
    {
        /// <summary>
        /// Uniquely identifies a Binding - each subscriber can only be subscribed to one input / output
        /// In an application such as UCR, each binding (GuiControl) can only be bound to one input / output
        /// </summary>
        public Guid SubscriberGuid { get; set; }

        // ToDo: Move?
        /// <summary>
        /// Allows grouping of subscriptions for easy toggling on / off sets of subscriptions
        /// </summary>
        public Guid ProfileGuid { get; set; }
    }
}

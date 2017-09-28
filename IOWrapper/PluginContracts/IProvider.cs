using HidSharp;
using System;
using System.Collections.Generic;

namespace Providers
{
    public interface IProvider : IDisposable
    {
        string ProviderName { get; }
        bool IsLive { get; }

        ProviderReport GetInputList();
        ProviderReport GetOutputList();
        DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq);
        DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq);

        bool SetProfileState(Guid profileGuid, bool state);
        bool SubscribeInput(InputSubscriptionRequest subReq);
        bool UnsubscribeInput(InputSubscriptionRequest subReq);
        bool SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        //bool SetOutputButton(string dev, uint button, bool state);
        bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
        void RefreshLiveState();
    }

    #region Subscription Requests
    /// <summary>
    /// Base class for Subscription Requests. Shared by Input and Output
    /// </summary>
    public class SubscriptionRequest
    {
        /// <summary>
        /// Identifies the Subscriber
        /// </summary>
        public SubscriptionDescriptor SubscriptionDescriptor { get; set; }

        /// <summary>
        /// Identifies the Provider that this subscription is for
        /// </summary>
        public ProviderDescriptor ProviderDescriptor { get; set; }

        /// <summary>
        /// Identifies which (Provider-specific) Device that this subscription is for
        /// </summary>
        public DeviceDescriptor DeviceDescriptor { get; set; }
    }

    /// <summary>
    /// Contains all the required information for :
    ///     The IOController to route the request to the appropriate Provider
    ///     The Provider to subscribe to the appropriate input
    ///     The Provider to notify the subscriber of activity
    /// </summary>
    public class InputSubscriptionRequest : SubscriptionRequest
    {
        /// <summary>
        /// Identifies the (Provider+Device-specific) Input that this subscription is for
        /// </summary>
        public BindingDescriptor BindingDescriptor { get; set; }

        /// <summary>
        /// Callback to be fired when this Input changes state
        /// </summary>
        public dynamic Callback { get; set; }

        public InputSubscriptionRequest Clone()
        {
            return (InputSubscriptionRequest)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Contains all the information for:
    ///     The IOController to route the request to the appropriate Provider
    ///     
    /// Output Subscriptions are typically used to eg create virtual devices...
    /// ... so that output can be sent to them
    /// </summary>
    public class OutputSubscriptionRequest : SubscriptionRequest {
        public OutputSubscriptionRequest Clone()
        {
            return (OutputSubscriptionRequest)this.MemberwiseClone();
        }
    }
    #endregion

    // Reports allow the back-end to tell the front-end what capabilities are available
    // Reports comprise of two parts:
    // Descriptors allow the front-end to subscribe to Bindings
    // Other meta-data allows the front-end to interpret capabilities, report style etc
    #region Reporting

    #region Provider Report

    /// <summary>
    /// Contains information about each provider
    /// </summary>
    public class ProviderReport
    {
        /// <summary>
        /// The human-friendly name of the Provider
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A description of what the Provider does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Provider
        /// </summary>
        public ProviderDescriptor ProviderDescriptor { get; set; }

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// This meta-data is intended to allow the front-end to present a user with alternatives
        /// </summary>
        public string API { get; set; }

        public List<DeviceReport> Devices { get; set; } = new List<DeviceReport>();
    }

    #endregion

    #region Device Reports

    /// <summary>
    /// Contains information about each Device on a Provider
    /// </summary>
    public class DeviceReport
    {
        /// <summary>
        /// The human-friendly name of the device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Device via the Provider
        /// </summary>
        public DeviceDescriptor DeviceDescriptor { get; set; }

        //public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>();

        /// <summary>
        /// Nodes give the device report structure and allow the front-end to logically group items
        /// </summary>
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();
    }

    #region Binding Report

    /// <summary>
    /// Contains information about each Binding on a Device on a Provider
    /// </summary>
    public class BindingReport
    {
        /// <summary>
        /// Meta-Data for the front-end to display a Human-Readable name for the Binding
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Meta-Data to allow the front-end to interpret the Binding
        /// </summary>
        public BindingCategory Category { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Binding
        /// </summary>
        public BindingDescriptor BindingDescriptor { get; set; }
    }
    #endregion

    /// <summary>
    /// Used as a sub-node, to logically group Bindings
    /// </summary>
    public class DeviceReportNode
    {
        /// <summary>
        /// Human-friendly name of this node
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Sub-Nodes
        /// </summary>
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();

        /// <summary>
        /// Bindings contained in this node
        /// </summary>
        public List<BindingReport> Bindings { get; set; } = new List<BindingReport>();
    }

    #endregion

    #endregion

    #region Descriptors
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
    public class DeviceDescriptor
    {
        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; set; }

        public int DeviceInstance { get; set; }
    }

    /// <summary>
    /// Identifies a Binding within a Device
    /// </summary>
    public class BindingDescriptor
    {
        /// <summary>
        /// The Type of the Binding - ie Button / Axis / POV
        /// </summary>
        public BindingType Type { get; set; }

        /// <summary>
        /// The Type-specific Index of the Binding
        /// </summary>
        public int Index { get; set; } = 0;

        // Currently un-used. May be desirable for POV directions
        /// <summary>
        /// The Type-specific SubIndex of the Binding
        /// </summary>
        public int SubIndex { get; set; } = 0;
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
    #endregion

    // Enums used to categorize how a binding reports
    #region Category Enums
    
    /// <summary>
    /// Describes what kind of input or output you are trying to read or emulate
    /// </summary>
    public enum BindingType { Axis, Button, POV };

    /// <summary>
    /// Describes the reporting style of a Binding
    /// Only used for the back-end to report to the front-end how to work with the binding
    /// </summary>
    public enum BindingCategory { Momentary, Event, Signed, Unsigned, Delta }
    //public enum AxisCategory { Signed, Unsigned, Delta }
    //public enum ButtonCategory { Momentary, Event }
    //public enum POVCategory { POV1, POV2, POV3, POV4 }
    #endregion

    #region Helper Classes
    static public class DeviceHelper
    {
        static public HidDeviceLoader loader = new HidDeviceLoader();

        public static string GetDeviceName(int vid, int pid, int? ser = null)
        {
            string str = "Unknown Device";
            try
            {
                var result = loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.Manufacturer;
                if (str.Length > 0)
                    str += " ";
                str += result.ProductName;
            }
            catch { };
            return str;
        }

        public static string GetDevicePath(int vid, int pid, int? ser = null)
        {
            string str = null;
            try
            {
                var result = loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.DevicePath;
            }
            catch { }
            return str;
        }
    }
    #endregion
}

using HidSharp;
using System;
using System.Collections.Generic;

namespace Providers
{
    public interface IProvider : IDisposable
    {
        string ProviderName { get; }
        ProviderReport GetInputList();
        ProviderReport GetOutputList();

        bool SetProfileState(Guid profileGuid, bool state);
        bool SubscribeInput(InputSubscriptionRequest subReq);
        bool UnsubscribeInput(InputSubscriptionRequest subReq);
        bool SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        //bool SetOutputButton(string dev, uint button, bool state);
        bool SetOutputState(OutputSubscriptionRequest subReq, BindingType inputType, uint inputIndex, int state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
    }

    #region Subscription Requests
    /// <summary>
    /// Base class for Subscription Requests. Shared by Input and Output
    /// </summary>
    public class SubscriptionRequest
    {
        public SubscriptionInfo SubscriptionInfo { get; set; }

        /// <summary>
        /// Identifies which Device this subscription is for
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }

        public ProviderInfo ProviderInfo { get; set; }
    }

    /// <summary>
    /// Contains all the required information for :
    ///     The IOController to route the request to the appropriate Provider
    ///     The Provider to subscribe to the appropriate input
    ///     The Provider to notify the subscriber of activity
    /// </summary>
    public class InputSubscriptionRequest : SubscriptionRequest
    {
        public BindingInfo BindingInfo { get; set; }

        /// <summary>
        /// Callback that is fired when this input changes state
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

        public ProviderInfo ProviderInfo { get; set; }

        public SortedDictionary<string, DeviceReport> Devices { get; set; }
            = new SortedDictionary<string, DeviceReport>();
    }

    #endregion

    #region Device Reports

    public class DeviceReport
    {
        /// <summary>
        /// The human-friendly name of the device
        /// </summary>
        public string DeviceName { get; set; }

        public DeviceInfo DeviceInfo { get; set; }

        //public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>();

        /// <summary>
        /// Nodes give the device report structure and allow the front-end to logically group items
        /// </summary>
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();
    }

    public class DeviceReportNode
    {
        public string Title { get; set; }
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();
        public List<BindingReport> Bindings { get; set; } = new List<BindingReport>();
    }

    #endregion

    #region Binding Report

    public class BindingReport
    {
        public string Title { get; set; }   // ToDo: move, meta-data
        public BindingCategory Category { get; set; }   // ToDo: move, meta-data
        public BindingInfo BindingInfo { get; set; }
    }
    #endregion

    #endregion

    #region Descriptors
    // Descriptors are used to identify various aspects of a Binding
    // These classes control routing of the subscription request

    /// <summary>
    /// Identifies the Provider responsible for handling the Binding
    /// </summary>
    public class ProviderInfo
    {
        /// <summary>
        /// The API implementation that handles this input
        /// This should be unique
        /// </summary>
        public string ProviderName { get; set; }

        // ToDo: Move out of this class - is meta-data
        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// </summary>
        public string API { get; set; }
    }

    /// <summary>
    /// Identifies a device within a Provider
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; set; }

        //public int DeviceInstance { get; set; }
    }

    /// <summary>
    /// Identifies a Binding within a Device
    /// </summary>
    public class BindingInfo
    {
        public BindingType Type { get; set; }
        public int Index { get; set; }
        public int SubIndex { get; set; }
    }

    /// <summary>
    /// Identifies the Subscriber
    /// </summary>
    public class SubscriptionInfo
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

    /// <summary>
    /// Enums used to categorize how a binding reports
    /// </summary>
    #region Category Enums
    public enum BindingType { Axis, Button, POV };

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

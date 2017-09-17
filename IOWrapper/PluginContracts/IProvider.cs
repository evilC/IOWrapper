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

    #region Subscriptions
    public class InputSubscriptionRequest : SubscriptionRequest
    {
        public BindingType InputType { get; set; }
        //public string SubscriberId { get; set; }
        public uint InputIndex { get; set; }
        public dynamic Callback { get; set; }
        // used, eg, for DirectInput POV number
        public int InputSubIndex { get; set; } = 0;
        public Guid ProfileGuid { get; set; }
        public InputSubscriptionRequest Clone()
        {
            return (InputSubscriptionRequest)this.MemberwiseClone();
        }
    }

    public class OutputSubscriptionRequest : SubscriptionRequest {
        public OutputSubscriptionRequest Clone()
        {
            return (OutputSubscriptionRequest)this.MemberwiseClone();
        }
    }

    public class SubscriptionRequest
    {
        public Guid SubscriberGuid { get; set; }
        public string ProviderName { get; set; }
        public string SubProviderName { get; set; } = null;
        public string DeviceHandle { get; set; }
    }
    #endregion

    #region Reporting
    #region Provider Report
    public class ProviderReport
    {
        public SortedDictionary<string, IOWrapperDevice> Devices { get; set; }
            = new SortedDictionary<string, IOWrapperDevice>();
    }
    #endregion

    #region Report Node
    public class DeviceNode
    {
        public string Title { get; set; }
        public List<DeviceNode> Nodes { get; set; } = new List<DeviceNode>();
        public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>();
    }
    #endregion

    #region Device Report
    public class IOWrapperDevice
    {
        /// <summary>
        /// The human-friendly name of the device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; set; }

        /// <summary>
        /// The API implementation that handles this input
        /// This should be unique
        /// </summary>
        public string ProviderName { get; set; }

        public string SubProviderName { get; set; }

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// </summary>
        public string API { get; set; }

        //public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>();

        /// <summary>
        /// Nodes give the device report structure and allow the front-end to logically group items
        /// </summary>
        public List<DeviceNode> Nodes { get; set; } = new List<DeviceNode>();
    }
    #endregion

    #region Binding Report
    public class BindingInfo
    {
        public string Title { get; set; }
        public BindingType Type { get; set; }
        public int Index { get; set; }
        public int SubIndex { get; set; }
        public List<BindingInfo> SubBindings { get; set; } = new List<BindingInfo>();
    }

    public class ButtonBindingInfo : BindingInfo
    {
        public ButtonCategory Category { get; set; }
    }

    public class AxisBindingInfo : BindingInfo
    {
        public AxisCategory Category { get; set; }
    }

    public class POVBindingInfo : BindingInfo
    {
        public POVCategory Category { get; set; }
    }
    #endregion

    /// <summary>
    /// Enums used to categorize how a binding reports
    /// </summary>
    #region Category Enums
    public enum BindingType { AXIS, BUTTON, POV };

    public enum AxisCategory { Signed, Unsigned, Delta }
    public enum ButtonCategory { Momentary, Event }
    public enum POVCategory { POV1, POV2, POV3, POV4 }
    #endregion
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

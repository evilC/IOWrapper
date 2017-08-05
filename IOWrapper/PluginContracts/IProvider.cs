using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers
{
    public interface IProvider
    {
        string ProviderName { get; }
        ProviderReport GetInputList();
        Guid? SubscribeButton(SubscriptionRequest subReq);
        bool UnsubscribeButton(Guid subscriptionGuid);
        Guid? SubscribeOutputDevice(SubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(Guid deviceSubscription);
        //bool SetOutputButton(string dev, uint button, bool state);
        bool SetOutputButton(Guid deviceSubscription, uint button, bool state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
    }

    public enum InputType { AXIS, BUTTON, POV };

    #region Subscriptions
    public class SubscriptionRequest
    {
        public InputType InputType { get; set; }
        //public string SubscriberId { get; set; }
        public string ProviderName { get; set; }
        public string DeviceHandle { get; set; }
        public uint InputIndex { get; set; }
        public dynamic Callback { get; set; }
        // used, eg, for DirectInput POV number
        public int InputSubId { get; set; } = 0;
    }
    #endregion

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

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// </summary>
        public string API { get; set; }

        /// <summary>
        /// How many buttons the device has
        /// </summary>
        public uint ButtonCount { get; set; } = 0;

        /// <summary>
        /// The names of the buttons.
        /// If ommitted, buttons numbers will be communicated to the user
        /// </summary>
        public List<string> ButtonNames { get; set; }
    }

    public class ProviderReport
    {
        public SortedDictionary<string, IOWrapperDevice> Devices { get; set; }
            = new SortedDictionary<string, IOWrapperDevice>();
    }
}

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
        bool SetOutputState(OutputSubscriptionRequest subReq, InputType inputType, uint inputIndex, int state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
    }

    public enum InputType { AXIS, BUTTON, POV };

    #region Subscriptions
    public class InputSubscriptionRequest : SubscriptionRequest
    {
        public InputType InputType { get; set; }
        //public string SubscriberId { get; set; }
        public uint InputIndex { get; set; }
        public dynamic Callback { get; set; }
        // used, eg, for DirectInput POV number
        public int InputSubId { get; set; } = 0;
        public Guid ProfileGuid { get; set; }
        public InputSubscriptionRequest Clone()
        {
            return (InputSubscriptionRequest)this.MemberwiseClone();
        }
    }

    public class OutputSubscriptionRequest : SubscriptionRequest { }

    public class SubscriptionRequest
    {
        public Guid SubscriberGuid { get; set; }
        public string ProviderName { get; set; }
        public string DeviceHandle { get; set; }
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

        public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>();
        /// <summary>
        /// A list of the device's buttons.
        /// Note that for some devices (eg keyboard), the indexes of the buttons may not be contiguous
        /// ie it may have index 1,3,5
        /// </summary>
        public List<ButtonInfo> ButtonList { get; set; }

        /// <summary>
        /// A List of Axes IDs that this axis supports
        /// </summary>
        public List<AxisInfo> AxisList { get; set; }
    }

    public class InputInfo
    {
        /// <summary>
        /// The index (zero based) of this input
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The name of this input.
        /// If purely a button number, then the first button (Index 0) should be called "1"
        /// </summary>
        public string Name { get; set; }
    }

    public class ButtonInfo : InputInfo
    {
        /// <summary>
        /// Buttons with this property set to true only have one state
        /// eg Mouse Wheel, which has no "press" or "release" event
        /// </summary>
        public bool IsEvent { get; set; }
    }

    public class AxisInfo : InputInfo
    {
        /// <summary>
        /// Axes with this property set to true do not rest in the middle
        /// eg a Pedal or a Trigger on an Xbox controller
        /// </summary>
        public bool IsUnsigned { get; set; }
    }

    public class ProviderReport
    {
        public SortedDictionary<string, IOWrapperDevice> Devices { get; set; }
            = new SortedDictionary<string, IOWrapperDevice>();
    }

    public class BindingInfo
    {
        public enum InputCategory
        {
            Button,
            Event,
            Range,
            Trigger,
            Delta
        }

        public string Title { get; set; }
        public bool IsBinding { get; set; }
        public InputCategory Category { get; set; }
        public InputType InputType { get; set; }
        public int InputIndex { get; set; }
        public int InputSubIndex { get; set; }
        public List<BindingInfo> SubBindings { get; set; } = new List<BindingInfo>();
    }
}

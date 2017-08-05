using System;
using System.ComponentModel.Composition;
using Providers;
using System.Collections.Generic;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IProvider
    {
        #region IProvider Members

        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }
        private Controller[] controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

        private readonly static List<string> buttonNames = new List<string>()
            { "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start", "Xbox" };

        public ProviderReport GetInputList()
        {
            var i = 0;
            var dr = new ProviderReport();
            foreach (var ctrlr in controllers)
            {
                if (ctrlr.IsConnected)
                {
                    dr.Devices.Add(i.ToString(), BuildXInputDevice(i));
                }
                i++;
            }
            return dr;
        }

        public bool SubscribeButton(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnsubscribeButton(InputSubscriptionRequest subReq)
        {

            return false;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool SetOutputButton(OutputSubscriptionRequest subReq, uint button, bool state)
        {
            return false;
        }
        #endregion

        private IOWrapperDevice BuildXInputDevice(int id)
        {
            return new IOWrapperDevice()
            {
                DeviceHandle = id.ToString(),
                ProviderName = ProviderName,
                API = "XInput",
                ButtonCount = 11,
                ButtonNames = buttonNames
            };
        }
        public class XInputDevice
        {
            /// <summary>
            /// The human-friendly name of the device
            /// </summary>
            public string DeviceName { get; set; } = "XBox Controller #";

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
            public string ProviderName { get; set; } = "SharpDX_XInput";

            /// <summary>
            /// The underlying API that handles this input
            /// It is intended that many providers could support a given API
            /// </summary>

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

    }
}

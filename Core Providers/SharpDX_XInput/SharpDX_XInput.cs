using System;
using System.ComponentModel.Composition;
using Providers;
using System.Collections.Generic;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IProvider
    {
        #region IProvider Members

        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

        public ProviderReport GetInputList()
        {
            var dr = new ProviderReport();
            dr.Devices.Add("0", new IOWrapperDevice()
            {
                DeviceHandle = "0",
                ProviderName = ProviderName,
                API = "XInput",
                ButtonCount = 11,
                ButtonNames = new List<string>() { "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start", "Xbox" }
            });
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
    }
}

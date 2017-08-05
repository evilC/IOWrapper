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

        public Guid? SubscribeButton(SubscriptionRequest subReq)
        {
            return null;
        }

        public bool UnsubscribeButton(Guid subscriptionGuid)
        {

            return false;
        }

        public Guid? SubscribeOutputDevice(SubscriptionRequest subReq)
        {
            return null;
        }

        public bool UnSubscribeOutputDevice(Guid deviceSubscription)
        {
            return false;
        }

        public bool SetOutputButton(Guid deviceSubscription, uint button, bool state)
        {
            return false;
        }
        #endregion
    }
}

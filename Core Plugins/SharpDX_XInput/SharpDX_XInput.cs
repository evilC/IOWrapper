using System;
using System.ComponentModel.Composition;
using PluginContracts;
using System.Collections.Generic;

namespace SharpDX_XInput
{
    [Export(typeof(IPlugin))]
    public class SharpDX_XInput : IPlugin
    {
        #region IPlugin Members

        public string PluginName { get { return typeof(SharpDX_XInput).Namespace; } }

        public ProviderReport GetInputList()
        {
            var dr = new ProviderReport();
            dr.Devices.Add("0", new IOWrapperDevice()
            {
                DeviceHandle = "0",
                PluginName = PluginName,
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

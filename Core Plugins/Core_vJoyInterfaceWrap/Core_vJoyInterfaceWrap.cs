using PluginContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_vJoyInterfaceWrap
{
    [Export(typeof(IPlugin))]
    public class Core_vJoyInterfaceWrap : IPlugin
    {
        #region IPlugin Members

        public string PluginName { get { return typeof(Core_vJoyInterfaceWrap).Namespace; } }

        public DeviceReport GetInputList()
        {
            return null;
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
        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Devices;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptKeyboardHandler : PolledDeviceHandler<ManagedWrapper.Stroke, (BindingType, int)>
    {
        public IceptKeyboardHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
        }

        protected override IDeviceUpdateHandler<ManagedWrapper.Stroke> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new IceptKeyboardUpdateHandler(deviceDescriptor, SubHandler, bindModeHandler);
        }

        public override void Poll(ManagedWrapper.Stroke update)
        {
            DeviceUpdateHandler.ProcessUpdate(update);
        }
    }
}

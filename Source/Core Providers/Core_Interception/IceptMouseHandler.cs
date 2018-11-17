using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptMouseHandler : DeviceHandlerBase<ManagedWrapper.Stroke>
    {
        private readonly IceptDeviceLibrary _deviceLibrary;

        public IceptMouseHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler, IceptDeviceLibrary deviceLibrary)
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
        }

        protected override IDeviceUpdateHandler<ManagedWrapper.Stroke> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new IceptMouseUpdateHandler(deviceDescriptor, SubHandler, bindModeHandler, _deviceLibrary);
        }

        public override bool Poll(ManagedWrapper.Stroke update)
        {
            return DeviceUpdateHandler.ProcessUpdate(update);
        }
    }
}

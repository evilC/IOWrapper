using System;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Devices
{
    public abstract class PolledDeviceHandler<TUpdate, TProcessorKey> : DeviceHandlerBase<TUpdate, TProcessorKey>
    {
        protected PolledDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
        }

        public abstract void Poll(TUpdate update);
    }
}

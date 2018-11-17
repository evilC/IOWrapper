using System;
using System.Threading;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    public abstract class PollingDeviceHandler<TUpdate, TProcessorKey> : DeviceUpdateHandler<TUpdate, TProcessorKey>
    {
        private Thread _pollThread;

        protected PollingDeviceHandler(DeviceDescriptor deviceDescriptor, ISubscriptionHandler subHandler, EventHandler<BindModeUpdate> bindModeHandler)
            : base(deviceDescriptor, subHandler, bindModeHandler)
        {
            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        protected abstract void PollThread();

        public override void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _pollThread = null;
        }
    }
}

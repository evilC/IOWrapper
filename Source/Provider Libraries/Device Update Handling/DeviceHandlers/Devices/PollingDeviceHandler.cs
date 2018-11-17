using System;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    public abstract class PollingDeviceHandlerBase<TRawUpdate, TProcessorKey> : DeviceHandlerBase<TRawUpdate, TProcessorKey>
    {
        private Thread _pollThread;

        protected PollingDeviceHandlerBase(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
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

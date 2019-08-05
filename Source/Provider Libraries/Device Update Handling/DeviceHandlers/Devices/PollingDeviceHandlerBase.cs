using System;
using System.Diagnostics;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    /// <summary>
    /// Handles scheduled calling of the ProcessUpdate method
    /// </summary>
    /// <typeparam name="TRawUpdate"></typeparam>
    /// <typeparam name="TProcessorKey"></typeparam>
    public abstract class PollingDeviceHandlerBase<TRawUpdate, TProcessorKey> : DeviceHandlerBase<TRawUpdate, TProcessorKey>
    {
        /// <summary>
        /// The thread which handles polling
        /// </summary>
        private Thread _pollThread;

        /// <summary>
        /// Set to true in the thread once it has acquired stick etc and is actually polling
        /// </summary>
        protected volatile bool PollThreadPolling = false;

        /// <summary>
        /// Set to false to tell the thread to terminate
        /// </summary>
        protected volatile bool PollThreadDesired = true;

        protected PollingDeviceHandlerBase(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
        }

        /// <summary>
        /// Start the PollThread
        /// </summary>
        public override void Init()
        {
            _pollThread = new Thread(PollThread);
            _pollThread.Start();
            var timeout = Environment.TickCount + 5000;
            while (!PollThreadPolling && Environment.TickCount < timeout) // Wait for PollThread to start
            {
                Thread.Sleep(10);
            }

            if (!PollThreadPolling)
            {
                throw new Exception($"Device {DeviceDescriptor.ToString()} is not connected");
            }
        }

        protected abstract void PollThread();

        public override void Dispose()
        {
            Debug.WriteLine("IOWrapper| Entering Dispose");
            if (_pollThread == null) return;
            Debug.WriteLine("IOWrapper| Signalling thread to stop...");
            PollThreadDesired = false;
            //Debug.WriteLine("IOWrapper| Aborting thread...");
            //_pollThread.Abort();
            Debug.WriteLine("IOWrapper| Joining thread...");
            _pollThread.Join();
            Debug.WriteLine("IOWrapper| Nulling out var");
            _pollThread = null;
            Debug.WriteLine("IOWrapper| Dispose done");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Subscriptions;
using HidWizards.IOWrapper.ProviderInterface.Updates;

namespace HidWizards.IOWrapper.ProviderInterface.Devices
{
    public abstract class PollingDeviceHandler<TUpdate, TProcessorKey> : IDisposable
    {
        private Thread _pollThread;
        protected DeviceUpdateHandler<TUpdate, TProcessorKey> _deviceUpdateHandler;
        protected SubscriptionHandler _subHandler;
        protected DeviceDescriptor _deviceDescriptor;

        protected PollingDeviceHandler(DeviceDescriptor deviceDescriptor)
        {
            _deviceDescriptor = deviceDescriptor;
        }

        public PollingDeviceHandler<TUpdate, TProcessorKey> Initialize(EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            _subHandler = new SubscriptionHandler(_deviceDescriptor, deviceEmptyHandler);
            _deviceUpdateHandler = CreateUpdateHandler(_deviceDescriptor, _subHandler, bindModeHandler);

            _pollThread = new Thread(PollThread);
            _pollThread.Start();

            return this;
        }

        public bool IsEmpty()
        {
            return _subHandler.Count() == 0;
        }

        public void SetDetectionMode(DetectionMode detectionMode)
        {
            _deviceUpdateHandler.SetDetectionMode(detectionMode);
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Subscribe(subReq);
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Unsubscribe(subReq);
        }

        protected abstract DeviceUpdateHandler<TUpdate, TProcessorKey> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler, EventHandler<BindModeUpdate> bindModeHandler);

        protected abstract void PollThread();


        public void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _pollThread = null;
        }
    }
}

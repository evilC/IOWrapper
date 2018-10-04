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

        protected PollingDeviceHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            _deviceDescriptor = deviceDescriptor;
            _subHandler = new SubscriptionHandler(deviceDescriptor, deviceEmptyHandler);

            InitializeUpdateHandler(deviceDescriptor, bindModeHandler);

            // ToDo: This should not be called in the base constructor, it should be called after the derived ctor finishes. Guid / controller ID set in ctor of derived class will not be set yet
            _pollThread = new Thread(PollThread);
            _pollThread.Start();
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


        private void InitializeUpdateHandler(DeviceDescriptor deviceDescriptor, EventHandler<BindModeUpdate> bindModeHandler)
        {
            _deviceUpdateHandler = CreateUpdateHandler(deviceDescriptor, _subHandler, bindModeHandler);
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

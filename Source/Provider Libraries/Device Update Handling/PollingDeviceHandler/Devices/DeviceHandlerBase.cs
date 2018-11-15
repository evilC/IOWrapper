using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Devices
{
    public abstract class DeviceHandlerBase<TUpdate, TProcessorKey>
    {
        protected IDeviceUpdateHandler<TUpdate> DeviceUpdateHandler;
        protected SubscriptionHandler SubHandler;
        protected DeviceDescriptor DeviceDescriptor;

        protected DeviceHandlerBase(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            DeviceDescriptor = deviceDescriptor;
            Initialize(deviceEmptyHandler, bindModeHandler);
        }

        private void Initialize(EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            SubHandler = new SubscriptionHandler(DeviceDescriptor, deviceEmptyHandler);
            DeviceUpdateHandler = CreateUpdateHandler(DeviceDescriptor, SubHandler, bindModeHandler);
        }

        public bool IsEmpty()
        {
            return SubHandler.Count() == 0;
        }

        public void SetDetectionMode(DetectionMode detectionMode)
        {
            DeviceUpdateHandler.SetDetectionMode(detectionMode);
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Subscribe(subReq);
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Unsubscribe(subReq);
        }

        protected abstract IDeviceUpdateHandler<TUpdate> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler, EventHandler<BindModeUpdate> bindModeHandler);

    }
}

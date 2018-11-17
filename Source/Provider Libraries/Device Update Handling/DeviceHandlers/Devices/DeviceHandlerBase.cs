using System;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    public abstract class DeviceHandlerBase<TUpdate>
    {
        protected IDeviceUpdateHandler<TUpdate> DeviceUpdateHandler;
        protected ISubscriptionHandler SubHandler;
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

        protected abstract IDeviceUpdateHandler<TUpdate> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, ISubscriptionHandler subscriptionHandler, EventHandler<BindModeUpdate> bindModeHandler);
        public abstract bool Poll(TUpdate update);
    }
}

using System;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    public interface IDeviceHandler<TUpdate>
    {
        /// <summary>
        /// Process an update from a device
        /// </summary>
        /// <param name="rawUpdate">The raw update from the device</param>
        /// <returns>Request blocking of input if true, else not requesting block</returns>
        bool ProcessUpdate(TUpdate rawUpdate);

        void SetDetectionMode(DetectionMode mode);

        event EventHandler<BindModeUpdate> BindModeUpdate;

        bool IsEmpty();
        void SubscribeInput(InputSubscriptionRequest subReq);
        void UnsubscribeInput(InputSubscriptionRequest subReq);
    }
}
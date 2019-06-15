using System;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    /// <inheritdoc />
    /// <summary>
    /// Describes a class which a provider can use to handle all functions related to a specific device
    /// </summary>
    /// <typeparam name="TRawUpdate">The format that Raw Updates come in from the device</typeparam>
    public interface IDeviceHandler<TRawUpdate> : IDisposable
    {
        /// <summary>
        /// Process an update from a device
        /// </summary>
        /// <param name="rawUpdate">The raw update from the device</param>
        /// <returns>Request blocking of input if true, else not requesting block</returns>
        bool ProcessUpdate(TRawUpdate rawUpdate);

        /// <summary>
        /// Turns on or off Bind Mode
        /// </summary>
        /// <param name="mode"></param>
        void SetDetectionMode(DetectionMode mode);

        /// <summary>
        /// Fired when there is an update in Bind Mode
        /// </summary>
        event EventHandler<BindModeUpdate> BindModeUpdate;

        /// <summary>
        /// Used to check if the device has no subscriptions
        /// </summary>
        /// <returns></returns>
        bool IsEmpty();

        void SubscribeInput(InputSubscriptionRequest subReq);
        void UnsubscribeInput(InputSubscriptionRequest subReq);
    }
}

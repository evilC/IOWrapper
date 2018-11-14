using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Updates
{
    public interface IDeviceUpdateHandler<TUpdate>
    {
        /// <summary>
        /// Process an update from a device
        /// </summary>
        /// <param name="rawUpdate">The raw update from the device</param>
        /// <returns>Request blocking of input if true, else not requesting block</returns>
        bool ProcessUpdate(TUpdate rawUpdate);

        void SetDetectionMode(DetectionMode mode);
    }
}
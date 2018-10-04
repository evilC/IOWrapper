using HidWizards.IOWrapper.DataTransferObjects;

namespace PollingDeviceHandlers.Updates
{
    public interface IDeviceUpdateHandler<TUpdate>
    {
        void ProcessUpdate(TUpdate rawUpdate);
        void SetDetectionMode(DetectionMode mode);
    }
}
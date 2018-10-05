using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.PollingDeviceHandlers.Updates
{
    public interface IDeviceUpdateHandler<TUpdate>
    {
        void ProcessUpdate(TUpdate rawUpdate);
        void SetDetectionMode(DetectionMode mode);
    }
}
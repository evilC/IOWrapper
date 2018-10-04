using HidWizards.IOWrapper.DataTransferObjects;

namespace ProviderHelpers.Updates
{
    public interface IDeviceUpdateHandler<TUpdate>
    {
        void ProcessUpdate(TUpdate rawUpdate);
        void SetDetectionMode(DetectionMode mode);
    }
}
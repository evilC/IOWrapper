using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Updates
{
    public interface IDeviceUpdateHandler<TUpdate>
    {
        void ProcessUpdate(TUpdate rawUpdate);
        void SetDetectionMode(DetectionMode mode);
    }
}
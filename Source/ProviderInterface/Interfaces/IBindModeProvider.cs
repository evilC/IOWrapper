using System;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Provider supports "Bind Mode" (Press any input to bind)
    /// </summary>
    public interface IBindModeProvider : IProvider
    {
        void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> callback = null);
    }
}
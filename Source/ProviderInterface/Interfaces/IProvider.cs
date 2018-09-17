using System;

namespace HidWizards.IOWrapper.ProviderInterface.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// A Base Interface that all Providers must implement
    /// </summary>
    public interface IProvider : IDisposable
    {
        /// <summary>
        /// The name of the provider
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// True if the Provider thinks everything has loaded OK
        /// </summary>
        bool IsLive { get; }

        /// <summary>
        /// Instruct the Provider to attempt to load dependencies etc
        /// </summary>
        void RefreshLiveState();

        /// <summary>
        /// Refresh the list of available devices
        /// </summary>
        void RefreshDevices();
    }
}

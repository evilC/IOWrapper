using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput.Handlers
{
    /// <summary>
    /// Handles input detection for this provider
    /// </summary>
    class DiHandler : ApiHandler<DiDeviceHandler>
    {
        public static DirectInput DiInstance { get; } = new DirectInput();
    }
}

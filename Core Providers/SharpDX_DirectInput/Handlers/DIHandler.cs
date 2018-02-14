using System.Collections.Concurrent;
using IProvider;
using IProvider.Handlers;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput.Handlers
{
    /// <inheritdoc />
    /// <summary>
    /// Handles input detection for this provider
    /// ToDo: Handle device unplug / replug better
    /// Will probably need some exterior mechanism (USB HID?) to detect plug / unplug
    /// Mechanism probably belongs in the core, but this provider would need to support it
    /// As an interim measure, we could probably have a "Refresh" button in the UI
    /// </summary>
    internal class DiHandler : ApiHandler
    {
        public static DirectInput DiInstance { get; } = new DirectInput();

        public override DeviceHandler CreateDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new DiDeviceHandler(subReq);
        }
    }
}

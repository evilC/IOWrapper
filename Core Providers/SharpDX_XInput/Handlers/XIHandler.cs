using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;

namespace SharpDX_XInput.Handlers
{
    public class XiHandler : ApiHandler
    {
        public override DeviceHandler CreateDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new XiDeviceHandler();
        }
    }
}

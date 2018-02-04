using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;

namespace SharpDX_XInput.Handlers
{
    public class XiHandler : ApiHandler<XiDeviceHandler>
    {
        public override ConcurrentDictionary<int, XiDeviceHandler> GetDeviceHandlerDictionary()
        {
            return new ConcurrentDictionary<int, XiDeviceHandler>();
        }

        public override XiDeviceHandler GetDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new XiDeviceHandler(subReq);
        }
    }
}

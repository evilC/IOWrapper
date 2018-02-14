using System.Collections.Concurrent;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;

namespace SharpDX_XInput.Handlers
{
    public class XiHandler : ApiHandler
    {
        public override DeviceHandler CreateDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new XiDeviceHandler(subReq);
        }
    }
}

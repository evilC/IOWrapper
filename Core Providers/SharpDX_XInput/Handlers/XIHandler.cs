using Providers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Providers.Handlers;

namespace SharpDX_XInput
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

using Providers;
using Providers.Helpers;
using SharpDX.DirectInput;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Providers.Handlers;

namespace SharpDX_DirectInput
{
    /// <summary>
    /// Handles input detection for this provider
    /// </summary>
    class DiHandler : ApiHandler<DiDeviceHandler>
    {
        public static DirectInput DiInstance { get; } = new DirectInput();

        public override ConcurrentDictionary<int, DiDeviceHandler> GetDeviceHandlerDictionary()
        {
            return new ConcurrentDictionary<int, DiDeviceHandler>();
        }

        public override DiDeviceHandler GetDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new DiDeviceHandler(subReq);
        }
    }
}

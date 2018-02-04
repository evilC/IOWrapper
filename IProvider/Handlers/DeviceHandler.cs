using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    public abstract class DeviceHandler
    {
        public abstract bool Subscribe(InputSubscriptionRequest subReq);
        public abstract bool Unsubscribe(InputSubscriptionRequest subReq);
        public abstract void Poll();
    }
}

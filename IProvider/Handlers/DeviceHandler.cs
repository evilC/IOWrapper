using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    public abstract class DeviceHandler
    {
        protected ConcurrentDictionary<BindingType,
            ConcurrentDictionary<int, BindingHandler>> _bindingDictionary
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();

        public abstract bool Subscribe(InputSubscriptionRequest subReq);
        public abstract bool Unsubscribe(InputSubscriptionRequest subReq);
        public abstract void Poll();
    }
}

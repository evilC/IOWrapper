using Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    public abstract class ApiHandler<TDeviceType>
    {
        public abstract bool Subscribe(InputSubscriptionRequest subReq);
        public abstract bool Unsubscribe(InputSubscriptionRequest subReq);
        public abstract ConcurrentDictionary<int, TDeviceType> GetDeviceHandlerDictionary();
        public abstract TDeviceType GetDeviceHandler(InputSubscriptionRequest subReq);
    }
}

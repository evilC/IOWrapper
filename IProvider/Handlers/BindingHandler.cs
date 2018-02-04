using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Provider.Handlers;

namespace Providers.Handlers
{
    public abstract class BindingHandler
    {
        public abstract bool Subscribe(InputSubscriptionRequest subReq);

        public abstract bool Unsubscribe(InputSubscriptionRequest subReq);

        public abstract void Poll(int pollValue);
    }
}

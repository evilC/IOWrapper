using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Provider.Handlers;
using System.Collections.Concurrent;

namespace Providers.Handlers
{
    public abstract class BindingHandler
    {
        //private int _index;
        //private int _subIndex = 0;

        protected ConcurrentDictionary<int, 
            ConcurrentDictionary<int, SubscriptionHandler>> _bindingDictionary
            = new ConcurrentDictionary<int, ConcurrentDictionary<int, SubscriptionHandler>>();

        public virtual SubscriptionHandler GetSubscriptionHandler(InputSubscriptionRequest subReq)
        {
            return _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Index, new ConcurrentDictionary<int, SubscriptionHandler>())
                .GetOrAdd(subReq.BindingDescriptor.SubIndex, new SubscriptionHandler());
        }

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            return GetSubscriptionHandler(subReq).Subscribe(subReq);
        }
        //public abstract bool Subscribe(InputSubscriptionRequest subReq);

        //public abstract bool Unsubscribe(InputSubscriptionRequest subReq);
        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return GetSubscriptionHandler(subReq).Unsubscribe(subReq);
        }

        //public abstract void Poll(int pollValue);
        public virtual void Poll(int pollValue)
        {
            foreach (var indexDictionary in _bindingDictionary)
            {
                foreach (var subscriptionHandler in indexDictionary.Value)
                {
                    subscriptionHandler.Value.State = pollValue;
                }
            }
            //_bindingDictionary.State = pollValue;
        }

    }
}

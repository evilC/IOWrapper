using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Provider.Handlers;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Providers.Handlers
{
    //Handles bindings for a given Index and SubIndex
    public class BindingHandler
    {
        protected ConcurrentDictionary<int, // SubIndex
                SubscriptionHandler> _bindingDictionary    // Handler
            = new ConcurrentDictionary<int, SubscriptionHandler>();

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            return _bindingDictionary
                .GetOrAdd(GetKeyFromSubIndex(subReq.BindingDescriptor.SubIndex), new SubscriptionHandler())
                .Subscribe(subReq);
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var k = subReq.BindingDescriptor.SubIndex;
            if (_bindingDictionary.ContainsKey(k))
            {
                return _bindingDictionary[k].Unsubscribe(subReq);
            }

            return false;
        }

        public virtual void Poll(int pollValue)
        {
            foreach (var subscriptionHandler in _bindingDictionary.Values)
            {
                subscriptionHandler.State = pollValue;
            }
        }

        // Allows overriding of the key value used for a given SubIndex
        public virtual int GetKeyFromSubIndex(int subIndex)
        {
            return subIndex;
        }
    }
}

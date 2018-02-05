using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Providers.Handlers
{
    public class BindingHandler
    {
        protected readonly BindingDescriptor BindingDescriptor;

        protected ConcurrentDictionary<int, // SubIndex
                SubscriptionHandler> _bindingDictionary    // Handler
            = new ConcurrentDictionary<int, SubscriptionHandler>();

        public BindingHandler(InputSubscriptionRequest subReq)
        {
            BindingDescriptor = subReq.BindingDescriptor;
        }

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
                if (_bindingDictionary[k].Unsubscribe(subReq))
                {
                    if (_bindingDictionary[k].IsEmpty())
                    {
                        _bindingDictionary.TryRemove(k, out _);
                    }
                    return true;
                }
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

        public virtual bool IsEmpty()
        {
            return _bindingDictionary.IsEmpty;
        }
    }
}

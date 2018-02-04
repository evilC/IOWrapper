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
    //Handles bindings for a given Index
    public class BindingHandler
    {
        protected Guid tmpGuid = Guid.NewGuid();

        protected ConcurrentDictionary<int, // SubIndex
                SubscriptionHandler> _bindingDictionary    // Handler
            = new ConcurrentDictionary<int, SubscriptionHandler>();

        //public virtual SubscriptionHandler CreateAndGetSubscriptionHandler(InputSubscriptionRequest subReq)
        //{
        //    return _bindingDictionary
        //        .GetOrAdd(subReq.BindingDescriptor.SubIndex, new SubscriptionHandler());
        //}

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            Debug.WriteLine($"IOWrapper| Guid: {tmpGuid}, Index: {subReq.BindingDescriptor.Index}, SubIndex: {subReq.BindingDescriptor.SubIndex}");
            return _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.SubIndex, new SubscriptionHandler())
                .Subscribe(subReq);

            //return CreateAndGetSubscriptionHandler(subReq).Subscribe(subReq);
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

    }
}

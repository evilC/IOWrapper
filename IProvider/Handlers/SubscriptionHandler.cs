using System;
using System.Collections.Concurrent;

namespace Providers.Handlers
{
    public class SubscriptionHandler
    {
        private int _state;
        private ConcurrentDictionary<Guid, InputSubscriptionRequest> _subscriptions = 
            new ConcurrentDictionary<Guid, InputSubscriptionRequest>();

        public int State
        {
            get { return _state; }
            set
            {
                _state = value;
                foreach (var subscriptionRequest in _subscriptions)
                {
                    subscriptionRequest.Value.Callback(_state);
                }
            }
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _subscriptions[subReq.SubscriptionDescriptor.SubscriberGuid] = subReq;
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return _subscriptions.TryRemove(subReq.SubscriptionDescriptor.SubscriberGuid, out _);
        }

        public bool IsEmpty()
        {
            return _subscriptions.IsEmpty;
        }
    }
}
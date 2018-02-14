using System;
using System.Collections.Concurrent;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Handlers
{
    /// <summary>
    /// Handles storing of subscriptions for a given input, and handling of callbacks
    /// 
    /// Indexes by SubscriberGuid from the <see cref="SubscriptionDescriptor"/>
    /// </summary>
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
                //Set _state IMMEDIATELY, else another callback may get queued!
                _state = value;
                // This is where change in state of an input generates a callback
                foreach (var subscriptionRequest in _subscriptions.Values)
                {
                    ThreadPool.QueueUserWorkItem(threadProc => subscriptionRequest.Callback(value));
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
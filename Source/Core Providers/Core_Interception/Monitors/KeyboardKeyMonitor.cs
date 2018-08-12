using System;
using System.Collections.Generic;
using System.Threading;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class KeyboardKeyMonitor
    {
        private readonly Dictionary<Guid, InputSubscriptionRequest> _subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

        public void Add(InputSubscriptionRequest subReq)
        {
            _subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
        }

        public void Remove(InputSubscriptionRequest subReq)
        {
            _subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
        }

        public bool HasSubscriptions()
        {
            return _subReqs.Count > 0;
        }

        public bool Poll(ushort state)
        {
            foreach (var subscriptionRequest in _subReqs.Values)
            {
                ThreadPool.QueueUserWorkItem(cb => subscriptionRequest.Callback(state));
            }

            return true;
        }
    }
}

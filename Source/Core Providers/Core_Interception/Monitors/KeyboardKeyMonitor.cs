using System;
using System.Collections.Generic;
using System.Threading;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class KeyboardKeyMonitor
    {
        private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

        public void Add(InputSubscriptionRequest subReq)
        {
            subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
        }

        public void Remove(InputSubscriptionRequest subReq)
        {
            subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
        }

        public bool HasSubscriptions()
        {
            return subReqs.Count > 0;
        }

        public bool Poll(ushort state)
        {
            foreach (var subscriptionRequest in subReqs.Values)
            {
                ThreadPool.QueueUserWorkItem(cb => subscriptionRequest.Callback(state));
            }

            return true;
        }
    }
}

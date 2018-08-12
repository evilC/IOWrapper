using System;
using System.Collections.Generic;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class MouseAxisMonitor
    {
        private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();
        public int MonitoredAxis { get; set; }

        public void Add(InputSubscriptionRequest subReq)
        {
            subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
            //Log("Added Subscription to Mouse Button {0}", subReq.InputIndex);
        }

        public void Remove(InputSubscriptionRequest subReq)
        {
            subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
        }

        public bool HasSubscriptions()
        {
            return subReqs.Count > 0;
        }

        public bool Poll(int value)
        {
            foreach (var subscriptionRequest in subReqs.Values)
            {
                ThreadPool.QueueUserWorkItem(cb => subscriptionRequest.Callback(value));
            }

            return true;
        }
    }
}

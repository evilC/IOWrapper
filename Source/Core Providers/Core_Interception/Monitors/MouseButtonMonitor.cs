using System;
using System.Collections.Generic;
using System.Threading;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class MouseButtonMonitor
    {
        private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();
        private int _index = -1;

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

        public bool Poll(int state)
        {
            foreach (var subscriptionRequest in subReqs.Values)
            {
                //Log("State: {0}", MonitoredState);
                ThreadPool.QueueUserWorkItem(cb => subscriptionRequest.Callback(state));
                if (subscriptionRequest.BindingDescriptor.Index > 4)
                {
                    // Wheel - simulate release
                    ThreadPool.QueueUserWorkItem(cb => subscriptionRequest.Callback(0));
                }
            }

            return true;
        }
    }
}
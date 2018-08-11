using System;
using System.Collections.Generic;
using System.Threading;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class MouseButtonMonitor
    {
        public int MonitoredState { get; set; }

        private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

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

        public bool Poll(ManagedWrapper.Stroke stroke)
        {
            bool block = false;
            if ((stroke.mouse.state & (ushort) ManagedWrapper.Filter.MouseButtonAny) != 0)
            {
                block = true;
                foreach (var subscriptionRequest in subReqs.Values)
                {
                    //Log("State: {0}", MonitoredState);
                    ThreadPool.QueueUserWorkItem(
                        new InterceptionCallback {subReq = subscriptionRequest, value = MonitoredState}
                            .FireCallback
                    );
                }
            }

            return block;
        }

        class InterceptionCallback
        {
            public InputSubscriptionRequest subReq;
            public int value;

            public void FireCallback(Object state)
            {
                subReq.Callback(value);
            }
        }
    }
}
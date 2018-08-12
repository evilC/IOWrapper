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
        private readonly Dictionary<Guid, InputSubscriptionRequest> _subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

        public void Add(InputSubscriptionRequest subReq)
        {
            _subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
            //Log("Added Subscription to Mouse Button {0}", subReq.InputIndex);
        }

        public void Remove(InputSubscriptionRequest subReq)
        {
            _subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
        }

        public bool HasSubscriptions()
        {
            return _subReqs.Count > 0;
        }

        public bool Poll(int state)
        {
            foreach (var subscriptionRequest in _subReqs.Values)
            {
                //Log("State: {0}", MonitoredState);
                ThreadPool.QueueUserWorkItem(cb => subscriptionRequest.Callback(state));
                //if (subscriptionRequest.BindingDescriptor.Index > 4)
                //{
                //    // Wheel - simulate release
                //    ThreadPool.QueueUserWorkItem(cb => DelayWheelRelease(subscriptionRequest));
                //}
            }

            return true;
        }

        //// Delays Mouse Wheel Release, so that if a user maps it to a key, the game's poll loop has a decent chance of seeing the key change state
        //public void DelayWheelRelease(InputSubscriptionRequest subscriptionRequest)
        //{
        //    Thread.Sleep(50);
        //    subscriptionRequest.Callback(0);
        //}
    }
}
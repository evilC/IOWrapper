using System;
using System.Collections.Generic;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class KeyboardKeyMonitor
    {
        public ushort code;
        public ushort stateDown;
        public ushort stateUp;

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

        public bool Poll(ManagedWrapper.Stroke stroke)
        {
            var isDown = stateDown == stroke.key.state;
            var isUp = stateUp == stroke.key.state;
            bool block = false;
            if (code == stroke.key.code && (isDown || isUp))
            {
                block = true;
                foreach (var subscriptionRequest in subReqs.Values)
                {
                    subscriptionRequest.Callback(isDown ? 1 : 0);
                    //Log("State: {0}", isDown);
                }
            }

            return block;
        }
    }
}

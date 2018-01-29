using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    /// <summary>
    /// Handles subscribing / unsubscribing to Bindings.
    /// </summary>
    public abstract class BindingHandler
    {
        protected Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
        protected BindingDescriptor bindingDescriptor;
        protected int currentState = 0;

        private static int povTolerance = 4500;
        protected int povAngle;

        public BindingHandler(BindingDescriptor descriptor)
        {
            bindingDescriptor = descriptor;
            if (bindingDescriptor.Type == BindingType.POV)
            {
                povAngle = bindingDescriptor.SubIndex * 9000;
            }
        }

        public virtual bool ProfileIsActive(Guid profileGuid)
        {
            return true;
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var subscriberGuid = subReq.SubscriptionDescriptor.SubscriberGuid;
            if (!subscriptions.ContainsKey(subscriberGuid))
            {
                subscriptions.Add(subscriberGuid, subReq);
                return true;
            }
            return false;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var subscriberGuid = subReq.SubscriptionDescriptor.SubscriberGuid;
            if (subscriptions.ContainsKey(subscriberGuid))
            {
                subscriptions.Remove(subscriberGuid);
                return true;
            }
            return false;
        }

        public abstract void ProcessPollResult(int state);

        public bool HasSubscriptions()
        {
            return subscriptions.Count > 0;
        }

        protected bool ValueMatchesAngle(int value, int angle)
        {
            if (value == -1)
                return false;
            var diff = AngleDiff(value, angle);
            return value != -1 && AngleDiff(value, angle) <= povTolerance;
        }

        private int AngleDiff(int a, int b)
        {
            var result1 = a - b;
            if (result1 < 0)
                result1 += 36000;

            var result2 = b - a;
            if (result2 < 0)
                result2 += 36000;

            return Math.Min(result1, result2);
        }
    }

    /// <summary>
    /// Base class for Polled Binding Handlers to derive from
    /// Processes poll results and decides whether to fire the callbacks
    /// </summary>
    public abstract class PolledBindingHandler : BindingHandler
    {
        public PolledBindingHandler(BindingDescriptor descriptor) : base(descriptor)
        {
        }

        public override void ProcessPollResult(int state)
        {
            int reportedValue = ConvertValue(state);
            if (currentState == reportedValue)
                return;
            currentState = reportedValue;

            foreach (var subscription in subscriptions.Values)
            {
                //if (ProfileIsActive(subscription.SubscriptionDescriptor.ProfileGuid))
                //{
                    subscription.Callback(reportedValue);
                //}
            }
        }

        public virtual int ConvertValue(int state)
        {
            return state;
        }
    }
}

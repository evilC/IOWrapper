using Providers;
using Providers.Handlers;

namespace SharpDX_XInput
{
    class XiBindingHandler : BindingHandler
    {
        private SubscriptionHandler subscriptionHandler = new SubscriptionHandler();

        public override void Poll(int pollValue)
        {
            subscriptionHandler.State = pollValue;
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            return subscriptionHandler.Subscribe(subReq);
        }

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return subscriptionHandler.Unsubscribe(subReq);
        }
    }
}
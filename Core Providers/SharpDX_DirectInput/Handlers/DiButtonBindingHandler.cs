using Providers;
using Providers.Handlers;

namespace SharpDX_DirectInput
{
    class DiButtonBindingHandler : BindingHandler
    {
        private SubscriptionHandler subscriptionHandler = new SubscriptionHandler();

        public override void Poll(int pollValue)
        {
            subscriptionHandler.State = pollValue == 128 ? 1 : 0;
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